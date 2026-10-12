using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class TransportDeviceProbeSafetyTests
{
    [Fact]
    public async Task EmptyHardwareId_ReturnsNotFound()
    {
        var transport = new FakeTransport(" \r\n\t ", "nes-usa");
        var result = await new TransportDeviceProbe(transport).ProbeAsync(CancellationToken.None);

        Assert.False(result.Found);
        Assert.Single(transport.Commands);
    }

    [Fact]
    public async Task EmptySystemCode_ReturnsNotFound()
    {
        var transport = new FakeTransport("ABC123", "  ");
        var result = await new TransportDeviceProbe(transport).ProbeAsync(CancellationToken.None);

        Assert.False(result.Found);
        Assert.Equal(2, transport.Commands.Count);
    }

    [Fact]
    public async Task ValidIdentity_ReturnsFound()
    {
        var transport = new FakeTransport("AB C 123", "snes-eur");
        var result = await new TransportDeviceProbe(transport).ProbeAsync(CancellationToken.None);

        Assert.True(result.Found);
        Assert.Equal("ABC123", result.HardwareId);
        Assert.Equal("snes-eur", result.ProductName);
    }

    private sealed class FakeTransport : IDeviceCommandTransport
    {
        private readonly string hardwareId;
        private readonly string systemCode;

        public FakeTransport(string hardwareId, string systemCode)
        {
            this.hardwareId = hardwareId;
            this.systemCode = systemCode;
        }

        public bool IsOnline => true;
        public List<string> Commands { get; } = new List<string>();

        public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string> ExecuteAsync(string command, CancellationToken ct)
        {
            Commands.Add(command);
            return Task.FromResult(command == "hakchi hwid" ? hardwareId : systemCode);
        }
    }
}
