#region header

// Troubleshooting.SDK - Provider.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 7:41 PM

#endregion

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Threading;

using Serilog;

using Troubleshooting.Common.Services;

#endregion

namespace Troubleshooting.SDK.Services
{
    /// <summary>
    ///     The provider will configure, load, and manage all services. It is also responsible for passing messages to them.
    /// </summary>
    internal class Provider : ICoreService
    {
        /// <summary>
        ///     Get the assembly so it can be reflected upon.
        /// </summary>
        private readonly Assembly provider = typeof(Provider).GetTypeInfo().Assembly;

        /// <summary>
        ///     The strong name key token identifying programs which are signed by us.
        /// </summary>
        private static readonly byte[] OfficialKeyToken = {0xB2, 0x25, 0xB9, 0x3, 0x33, 0x57, 0x5A, 0xCD};

        /// <summary>
        ///     A list of all services located by this provider.
        /// </summary>
        internal IEnumerable<IService> Services { get; private set; }

        /// <inheritdoc />
        public ILogger Logger { get; set; }

        /// <summary>
        ///     Constructs the Provider and enables message tracing to a <see cref="Console"/> type host.
        /// </summary>
        /// <param name="log">Conforms to the <see cref="ILogger"/> interface and will be passed to all services.</param>
        internal Provider(ILogger log)
        {
            Logger = log;

            // Enable message output to a console type host.
            EnableMessageTracing();
        }

        /// <summary>
        ///     Responsible for calling <see cref="IService.Initialize"/> on the services and passing in a reference to this provider.
        /// </summary>
        internal void StartServices()
        {
            foreach (IService serv in this.Services)
            {
                if (!serv.Initialize(this).Result)
                    Logger.Fatal("kill-service: {0} failed to initialize.", serv.Name);
            }
        }

        /// <summary>
        ///     Responsible for loading all available services.
        /// </summary>
        internal void ConfigureServices()
        {
            ContainerConfiguration asmConfig = new ContainerConfiguration().WithAssemblies(this.LoadServiceAssemblies());

            using (CompositionHost container = asmConfig.CreateContainer())
                this.Services = container.GetExports<IService>();

            foreach (IService serv in this.Services)
                Logger.Information("Loaded service: {0}", serv.Name);
        }
        
        /// <summary>
        ///     A method to scan the contents of the service provider's directory to locate available services.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Assembly> LoadServiceAssemblies()
        {
            var servicePath = Path.GetDirectoryName(provider.Location);

            //  Include all sub-assemblies but exclude SDK and Topics.
            var assemblies =
                Directory.GetFiles(servicePath, "Troubleshooting.*.dll", SearchOption.AllDirectories)
                    .Where(x => Regex.IsMatch(x, @"Troubleshooting\.(?!SDK|Common)\w*\.dll",
                        RegexOptions.Compiled | RegexOptions.IgnoreCase));

            foreach (Assembly asm in assemblies.Select(Assembly.LoadFrom))
            {
                if (!asm.GetName().GetPublicKeyToken().SequenceEqual(OfficialKeyToken))
                    Logger.Fatal("kill-program: security violation detected.");

                Logger.Information("load-microservice: {0} successfully added.", asm.GetName().Name);
                yield return asm;
            }
        }

        /// <inheritdoc />
        public dynamic CreateMessage([System.ComponentModel.DataAnnotations.Required]string topic)
        {
            dynamic msg = new ExpandoObject();
            msg.Timestamp = DateTime.Now;
            msg.Topic = topic;
            return msg;
        }

        /// <inheritdoc />
        [ExplicitlySynchronized]
        public Task PostMessage([NotNull] dynamic message)
        {
            Task[] tl = this.PostMessageToServices(message);

            Task.WaitAll(tl);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Allows actions to be performed on the message chain.
        /// </summary>
        private event Action<dynamic> MessageChain;

        /// <summary>
        ///     Invokes the message chain with a message wrapped in potential actions.
        /// </summary>
        /// <param name="message">A dynamic message created by <see cref="ICoreService.CreateMessage"/> method.</param>
        /// <returns></returns>
        private Task[] PostMessageToServices(dynamic message)
        {
            // Invoke the message chain.
            this.MessageChain?.Invoke(message);

            List<Task> tasks = new List<Task>();

            foreach (IService serv in this.Services)
                tasks.Add(serv.HandleMessage(message));

            return tasks.ToArray();
        }

        /// <summary>
        ///     Adds the LogMessage action to <see cref="MessageChain"/> in order to print messages to a host of type <see cref="Console"/>.
        /// </summary>
        private void EnableMessageTracing()
        {
            this.MessageChain += this.LogMessage;
        }

        /// <summary>
        ///     Removes the LogMessage action from <see cref="MessageChain"/>.
        /// </summary>
        private void DisableMessageTracing()
        {
            this.MessageChain -= this.LogMessage;
        }

        /// <summary>
        ///     Wraps a message in a string builder and outputs it to the <see cref="Console"/> type host before sending it out to all Actors.
        /// </summary>
        /// <param name="message"></param>
        private void LogMessage(dynamic message)
        {
            var entry = new StringBuilder($"{message.Topic} {{");

            // Casting the properties of message for the sake of revealing a Count property.
            var props = new KeyValuePair <string, object>[((IDictionary <string, object>) message).Count];

            // Append the properties
            ((IDictionary<string, object>) message).CopyTo(props, 0);

            // Append the props except for the basic ones using JSON syntax.
            var subsequent = false;
            foreach (KeyValuePair<string, object> prop in props
                .Where(x => (x.Key != "Timestamp") && (x.Key != "Topic")))
            {
                if (subsequent)
                    entry.Append(", ");

                switch (prop.Value)
                {
                    case Array arr:
                    {
                        entry.Append($"{prop.Key}: ");

                        if (arr.Length == 0)
                            entry.Append("[]");
                        else
                        {
                            var subsqarr = false;

                            entry.Append("[");

                            foreach (object o in arr)
                            {
                                if (subsqarr)
                                    entry.Append(", ");

                                entry.Append(o);

                                subsqarr = true;
                            }

                            entry.Append("]");
                        }

                        break;
                    }

                    default:
                        entry.Append($"{prop.Key}: {prop.Value}");
                        break;
                }

                subsequent = true;
            }

            //  Finish off and write.
            entry.Append("}");

            //  Post message to the general log.
            Logger.Debug(entry.ToString());
        }

    }
}