#region header

// Troubleshooting.Calculator - CalculatorService.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 12:38 AM

#endregion

#region using

using System;
using System.Composition;
using System.Threading.Tasks;
using PostSharp.Extensibility;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;

using Serilog;

using Troubleshooting.Common;
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

namespace Troubleshooting.Decisions
{
    /// <summary>
    ///     This service exists to generate random computation operations of the types:
    ///     ADD, SUB, MUL, DIV.
    ///     These are then sent out to the other actors.
    /// </summary>
    [Export(typeof(Common.Services.IService))]
    [Actor]
    public class DecisionsService : Common.Services.IService
    {
        #region Properties & Fields

        /// <summary>
        ///     Private reference back to the service provider.
        /// </summary>
        [Reference]
        private ICoreService provider;

        /// <summary>
        ///     Private reference back to the logger.
        /// </summary>
        [Reference] private ILogger log { get; set; }

        /// <inheritdoc />
        public string Name => "DecisionsService";

        #endregion

        #region Public Entry-Point Methods

        /// <inheritdoc />
        [Reentrant]
        public async Task<bool> Initialize(ICoreService core)
        {
            this.provider = core;

            this.log = core.Logger;

            return true;
        }

        /// <inheritdoc />
        [Reentrant]
        public async Task HandleMessage(dynamic message)
        {
            switch (message.Topic)
            {
                case Topics.Hello:
                    log.Information("Hello from the decisions!");
                    this.TestRefresh();

                    break;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Just a method to make sure that the main loop is working.
        /// </summary>
        private async void TestRefresh()
        {
            await Task.Delay(6250);
            var msg = this.provider.CreateMessage("Decisions is still alive.");
            await this.provider.PostMessage(msg);
        }

        #endregion
    }
}
