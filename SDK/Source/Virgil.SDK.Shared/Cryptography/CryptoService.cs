#region Copyright (C) 2016 Virgil Security Inc.
// Copyright (C) 2015-2016 Virgil Security Inc.
// 
// Lead Maintainer: Virgil Security Inc. <support@virgilsecurity.com>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions 
// are met:
// 
//   (1) Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.
//   
//   (2) Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in
//   the documentation and/or other materials provided with the
//   distribution.
//   
//   (3) Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived 
//   from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR ''AS IS'' AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
// IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace Virgil.SDK.Cryptography
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    public class CryptoService
    {
        public byte[] Encrypt(byte[] data, IEnumerable<IRecipient> recipients)
        {
            throw new NotImplementedException();
        }

        public Stream Encrypt(Stream stream, IEnumerable<IRecipient> recipients)
        {
            throw new NotImplementedException();
        }
        
        public bool Verify(byte[] data, byte[] signature, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }

        public bool Verify(Stream stream, byte[] signature, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }
    }

    public interface ISecurityModule
    {
        byte[] Decrypt(byte[] cipherdata);

        Stream Decrypt(Stream cipherstream);

        byte[] Sign(byte[] data);

        byte[] Sign(Stream cipherstream);
    }

    public class SecurityModule : ISecurityModule
    {
        private readonly SecurityModuleParameters parameters;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityModule" /> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public SecurityModule(SecurityModuleParameters parameters)
        {
            this.parameters = parameters;
        }

        public byte[] Decrypt(byte[] cipherdata)
        {
            throw new NotImplementedException();
        }

        public Stream Decrypt(Stream cipherstream)
        {
            throw new NotImplementedException();
        }

        public byte[] Sign(byte[] data)
        {
            throw new NotImplementedException();
        }

        public byte[] Sign(Stream cipherstream)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a security module parameters.
    /// </summary>
    public class SecurityModuleParameters
    {
        /// <summary>
        /// Gets or sets the name of the module.
        /// </summary>
        public string ModuleName { get; set; }
    }
}