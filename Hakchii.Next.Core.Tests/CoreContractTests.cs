using Hakchii.Next.Core;
using Hakchii.Next.Core.Device;
using Xunit;

namespace Hakchii.Next.Core.Tests;

public class CoreContractTests
{
    [Fact]
    public void OperationResult_Failure_PreservesReason()
    {
        var result = OperationResult.Fail("offline");
        Assert.False(result.Success);
        Assert.Equal("offline", result.Error);
    }

    [Fact]
    public void NesProfile_CanAdvertiseExpectedCapabilities()
    {
        var profile = new ConsoleProfile
        {
            Id = "nes-mini",
            Name = "Nintendo Classic Mini: NES",
            Family = ConsoleFamily.NesMini,
            Capabilities = new DeviceCapabilities { SupportsCloverShell = true, SupportsHmod = true, SupportsRetroArch = true, SupportsNandBackup = true }
        };
        Assert.Equal(ConsoleFamily.NesMini, profile.Family);
        Assert.True(profile.Capabilities.SupportsRetroArch);
        Assert.True(profile.Capabilities.SupportsHmod);
    }
}
