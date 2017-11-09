#region header

// Troubleshooting.Common - ICoreService.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 9:09 PM

#endregion

#region using

using System.Threading.Tasks;
using Serilog;

#endregion

namespace Troubleshooting.Common.Services
{
    public interface ICoreService
    {
        /// <summary>
        ///     Creates a message to be sent out to Actors.
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        dynamic CreateMessage(string topic);

        /// <summary>
        ///     Sends a message out to Actors.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PostMessage(dynamic message);

        /// <summary>
        ///     Holds a reference to the logger from the program entry point.
        /// </summary>
        ILogger Logger { get; set; }
    }
}