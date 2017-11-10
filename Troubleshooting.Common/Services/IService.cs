#region header

// Troubleshooting.Common - IService.cs
// 
// Copyright Untethered Labs, Inc.  All rights reserved.
// 
// Created: 2017-11-08 9:09 PM

#endregion

#region using

using System.Threading.Tasks;

#endregion

namespace Troubleshooting.Common.Services
{
    public interface IService
    {
        /// <summary>
        ///     Identifies the service.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Kicks off initialization and setup procedures for a service after it has been loaded.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        Task<bool> Initialize(ICoreService core);

        /// <summary>
        ///     Allows the service to participate as an Actor to receive messages from other services.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task HandleMessage(dynamic message);
    }
}