using System; using System.Threading; using System.Threading.Tasks;
namespace Hakchii.Next.Core.Recovery {
 public sealed class BackupImage { public string Id{get;set;} public byte[] Data{get;set;} public string Sha256{get;set;} public DateTime CreatedUtc{get;set;} }
 public interface IBackupService { Task<BackupImage> CreateAsync(CancellationToken ct); Task<OperationResult> VerifyAsync(BackupImage image,CancellationToken ct); Task<OperationResult> RestoreAsync(BackupImage image,CancellationToken ct); }
 public sealed class RecoveryService { private readonly IBackupService backup; public RecoveryService(IBackupService b){backup=b;} public async Task<OperationResult> RestoreVerifiedAsync(BackupImage image,CancellationToken ct){if(image==null)return OperationResult.Fail("Backup image is required");var v=await backup.VerifyAsync(image,ct).ConfigureAwait(false);if(!v.Success)return v;return await backup.RestoreAsync(image,ct).ConfigureAwait(false);} }
}
