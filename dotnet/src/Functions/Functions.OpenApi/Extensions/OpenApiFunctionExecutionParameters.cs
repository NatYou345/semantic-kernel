﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Http;

namespace Microsoft.SemanticKernel.Plugins.OpenApi;

/// <summary>
/// OpenAPI function execution parameters.
/// </summary>
public class OpenApiFunctionExecutionParameters
{
    /// <summary>
    /// HttpClient to use for sending HTTP requests.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Callback for adding authentication data to HTTP requests.
    /// </summary>
    public AuthenticateRequestAsyncCallback? AuthCallback { get; set; }

    /// <summary>
    /// Override for REST API server url.
    /// </summary>
    public Uri? ServerUrlOverride { get; set; }

    /// <summary>
    /// Flag indicating whether to ignore non-compliant errors of the OpenAPI document or not.
    /// If set to true, the execution will not throw exceptions for non-compliant documents.
    /// Please note that enabling this option may result in incomplete or inaccurate execution results.
    /// </summary>
    public bool IgnoreNonCompliantErrors { get; set; }

    /// <summary>
    /// Optional user agent header value.
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Determines whether the REST API operation payload is constructed dynamically based on payload metadata.
    /// It's enabled by default and allows to support operations with simple payload structure - no properties with the same name at different levels.
    /// To support more complex payloads, it should be disabled and the payload should be provided via the 'payload' argument.
    /// See the 'Providing Payload for OpenAPI Functions' ADR for more details: https://github.com/microsoft/semantic-kernel/blob/main/docs/decisions/0062-open-api-payload.md
    /// </summary>
    [Experimental("SKEXP0040")]
    public bool EnableDynamicPayload { get; set; }

    /// <summary>
    /// Determines whether payload parameter names are augmented with namespaces. It's only applicable when EnableDynamicPayload property is set to true.
    /// Namespaces prevent naming conflicts by adding the parent parameter name as a prefix, separated by dots.
    /// For instance, without namespaces, the 'email' parameter for both the 'sender' and 'receiver' parent parameters
    /// would be resolved from the same 'email' argument, which is incorrect. However, by employing namespaces,
    /// the parameters 'sender.email' and 'sender.receiver' will be correctly resolved from arguments with the same names.
    /// See the 'Providing Payload for OpenAPI Functions' ADR for more details: https://github.com/microsoft/semantic-kernel/blob/main/docs/decisions/0062-open-api-payload.md
    /// </summary>
    [Experimental("SKEXP0040")]
    public bool EnablePayloadNamespacing { get; set; }

    /// <summary>
    /// Optional list of HTTP operations to skip when importing the OpenAPI document.
    /// </summary>
    [Experimental("SKEXP0040")]
    public IList<string> OperationsToExclude { get; set; }

    /// <summary>
    /// A custom HTTP response content reader. It can be useful when the internal reader
    /// for a specific content type is either missing, insufficient, or when custom behavior is desired.
    /// For instance, the internal reader for "application/json" HTTP content reads the content as a string.
    /// This may not be sufficient in cases where the JSON content is large, streamed chunk by chunk, and needs to be accessed
    /// as soon as the first chunk is available. To handle such cases, a custom reader can be provided to read the content
    /// as a stream rather than as a string.
    /// If the custom reader is not provided, or the reader returns null, the internal reader is used.
    /// </summary>
    [Experimental("SKEXP0040")]
    public HttpResponseContentReader? HttpResponseContentReader { get; set; }

    /// <summary>
    /// A custom REST API parameter filter.
    /// </summary>
    [Experimental("SKEXP0040")]
    internal RestApiParameterFilter? ParameterFilter { get; set; }

    /// <summary>
    /// The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiFunctionExecutionParameters"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for sending HTTP requests.</param>
    /// <param name="authCallback">The callback for adding authentication data to HTTP requests.</param>
    /// <param name="serverUrlOverride">The override for the REST API server URL.</param>
    /// <param name="userAgent">Optional user agent header value.</param>
    /// <param name="ignoreNonCompliantErrors">A flag indicating whether to ignore non-compliant errors of the OpenAPI document or not
    /// If set to true, the execution will not throw exceptions for non-compliant documents.
    /// Please note that enabling this option may result in incomplete or inaccurate execution results.</param>
    /// <param name="enableDynamicOperationPayload">Determines whether the REST API operation payload is constructed dynamically based on payload metadata.
    /// If false, the REST API payload must be provided via the 'payload' argument.</param>
    /// <param name="enablePayloadNamespacing">Determines whether payload parameter names are augmented with namespaces.
    /// Namespaces prevent naming conflicts by adding the parent parameter name as a prefix, separated by dots.</param>
    /// <param name="operationsToExclude">Optional list of operations not to import, e.g. in case they are not supported</param>
    [Experimental("SKEXP0040")]
    public OpenApiFunctionExecutionParameters(
        HttpClient? httpClient = null,
        AuthenticateRequestAsyncCallback? authCallback = null,
        Uri? serverUrlOverride = null,
        string? userAgent = null,
        bool ignoreNonCompliantErrors = false,
        bool enableDynamicOperationPayload = true,
        bool enablePayloadNamespacing = false,
        IList<string>? operationsToExclude = null)
    {
        this.HttpClient = httpClient;
        this.AuthCallback = authCallback;
        this.ServerUrlOverride = serverUrlOverride;
        this.UserAgent = userAgent ?? HttpHeaderConstant.Values.UserAgent;
        this.IgnoreNonCompliantErrors = ignoreNonCompliantErrors;
        this.EnableDynamicPayload = enableDynamicOperationPayload;
        this.EnablePayloadNamespacing = enablePayloadNamespacing;
        this.OperationsToExclude = operationsToExclude ?? [];
    }
}
