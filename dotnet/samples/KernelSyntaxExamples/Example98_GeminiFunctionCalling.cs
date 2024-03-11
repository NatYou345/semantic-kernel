﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.GoogleVertexAI;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public sealed class Example98_GeminiFunctionCalling : BaseTest
{
    [RetryFact]
    public async Task GoogleAIAsync()
    {
        this.WriteLine("============= Google AI - Gemini Chat Completion with function calling =============");

        string geminiApiKey = TestConfiguration.GoogleAI.ApiKey;
        string geminiModelId = TestConfiguration.GoogleAI.Gemini.ModelId;

        if (geminiApiKey is null || geminiModelId is null)
        {
            this.WriteLine("Gemini credentials not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(
                modelId: geminiModelId,
                apiKey: geminiApiKey)
            .Build();

        await this.RunSampleAsync(kernel);
    }

    [RetryFact]
    public async Task VertexAIAsync()
    {
        this.WriteLine("============= Vertex AI - Gemini Chat Completion with function calling =============");

        string geminiApiKey = TestConfiguration.VertexAI.BearerKey;
        string geminiModelId = TestConfiguration.VertexAI.Gemini.ModelId;
        string geminiLocation = TestConfiguration.VertexAI.Location;
        string geminiProject = TestConfiguration.VertexAI.ProjectId;

        if (geminiApiKey is null || geminiModelId is null || geminiLocation is null || geminiProject is null)
        {
            this.WriteLine("Gemini vertex ai credentials not found. Skipping example.");
            return;
        }

        Kernel kernel = Kernel.CreateBuilder()
            .AddVertexAIGeminiChatCompletion(
                modelId: geminiModelId,
                bearerKey: geminiApiKey,
                location: geminiLocation,
                projectId: geminiProject)
            .Build();

        await this.RunSampleAsync(kernel);
    }

    private async Task RunSampleAsync(Kernel kernel)
    {
        // Add a plugin with some helper functions we want to allow the model to utilize.
        kernel.ImportPluginFromFunctions("HelperFunctions", new[]
        {
            kernel.CreateFunctionFromMethod(() => DateTime.UtcNow.ToString("R"), "GetCurrentUtcTime", "Retrieves the current time in UTC."),
            kernel.CreateFunctionFromMethod((string cityName) =>
                cityName switch
                {
                    "Boston" => "61 and rainy",
                    "London" => "55 and cloudy",
                    "Miami" => "80 and sunny",
                    "Paris" => "60 and rainy",
                    "Tokyo" => "50 and sunny",
                    "Sydney" => "75 and sunny",
                    "Tel Aviv" => "80 and sunny",
                    _ => "31 and snowing",
                }, "Get_Weather_For_City", "Gets the current weather for the specified city"),
        });

        WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        {
            GeminiPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            WriteLine(await kernel.InvokePromptAsync(
                "Check current UTC time, and return current weather in Paris city", new(settings)));
            WriteLine();
        }

        WriteLine("======== Example 2: Use automated function calling with a streaming prompt ========");
        {
            GeminiPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            await foreach (var update in kernel.InvokePromptStreamingAsync(
                               "Check current UTC time, and return current weather in Boston city", new(settings)))
            {
                Write(update);
            }

            WriteLine();
        }

        WriteLine("======== Example 3: Use manual function calling with a non-streaming prompt ========");
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();

            GeminiPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions };
            chatHistory.AddUserMessage("Check current UTC time, and return current weather in London city");
            while (true)
            {
                var result = (GeminiChatMessageContent)await chat.GetChatMessageContentAsync(chatHistory, settings, kernel);

                if (result.Content is not null)
                {
                    Write(result.Content);
                }

                if (result.ToolCalls is not { Count: > 0 })
                {
                    break;
                }

                chatHistory.Add(result);
                foreach (var toolCall in result.ToolCalls)
                {
                    if (!kernel.Plugins.TryGetFunctionAndArguments(toolCall, out KernelFunction? function, out KernelArguments? arguments))
                    {
                        this.WriteLine("Unable to find function. Please try again!");
                        continue;
                    }

                    var functionResponse = await function.InvokeAsync(kernel, arguments);
                    Assert.NotNull(functionResponse);

                    var calledToolResult = new GeminiFunctionToolResult(toolCall, functionResponse);

                    chatHistory.Add(new GeminiChatMessageContent(calledToolResult));
                }
            }

            WriteLine();
        }

        /* Uncomment this to try in a console chat loop.
        Console.WriteLine("======== Example 4: Use automated function calling with a streaming chat ========");
        {
            GeminiPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();

            while (true)
            {
                Console.Write("Question (Type \"quit\" to leave): ");
                string question = Console.ReadLine() ?? string.Empty;
                if (question == "quit")
                {
                    break;
                }

                chatHistory.AddUserMessage(question);
                System.Text.StringBuilder sb = new();
                await foreach (var update in chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
                {
                    if (update.Content is not null)
                    {
                        Console.Write(update.Content);
                        sb.Append(update.Content);
                    }
                }

                chatHistory.AddAssistantMessage(sb.ToString());
                Console.WriteLine();
            }
        }
        */
    }

    public Example98_GeminiFunctionCalling(ITestOutputHelper output) : base(output) { }
}
