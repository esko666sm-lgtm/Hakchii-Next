using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Hakchii.Next.Core.Saves
{
    public interface ISaveBackupStore
    {
        OperationResult Save(SaveSnapshot snapshot);
    }

    public interface ISaveBackupHistoryStore
    {
        IReadOnlyCollection<SaveSnapshot> List(string gameId);
    }

    public sealed class SaveFileStore : ISaveBackupStore, ISaveBackupHistoryStore
    {
        private readonly string rootDirectory;

        public SaveFileStore(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Save backup directory is required", nameof(rootDirectory));

            this.rootDirectory = rootDirectory;
            Directory.CreateDirectory(rootDirectory);
        }

        public OperationResult Save(SaveSnapshot snapshot)
        {
            if (snapshot == null)
                return OperationResult.Fail("Save snapshot is required");
            if (string.IsNullOrWhiteSpace(snapshot.GameId))
                return OperationResult.Fail("Game id is required");
            if (snapshot.Data == null || snapshot.Data.Length == 0)
                return OperationResult.Fail("Save data is empty");

            try
            {
                Directory.CreateDirectory(rootDirectory);

                var createdUtc = snapshot.CreatedUtc == default(DateTime)
                    ? DateTime.UtcNow
                    : snapshot.CreatedUtc.ToUniversalTime();

                var safeGameId = Sanitize(snapshot.GameId);
                var stamp = createdUtc.ToString("yyyyMMddTHHmmssfffffffZ", CultureInfo.InvariantCulture);
                var basePath = Path.Combine(rootDirectory, safeGameId + "-" + stamp);
                var dataPath = basePath + ".sav";
                var manifestPath = basePath + ".meta";

                File.WriteAllBytes(dataPath, snapshot.Data);
                File.WriteAllLines(manifestPath, new[]
                {
                    "gameId=" + snapshot.GameId,
                    "createdUtc=" + createdUtc.ToString("O", CultureInfo.InvariantCulture),
                    "size=" + snapshot.Data.LongLength.ToString(CultureInfo.InvariantCulture),
                    "sha256=" + ComputeSha256(snapshot.Data)
                });

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message);
            }
        }

        public SaveSnapshot LoadLatest(string gameId)
        {
            return List(gameId).FirstOrDefault();
        }

        public IReadOnlyCollection<SaveSnapshot> List(string gameId)
        {
            if (string.IsNullOrWhiteSpace(gameId) || !Directory.Exists(rootDirectory))
                return new SaveSnapshot[0];

            var items = new List<SaveSnapshot>();

            foreach (var metaPath in Directory.GetFiles(rootDirectory, "*.meta"))
            {
                var values = ReadValues(metaPath);
                if (!values.TryGetValue("gameId", out var storedGameId) ||
                    !string.Equals(storedGameId, gameId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!values.TryGetValue("createdUtc", out var createdText) ||
                    !DateTime.TryParse(createdText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var createdUtc))
                    continue;

                if (!values.TryGetValue("size", out var sizeText) ||
                    !long.TryParse(sizeText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expectedSize))
                    continue;

                if (!values.TryGetValue("sha256", out var expectedHash))
                    continue;

                var dataPath = Path.ChangeExtension(metaPath, ".sav");
                if (!File.Exists(dataPath))
                    continue;

                var data = File.ReadAllBytes(dataPath);
                if (data.LongLength != expectedSize)
                    continue;

                if (!string.Equals(ComputeSha256(data), expectedHash, StringComparison.OrdinalIgnoreCase))
                    continue;

                items.Add(new SaveSnapshot
                {
                    GameId = storedGameId,
                    Data = data,
                    CreatedUtc = createdUtc.ToUniversalTime()
                });
            }

            return items.OrderByDescending(x => x.CreatedUtc).ToArray();
        }

        private static Dictionary<string, string> ReadValues(string path)
        {
            return File.ReadAllLines(path)
                .Select(line => line.Split(new[] { '=' }, 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);
        }

        private static string Sanitize(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        }

        private static string ComputeSha256(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
