CorrelationId
=============

This repo contains middleware for synching a TraceIdentity (correlation ID) across ASP.NET Core APIs.

This is intended for cases where you have multiple API services that may pass a single user request (transaction) through a chain of APIs in order to satisfy the final result. For example, a front end API may be called from a browser, which then in turn calls a backend API to gather some required data.

The TraceIdentifier on the HttpContext will be used for new requests and additionally set a header on the response. In cases where the incoming request includes an existing correlation ID in the header, the TraceIdentifier will be updated to that ID. This allows logging and diagnostics to be correlated for a single user transaction and to track the path of a user request through multiple API services.