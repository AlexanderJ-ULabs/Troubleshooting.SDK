#region header

// Troubleshooting.Calculator - CalculatorService.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 7:18 PM

#endregion

#region using

using System;
using System.Composition;
using System.Threading.Tasks;

using PostSharp.Patterns.Diagnostics;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;

using Serilog;

using Troubleshooting.Common;
using Troubleshooting.Common.Messaging;
using Troubleshooting.Common.Services;

#endregion

#pragma warning disable 1998

namespace Troubleshooting.Calculator
{
    /// <summary>
    ///     This service exists to run simple calculation requests received from other services.
    /// </summary>
    [Export (typeof (IService))]
    [Actor]
    public class CalculatorService : IService
    {
        /// <summary>
        ///     Private backreference to the service provider.
        /// </summary>
        [Reference]
        private ICoreService provider;

        [Reference] private ILogger log { get; set; }

        /// <inheritdoc />
        public string Name => "CalculatorService";

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
                    log.Information("Hello from the calculator!");
                    this.TestRefresh();

                    break;
            }
        }

        /// <summary>
        ///     Just a method to make sure that the main loop is working.
        /// </summary>
        private async void TestRefresh()
        {
            await Task.Delay(5000);
            var msg = this.provider.CreateMessage("Calculator is still alive.");
            await this.provider.PostMessage(msg);
        }
    }
}
