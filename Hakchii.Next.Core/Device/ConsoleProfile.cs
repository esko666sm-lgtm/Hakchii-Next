namespace Hakchii.Next.Core.Device
{
    public enum ConsoleFamily { Unknown, NesMini, SnesMini }

    public sealed class DeviceCapabilities
    {
        public bool SupportsCloverShell { get; set; }
        public bool SupportsHmod { get; set; }
        public bool SupportsRetroArch { get; set; }
        public bool SupportsNandBackup { get; set; }
    }

    public sealed class ConsoleProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ConsoleFamily Family { get; set; }
        public DeviceCapabilities Capabilities { get; set; }
    }
}
