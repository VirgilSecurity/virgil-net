﻿namespace Virgil.SDK.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using FizzWare.NBuilder;
    using FluentAssertions;

    using Newtonsoft.Json;
    using NUnit.Framework;

    using Virgil.SDK.Client;
    using Virgil.SDK.Client.Models;
    using Virgil.SDK.Cryptography;

    public class CreateCardRequestTests
    {
        [Test]
        public void Create_GivenIdentityAndTypeAndPublicKey_ShouldReturnCanonicalRequestWithPassedParameters()
        {
            var crypto = new VirgilCrypto();

            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            const string identity = "Alice";
            const string identityType = "member";

            var reqiest = CreateCardRequest.Create(identity, identityType, exportedPublicKey);
            
            var requestJson = Encoding.UTF8.GetString(reqiest.Snapshot);
            var requestModel = JsonConvert.DeserializeObject<CardRequestModel>(requestJson);
            
            requestModel.Identity.ShouldBeEquivalentTo(identity);
            requestModel.IdentityType.ShouldBeEquivalentTo(identityType);
            requestModel.PublicKey.ShouldBeEquivalentTo(exportedPublicKey);
        }

        [Test]
        public void Create_GivenCardRequestModel_ShouldReturnCanonicalRequestEquivalentToPassedModel()
        {
            var crypto = new VirgilCrypto();
            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            var originalRequestModel = Builder<CardRequestModel>.CreateNew()
                .With(it => it.PublicKey = exportedPublicKey)
                .Build();
            
            var request = CreateCardRequest.Create(originalRequestModel);

            var requestJson = Encoding.UTF8.GetString(request.Snapshot);
            var requestModel = JsonConvert.DeserializeObject<CardRequestModel>(requestJson);

            originalRequestModel.ShouldBeEquivalentTo(requestModel);
        }

        [Test]
        public void Create_GivenAnyParameters_ShouldCreateRequestWithApplicationScope()
        {
            var crypto = new VirgilCrypto();

            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            const string identity = "Alice";
            const string identityType = "member";

            var request = CreateCardRequest.Create(identity, identityType, exportedPublicKey);

            request.Scope.Should().Be(VirgilCardScope.Application);
        }

        [Test]
        public void CreateGlobal_GivenAnyParameters_ShouldCreateRequestWithGlobalScope()
        {
            var crypto = new VirgilCrypto();

            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            const string identity = "Alice";
            var request = CreateCardRequest.CreateGlobal(identity, exportedPublicKey);

            request.Scope.Should().Be(VirgilCardScope.Global);
        }

        [Test]
        public void Export_WithoutParameters_ShouldReturnStringRepresentationOfRequest()
        {
            var crypto = new VirgilCrypto();

            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            const string identity = "alice@virgilsecurity.com";

            var request = CreateCardRequest.CreateGlobal(identity, exportedPublicKey);

            var fingerprint = crypto.CalculateFingerprint(request.Snapshot);
            var signature = crypto.SignFingerprint(fingerprint, privateKey);

            request.AppendSignature(fingerprint, signature);

            var exportedRequest = request.Export();

            var jsonData = Convert.FromBase64String(exportedRequest);
            var json = Encoding.UTF8.GetString(jsonData);

            var model = JsonConvert.DeserializeObject<SignedRequestModel>(json);

            model.RequestSnapshot.ShouldBeEquivalentTo(request.Snapshot);
            model.Meta.Signs.ShouldAllBeEquivalentTo(request.Signs);
        }

        [Test]
        public void Export_WithoutParameters_ShouldBeEquivalentToImportedRequest()
        {
            var crypto = new VirgilCrypto();

            var privateKey = crypto.GenerateKey();
            var exportedPublicKey = crypto.ExportPublicKey(privateKey);

            const string identity = "alice@virgilsecurity.com";

            var request = CreateCardRequest.CreateGlobal(identity, exportedPublicKey, 
                data: new Dictionary<string, string>
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2"
                });

            var fingerprint = crypto.CalculateFingerprint(request.Snapshot);
            var signature = crypto.SignFingerprint(fingerprint, privateKey);

            request.AppendSignature(fingerprint, signature);

            var exportedRequest = request.Export();
            var importedRequest = CreateCardRequest.Import(exportedRequest);

            importedRequest.ShouldBeEquivalentTo(request);
        }
    }
}