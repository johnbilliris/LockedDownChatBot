﻿using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.SemanticKernel;

namespace LockedDownBotSemanticKernel.Primitives;

public class SemanticKernelWrapperFactory
{
    private SemanticKernelWrapper? _client;

    public SemanticKernelWrapper GetFromSettings(IDictionary<string, object> config)
    {
        var endpoint = (string)config["OPENAI_ENDPOINT"];
        var model = (string)config["OPENAI_MODEL"];
        var key = config["OPENAI_KEY"] as string;
        var clientId = config["OPENAI_MANAGED_IDENTITY_CLIENT_ID"] as string;
        return GetFromSettings(endpoint, key, clientId, model);
    }

    public SemanticKernelWrapper GetFromSettings(string endpoint, string? key, string? clientId, string model)
    {
        if (_client == null)
        {
            var useManagedIdentity = string.IsNullOrWhiteSpace(key);

            _client = useManagedIdentity
                ? new SemanticKernelWrapper(new KernelBuilder()
                    .WithAzureChatCompletionService(model, endpoint, new ManagedIdentityCredential(clientId)).Build())
                : new SemanticKernelWrapper(new KernelBuilder().WithAzureChatCompletionService(model, endpoint, key!)
                    .Build());

            _client.ImportSkills(typeof(SemanticKernelWrapperFactory).Assembly);
        }

        return _client;
    }

    public OpenAIClient GetRawClientFromSettings(IDictionary<string, object> config, out string model,
        out string embeddingModel)
    {
        var endpoint = (string)config["OPENAI_ENDPOINT"];
        var key = config["OPENAI_KEY"] as string;
        var clientId = config["OPENAI_MANAGED_IDENTITY_CLIENT_ID"] as string;
        model = (string)config["OPENAI_MODEL"];
        embeddingModel = (string)config["OPENAI_EMBEDDING_MODEL"];

        var useManagedIdentity = string.IsNullOrWhiteSpace(key);
        return useManagedIdentity
            ? new OpenAIClient(new Uri(endpoint), new ManagedIdentityCredential(clientId))
            : new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key!));
    }
}