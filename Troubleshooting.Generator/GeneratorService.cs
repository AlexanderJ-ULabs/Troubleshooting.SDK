#region header

// Troubleshooting.Calculator - CalculatorService.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 12:38 AM

#endregion

#region using

using System.Composition;
using System.Threading.Tasks;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using Serilog;
using Troubleshooting.Common.Messaging;
using Troubleshooting.Common.Services;

#endregion

#region Warning Explanation

//     This program is not using true async. The Actor Model prevents race conditions
//     and guarantees thread safety by processing all messages concurrently. This
//     allows us to maximize available CPU resources by assigning tasks to a pool of
//     threads through PostSharp's implementaiton of the Actor Model.
//     We do not need this warning.
//

#endregion

#pragma warning disable 1998

namespace Troubleshooting.Generator
{
    /// <summary>
    ///     This service exists to generate random integers and send them out to other actors.
    /// </summary>
    [Export(typeof(IService))]
    [Actor]
    public class GeneratorService : IService
    {
        #region Private Methods

        /// <summary>
        ///     Just a method to make sure that the main loop is working.
        /// </summary>
        private async void TestRefresh()
        {
            await Task.Delay(7000);
            var msg = provider.CreateMessage("Generator is still alive.");
            await provider.PostMessage(msg);
        }

        #endregion

        #region Properties & Fields

        /// <summary>
        ///     Private reference back to the service provider.
        /// </summary>
        [Reference] private ICoreService provider;

        /// <summary>
        ///     Private reference back to the logger.
        /// </summary>
        [Reference]
        private ILogger log { get; set; }

        /// <inheritdoc />
        public string Name => "GeneratorService";

        #endregion

        #region Public Entry-Point Methods

        /// <inheritdoc />
        [Reentrant]
        public async Task<bool> Initialize(ICoreService core)
        {
            provider = core;

            log = core.Logger;

            return true;
        }

        /// <inheritdoc />
        [Reentrant]
        public async Task HandleMessage(dynamic message)
        {
            switch (message.Topic)
            {
                case Topics.Hello:
                    log.Information("Hello from the generator!");
                    TestRefresh();

                    break;
            }
        }

        #endregion
    }
}