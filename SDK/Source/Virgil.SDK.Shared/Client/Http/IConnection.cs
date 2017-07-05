﻿namespace Virgil.SDK.Client.Http
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A connection for making HTTP requests against URI endpoints.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Base address for the connection.
        /// </summary>
        Uri BaseAddress { get; }

        /// <summary>
        /// Sends an HTTP request to the API.
        /// </summary>
        /// <param name="request">The HTTP request details.</param>
        /// <param name="ignoreError">if set to <c>true</c> [ignore error].</param>
        /// <returns></returns>
        Task<IResponse> Send(IRequest request, bool ignoreError = false);
    }
}