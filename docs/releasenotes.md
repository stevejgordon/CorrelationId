# Release Notes

Packages are available on NuGet: [CorrelationId](https://www.nuget.org/packages/CorrelationId/).

## v3.0.1

### Bug Fixes

* Do not throw exception when Correlation accessor has Context as null - [#96](https://github.com/stevejgordon/CorrelationId/pull/96)

## v3.0.0

Several requested features have been added which provide more control over correlation ID generation and customisation.

This release includes several breaking changes, and upgrading will require some changes to consuming code.

A major new feature in this release is the concept of an `ICorrelationIdProvider`. This interface defines an abstraction for generating correlation IDs. The library includes two provider implementations which include the previous behaviour. The `GuidCorrelationIdProvider`, when registered will generate new GUID-based correlations IDs. The `TraceIdCorrelationIdProvider` will generate the correlation ID, setting it to the same value as the TraceIdentifier string on the `HttpContext`.

Only one provider may be registered. Registering multiple providers will cause an `InvalidOperationException` to be thrown.

**BREAKING CHANGES**

### Registering services

Changes have been made to the registration methods on the `IServiceCollection` to support the new providers concept.

When registering the required correlation ID services by calling the `AddCorrelationId` method, this now returns an `ICorrelationIdBuilder` that supports additional methods that can be used to configure the provider, which will be used. This method does not set a default provider, so it is expected that one of the appropriate `ICorrelationIdBuilder` builder methods be called.

Alternatively, the `AddCorrelationId<T>` method can be called, which accepts the type to use for the `ICorrelationIdProvider`.

Finally, the `AddDefaultCorrelationId` method may be used, which returns the `IServiceCollection` and which does not support further configuration of the correlation ID configuration using the builder. In this case, the default provider will be the `GuidCorrelationIdProvider`. This method exists for those wanting to chain `IServiceCollection` extension methods and where the default GUID provider is suitable.

### Configuration Options

A change has been made to how the `CorrelationIdOptions` are configured for the correlation ID behaviour. Previously, a `CorrelationIdOptions` instance could be passed to the `UseCorrelationId` extension method on the `IApplicationBuilder`. This is no longer the correct way to register options. Instead, options can be configured via Action delegate overloads on the `IServiceCollection` extensions methods.

```
services.AddDefaultCorrelationId(options =>
{ 
    options.CorrelationIdGenerator = () => "Foo";
    options.AddToLoggingScope = true;
    options.EnforceHeader = true;
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.RequestHeader = "My-Custom-Correlation-Id";
    options.ResponseHeader = "X-Correlation-Id";
    options.UpdateTraceIdentifier = false;
});
```

### CorrelationIdOptions

**BREAKING CHANGES**

* `DefaultHeader` renamed to `RequestHeader`
* `UseGuidForCorrelationId` removed as this is now controlled by the registered `ICorrelationIdProvider`.

*New Options*

* Added `ResponseHeader` - The name of the header to which the Correlation ID is written. This change supports scenarios where it is necessary to read from one header but return the correlation ID using a different header name. Defaults to the same value as the `RequestHeader` unless specifically set.
* Added `IgnoreRequestHeader` - When `true` the incoming correlation ID in the `RequestHeader` is ignored and a new correlation ID is generated.
* Added `EnforceHeader` - Enforces the inclusion of the correlation ID request header. When `true` and a correlation ID header is not included, the request will fail with a 400 Bad Request response.
* Added `AddToLoggingScope` - Add the correlation ID value to the logger scope for all requests. When `true` the value of the correlation ID will be added to the logger scope payload.
* Added `LoggingScopeKey` - The name for the key used when adding the correlation ID to the logger scope. Defaults to 'CorrelationId'
* Added `CorrelationIdGenerator` - A `Func<string>` that returns the correlation ID in cases where no correlation ID is retrieved from the request header. It can be used to customise the correlation ID generation. When set, this function will be used instead of the registered `ICorrelationIdProvider`.

### CorrelationContext

The constructor for this type has been made public (previously internal) to support the creation of `CorrelationContext` instances when unit testing code which depends on the `ICorrelationContextAccessor`. This makes mocking the `ICorrelationContextAccessor` a much easier task.

### IApplicationBuilder Extension Methods

The overloads of the `UseCorrelationId` methods have been removed as options are no longer provided when adding the correlation ID middleware to the pipeline. 

* `UseCorrelationId(string header)` removed.
* `UseCorrelationId(CorrelationIdOptions options)` removed.

## 2.1.0

**Potential breaking changes**

Unfortunately, despite this being a minor release, a potential breaking change has slipped in. The Create method on the CorrelationContextFactory requires two arguments (previously one). If you are mocking or using this class directly, then this change may affect you.

* Adds a new option `UpdateTraceIdentifier`: Controls whether the ASP.NET Core TraceIdentifier will be set to match the CorrelationId. The default value is `true`.
* Adds a new option `UseGuidForCorrelationId`: Controls whether a GUID will be used in cases where no correlation ID is retrieved from the request header. The default value is `false`.

## 2.0.1

* Non-breaking change to include the correct project and repo URLs in the NuGet package information.

## 2.0.0

**Includes breaking changes**

This major release introduces a key requirement of including a `CorrelationContext` which makes it possible to access the CorrelationId in classes that don't automatically have access to the HttpContext in order to retrieve the TraceIdentifier.

This is a breaking change since the registration of services in now required. An exception will be thrown if the services are not registered prior to calling the middleware.

Consuming classes can now include a constructor dependency for `ICorrelationContextAccessor` which will enable the retrieval and use of the current `CorrelationContext` when performing logging.

## v1.0.1

* Fix #3 - Avoid setting response header if it is already set

## v1.0.0

* Initial release
