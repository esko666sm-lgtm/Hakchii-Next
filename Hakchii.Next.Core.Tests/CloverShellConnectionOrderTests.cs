using System.Threading;
using System.Threading.Tasks;
using Hakchii.Next.Core;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CloverShellConnectionOrderTests
{
    [Fact]
    public async Task Connect_BringsTransportOnlineBeforeDetection()
    {
        var transport = new Transport();
        var detector = new Detector(transport);
        var connection = new CloverShellConsoleConnection(transport, detector);

        var result = await connection.ConnectAsync(CancellationToken.None);

        Assert.True(result.Success);
        Assert.True(detector.SawOnlineTransport);
        Assert.True(connection.IsConnected);
    }

    [Fact]
    public async Task UnsupportedDetectedConsole_DisconnectsTransportAndClearsProfile()
    {
        var transport = new Transport();
        var detector = new UnsupportedDetector();
        var connection = new CloverShellConsoleConnection(transport, detector);

        var result = await connection.ConnectAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(connection.IsConnected);
        Assert.Null(connection.Profile);
        Assert.Equal(1, transport.DisconnectCount);
    }

    [Fact]
    public async Task DetectionFailure_DisconnectsTransportAndClearsProfile()
    {
        var transport = new Transport();
        var connection = new CloverShellConsoleConnection(transport, new NullDetector());

        var result = await connection.ConnectAsync(CancellationToken.None);

        Assert.False(result.Success);
        Assert.False(connection.IsConnected);
        Assert.Null(connection.Profile);
        Assert.Equal(1, transport.DisconnectCount);
    }

    private sealed class Transport : ICloverShellTransport
    {
        public bool IsOnline { get; private set; }
        public int DisconnectCount { get; private set; }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            IsOnline = true;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken)
        {
            DisconnectCount++;
            IsOnline = false;
            return Task.CompletedTask;
        }
    }

    private sealed class Detector : IDeviceDetector
    {
        private readonly Transport transport;
        public bool SawOnlineTransport { get; private set; }
        public Detector(Transport transport) { this.transport = transport; }

        public Task<ConsoleProfile> DetectAsync(CancellationToken cancellationToken)
        {
            SawOnlineTransport = transport.IsOnline;
            return Task.FromResult(new ConsoleProfile
            {
                Family = ConsoleFamily.NesMini,
                Capabilities = ConsoleProfiles.CapabilitiesFor(ConsoleFamily.NesMini)
            });
        }
    }

    private sealed class UnsupportedDetector : IDeviceDetector
    {
        public Task<ConsoleProfile> DetectAsync(CancellationToken cancellationToken)
            => Task.FromResult(new ConsoleProfile
            {
                Family = ConsoleFamily.Unknown,
                Capabilities = ConsoleProfiles.CapabilitiesFor(ConsoleFamily.Unknown)
            });
    }

    private sealed class NullDetector : IDeviceDetector
    {
        public Task<ConsoleProfile> DetectAsync(CancellationToken cancellationToken)
            => Task.FromResult<ConsoleProfile>(null!);
    }
}
