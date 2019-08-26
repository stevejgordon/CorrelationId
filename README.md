# Correlation ID Middleware

[![NuGet](https://img.shields.io/nuget/v/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId)
[![NuGet](https://img.shields.io/nuget/dt/CorrelationId.svg)](https://www.nuget.org/packages/CorrelationId)
[![Build Status](https://travis-ci.org/stevejgordon/CorrelationId.svg?branch=dev)](https://travis-ci.org/stevejgordon/CorrelationId)
[![Build Status](https://stevejgordon.visualstudio.com/CorrelationId/_apis/build/status/stevejgordon.CorrelationId)](https://stevejgordon.visualstudio.com/CorrelationId/_build/latest?definitionId=1)

This repo contains middleware for syncing a TraceIdentity (correlation ID) across ASP.NET Core APIs.

This is intended for cases where you have multiple API services that may pass a single user request (transaction) through a chain of APIs in order to satisfy the final result. For example, a front end API may be called from a browser, which then in turn calls a backend API to gather some required data.

The TraceIdentifier on the HttpContext will be used for new requests and additionally set a header on the response. In cases where the incoming request includes an existing correlation ID in the header, the TraceIdentifier will be updated to that ID. This allows logging and diagnostics to be correlated for a single user transaction and to track the path of a user request through multiple API services.

Examples in the [wiki](https://github.com/stevejgordon/CorrelationId/wiki).

## Known Issue with ASP.NET Core 2.2.0

It appears that a [regression in the code for ASP.NET Core 2.2.0](https://github.com/aspnet/AspNetCore/issues/5144) means that setting the TraceIdentifier on the context via middleware results in the context becoming null when accessed further down in the pipeline. A fix is ready for 3.0.0 and the team plan to back-port this for the 2.2.2 release timeframe.

A workaround at this time is to disable the behaviour of updating the TraceIdentifier using the options when adding the middleware...

```
app.UseCorrelationId(new CorrelationIdOptions
{
	Header = "X-Correlation-ID",
	UseGuidForCorrelationId = true,
	UpdateTraceIdentifier = false
});
```

## Installation

You should install [CorrelationId with NuGet](https://www.nuget.org/packages/CorrelationId/):

    Install-Package CorrelationId

This command from Package Manager Console will download and install CorrelationId and all required dependencies.
