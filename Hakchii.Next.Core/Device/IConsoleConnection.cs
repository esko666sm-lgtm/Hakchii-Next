using System.Threading;
using System.Threading.Tasks;

namespace Hakchii.Next.Core.Device
{
    public interface IConsoleConnection
    {
        bool IsConnected { get; }
        ConsoleProfile Profile { get; }
        Task<OperationResult> ConnectAsync(CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }

    public interface IDeviceDetector
    {
        Task<ConsoleProfile> DetectAsync(CancellationToken cancellationToken);
    }
}
