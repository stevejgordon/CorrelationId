# Correlation ID

Correlations IDs are used in distributed applications to trace requests across multiple services. This library and package provides a lightweight correlation ID approach. When enabled, request headers are checked for a correlation ID from the consumer. If found, this correlation ID is attached to the Correlation Context which can be used to access the current correlation ID where it is required for logging etc.

Optionally, this correlation ID can be attached to downstream HTTP calls made via a `HttpClient` instance created by the `IHttpClientFactory`.

## Release Notes

[Change history and release notes](https://stevejgordon.github.io/CorrelationId/releasenotes).

## Supported Runtimes
- .NET Standard 2.0+

| Package | NuGet Stable | NuGet Pre-release | Downloads | Travis CI | Azure Pipelines |
| ------- | ------------ | ----------------- | --------- | --------- | ----------------|
| [CorrelationId](https://www.nuget.org/packages/CorrelationId/) | [![NuGet](https://img.shields.io/nuget/v/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![NuGet](https://img.shields.io/nuget/vpre/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![Nuget](https://img.shields.io/nuget/dt/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId) | [![Build Status](https://travis-ci.org/stevejgordon/CorrelationId.svg?branch=master)](https://travis-ci.org/stevejgordon/CorrelationId) | [![Build Status](https://dev.azure.com/stevejgordon/CorrelationId/_apis/build/status/stevejgordon.CorrelationId?branchName=master)](https://dev.azure.com/stevejgordon/CorrelationId/_build/latest?definitionId=1&branchName=master) |

## Installation

You should install [CorrelationId from NuGet](https://www.nuget.org/packages/CorrelationId/):

```ps
Install-Package CorrelationId
```

This command from Package Manager Console will download and install CorrelationId and all required dependencies.

All stable and some pre-release packages are available on NuGet. 

## Quick Start

### Register with DI

Inside `ConfigureServices` add the required correlation ID services, with common defaults.

```csharp
services.AddDefaultCorrelationId
```

This registers a correlation ID provider which generates new IDs based on a random GUID.

### Add the middleware

Register the middleware into the pipeline. This should occur before any downstream middleware which requires the correlation ID. Normally this will be registered very early in the middleware pipeline.

```csharp
app.UseCorrelationId();
```

Where you need to access the correlation ID, you may request the `ICorrelationContextAccessor` from DI.

```csharp
public class TransientClass
{
   private readonly ICorrelationContextAccessor _correlationContext;

   public TransientClass(ICorrelationContextAccessor correlationContext)
   {
	  _correlationContext = correlationContext;
   }

   ...
}
```

See the [sample app](https://github.com/stevejgordon/CorrelationId/tree/master/samples/3.1/MvcSample) for example usage.

Full documentation can be found in the [wiki](https://github.com/stevejgordon/CorrelationId/wiki).

## Known Issue with ASP.NET Core 2.2.0

It appears that a [regression in the code for ASP.NET Core 2.2.0](https://github.com/aspnet/AspNetCore/issues/5144) means that setting the TraceIdentifier on the context via middleware results in the context becoming null when accessed further down in the pipeline. A fix is was released in 2.2.2.

A workaround at this time is to disable the behaviour of updating the TraceIdentifier using the options when adding the middleware.

## Support

If this library has helped you, feel free to [buy me a coffee](https://www.buymeacoffee.com/stevejgordon) or see the "Sponsor" link [at the top of the GitHub page](https://github.com/stevejgordon/CorrelationId).
