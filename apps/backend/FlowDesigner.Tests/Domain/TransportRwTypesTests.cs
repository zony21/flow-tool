using FlowDesigner.Domain.Entities.Core;
using Xunit;

namespace FlowDesigner.Tests.Domain;

public sealed class TransportRwTypesTests
{
    [Theory]
    [InlineData("NONE", "NONE")]
    [InlineData("READ", "READ")]
    [InlineData("WRITE", "WRITE")]
    [InlineData("read", "READ")]
    [InlineData(" write ", "WRITE")]
    [InlineData(null, "NONE")]
    [InlineData("", "NONE")]
    [InlineData("INVALID", "NONE")]
    public void NormalizeOrDefault_ReturnsExpectedValue(string? value, string expected)
    {
        Assert.Equal(expected, TransportRwTypes.NormalizeOrDefault(value));
    }

    [Theory]
    [InlineData("NONE", true)]
    [InlineData("READ", true)]
    [InlineData("WRITE", true)]
    [InlineData("read", true)]
    [InlineData("INVALID", false)]
    [InlineData(null, false)]
    public void IsValid_ReturnsExpectedResult(string? value, bool expected)
    {
        Assert.Equal(expected, TransportRwTypes.IsValid(value));
    }
}
