﻿#region Copyright (C) Virgil Security Inc.
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
    
namespace Virgil.SDK.Client
{
    using System.Collections.Generic;

    /// <summary>
    /// The search criteria that determines what cards list to retrieve.
    /// </summary>
    public class SearchCriteria 
    {
        /// <summary>
        /// Gets or sets the identities.
        /// </summary>
        public IEnumerable<string> Identities { get; set; }

        /// <summary>
        /// Gets or sets the type of the identity.
        /// </summary>
        public string IdentityType { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        public CardScope Scope { get; set; }

        public static SearchCriteria ByIdentities(params string[] identities)
        {
            return new SearchCriteria
            {
                Identities = identities,
                Scope = CardScope.Application
            };
        }

        public static SearchCriteria ByIdentity(string identity)
        {
            return new SearchCriteria
            {
                Identities = new[] { identity },
                Scope = CardScope.Application
            };
        }

        public static SearchCriteria ByAppBundle(string bundle)
        {
            return new SearchCriteria
            {
                Identities = new[] { bundle },
                IdentityType = "application",
                Scope = CardScope.Global
            };
        }
    }
}