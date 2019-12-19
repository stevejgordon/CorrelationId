# Release Notes

Packages are available on NuGet: [CorrelationId](https://www.nuget.org/packages/CorrelationId/).

## v3.0.0-preview.1

**Includes breaking changes**

TODO

## 2.1.0

**Potential breaking changes**

Unfortunately, despite this being a minor release a potential breaking change has slipped in. The Create method on the CorrelationContextFactory requires two arguments (previously one). If you are mocking or using this class directly then this change may affect you.

* Adds a new option - UpdateTraceIdentifier: Controls whether the ASP.NET Core TraceIdentifier will be set to match the CorrelationId. The default value is `true`.
* Adds a new option - UseGuidForCorrelationId : Controls whether a GUID will be used in cases where no correlation ID is retrieved from the request header. When false the TraceIdentifier for the current request will be used. The default value is `false`.

## 2.0.1

* Non breaking change to include the correct project and repo URLs in the NuGet package information.

## 2.0.0

**Includes breaking changes**

This major release introduces a key requirement of including a CorrelationContext which makes it possible to access the CorrelationId in classes that don't automatically have access to the HttpContext in order to retrieve the TraceIdentifier.

This is a breaking change since the registration of services in now required. An exception will be thrown if the services are not registered prior to calling the middleware.

Consuming classes can now include a constructor dependency for ICorrelationContextAccessor which will enable the retrieval and use of the current CorrelationContext when performing logging.

## v1.0.1

* Fix #3 - Avoid setting reponse header if it is already set

## v1.0.0

* Initial release