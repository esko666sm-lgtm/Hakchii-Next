using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class ConsoleProfileClassificationTests
{
    [Fact]
    public void ProductContainingNesAsPartOfWord_RemainsUnknown()
    {
        var profile = ConsoleProfiles.FromProbe(new DeviceProbeResult
        {
            Found = true,
            HardwareId = "USB\\VID_1234&PID_5678",
            ProductName = "Honest USB Device"
        });

        Assert.Equal(ConsoleFamily.Unknown, profile.Family);
        Assert.False(profile.Capabilities.SupportsNandBackup);
    }

    [Theory]
    [InlineData("Nintendo Classic Mini: NES", ConsoleFamily.NesMini)]
    [InlineData("nes-usa", ConsoleFamily.NesMini)]
    [InlineData("Nintendo Classic Mini: SNES", ConsoleFamily.SnesMini)]
    [InlineData("snes-eur", ConsoleFamily.SnesMini)]
    [InlineData("Super Famicom", ConsoleFamily.SnesMini)]
    [InlineData("Famicom", ConsoleFamily.NesMini)]
    public void KnownConsoleNames_AreClassifiedByTokens(string productName, ConsoleFamily expected)
    {
        var profile = ConsoleProfiles.FromProbe(new DeviceProbeResult
        {
            Found = true,
            HardwareId = "USB\\VID_1234&PID_5678",
            ProductName = productName
        });

        Assert.Equal(expected, profile.Family);
    }
}
