#region header

// Troubleshooting.SDK - Program.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 9:09 PM

#endregion

#region using

using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Troubleshooting.Common.Messaging;
using Console = Colorful.Console;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Troubleshooting.SDK.Services;

#endregion

namespace Troubleshooting.SDK
{
    /// <summary>
    ///     This is a <see cref="Console"/> host which acts as an entry-point and REPL for services.
    /// </summary>
    internal class Program
    {
        #region Properties & Fields

        /// <summary>
        ///     This allows the application to indefinitely wait until exit is requested.
        /// </summary>
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        /// <summary>
        ///     Handles the loading of services and manages the Actor Model implementation.
        /// </summary>
        private static Provider ServiceProvider { get; set; }

        /// <summary>
        ///     Stores the reference to the logger so that it may be passed to the service provider.
        /// </summary>
        private static ILogger Logger { get; set; }

        /// <summary>
        ///     Utilizing the Colorful Console package and this class to standardize message colors.
        /// </summary>
        private static readonly Colors Messaging = new Colors();

        #endregion

        #region Main

        /// <summary>
        ///     Entry point for the application, holding the original reference to service provider and logger.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args = null)
        {
            //  Prepare the quit event so that application can loop when setup is complete.
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            Console.WriteLine("hello-world: Troubleshooting.SDK entry-point reached.", Messaging.Info);

            Initialize();

            Console.WriteLine("hello-world: Troubleshooting.SDK end-point reached.", Messaging.Info);
            ServiceProvider.Stop();
            Log.CloseAndFlush();

            Console.WriteLine("hello-world: Press any key to quit.", Messaging.Error);
            Console.ReadLine();
        }

        #endregion

        #region Static Initializers

        /// <summary>
        ///     Basic setup procedures to call a service provider and logger.
        /// </summary>
        private static void Initialize()
        {
            Logger = SetupLogging();
            LoadServices(Logger);

            //  Wait for quit.
            Logger.Debug("You've now entered the main loop. Press CTRL+C or issue a cancel command to exit.");
            _quitEvent.WaitOne();
        }

        /// <summary>
        ///     Loads services by creating a new provider and passing it a reference to the logger.
        /// </summary>
        /// <param name="log"></param>
        private static void LoadServices(ILogger log)
        {
            ServiceProvider = new Provider(log);
            ServiceProvider.ConfigureServices();
            ServiceProvider.StartServices();

            //  A greeting message to inform services that the host is done with preparations.
            dynamic hello = ServiceProvider.CreateMessage(Topics.Hello);
            ServiceProvider.PostMessage(hello);
        }

        /// <summary>
        ///     Currently using Serilog through the Microsoft.Extensions.Logging abstraction library.
        /// </summary>
        private static ILogger SetupLogging()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.LiterateConsole(
                        outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level,-11}] {Message}{NewLine}{Exception}")
                .WriteTo.RollingFile("log-{Date}.txt",
                    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level,-11}] {Message}{NewLine}{Exception}")
                .CreateLogger();
        }

        #endregion
    }
}