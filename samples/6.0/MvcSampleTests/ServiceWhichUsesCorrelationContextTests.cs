using CorrelationId;
using CorrelationId.Abstractions;
using Moq;
using MvcSample;

namespace MvcSampleTests;

public class ServiceWhichUsesCorrelationContextTests
{
    [Fact]
    public void DoStuff_Test()
    {
        var mockCorrelationContextAccessor = new Mock<ICorrelationContextAccessor>();
        mockCorrelationContextAccessor.Setup(x => x.CorrelationContext)
            .Returns(new CorrelationContext("ABC", "RequestHeader"));

        var sut = new ServiceWhichUsesCorrelationContext(mockCorrelationContextAccessor.Object);

        var result = sut.DoStuff();

        Assert.Equal("Formatted correlation ID:ABC", result);
    }
}