# Correlation ID

Correlations IDs are used in distributed applications to trace requests across multiple services. This library and package provides a lightweight correlation ID approach. When enabled, request headers are checked for a correlation ID from the consumer. If found, this correlation ID is attached to the Correlation Context which can be used to access the current correlation ID where it is required for logging etc.

Optionally, this correlation ID can be attached to downstream HTTP calls made via a `HttpClient` instance created by the `IHttpClientFactory`.

## Release Notes

[Change history and release notes](https://stevejgordon.github.io/CorrelationId/releasenotes).

## Supported Runtimes
- .NET Standard 2.0+

| Package | NuGet Stable | NuGet Pre-release | Downloads | Travis CI | Azure Pipelines |
| ------- | ------------ | ----------------- | --------- | --------- | ----------------|
| [CorrelationId](https://www.nuget.org/packages/CorrelationId/) | [![NuGet](https://img.shields.io/nuget/v/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![NuGet](https://img.shields.io/nuget/vpre/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![Nuget](https://img.shields.io/nuget/dt/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![Build Status](https://travis-ci.org/stevejgordon/CorrelationId.svg?branch=dev)](https://travis-ci.org/stevejgordon/CorrelationId) | [![Build Status](https://stevejgordon.visualstudio.com/CorrelationId/_apis/build/status/stevejgordon.CorrelationId)](https://stevejgordon.visualstudio.com/CorrelationId/_build/latest?definitionId=1) |

## Installation

You should install [CorrelationId from NuGet](https://www.nuget.org/packages/CorrelationId/):

```ps
Install-Package CorrelationId
```

This command from Package Manager Console will download and install CorrelationId and all required dependencies.

All stable and some pre-release packages are available on NuGet. 

## Usage

Examples in the [wiki](https://github.com/stevejgordon/CorrelationId/wiki).

## Known Issue with ASP.NET Core 2.2.0

It appears that a [regression in the code for ASP.NET Core 2.2.0](https://github.com/aspnet/AspNetCore/issues/5144) means that setting the TraceIdentifier on the context via middleware results in the context becoming null when accessed further down in the pipeline. A fix is ready for 3.0.0 and the team plan to back-port this for the 2.2.2 release timeframe.

A workaround at this time is to disable the behaviour of updating the TraceIdentifier using the options when adding the middleware.

## Support

If this library has helped you, feel free to [buy me a coffee](https://www.buymeacoffee.com/stevejgordon) or see the "Sponsor" link [at the top of the GitHub page](https://github.com/stevejgordon/CorrelationId).