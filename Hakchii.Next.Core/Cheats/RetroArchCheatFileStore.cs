using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hakchii.Next.Core.Cheats
{
    public sealed class RetroArchCheatFileStore
    {
        private readonly RetroArchCheatParser parser = new RetroArchCheatParser();
        private readonly RetroArchCheatSerializer serializer = new RetroArchCheatSerializer();

        public IReadOnlyCollection<CheatEntry> Load(string path, string gameId)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new CheatEntry[0];

            var text = File.ReadAllText(path, Encoding.UTF8);
            return parser.Parse(text, gameId);
        }

        public OperationResult Save(string path, IEnumerable<CheatEntry> cheats)
        {
            if (string.IsNullOrWhiteSpace(path))
                return OperationResult.Fail("Cheat file path is required");

            var items = (cheats ?? new CheatEntry[0]).ToArray();
            string gameId = null;
            foreach (var cheat in items)
            {
                if (cheat == null || string.IsNullOrWhiteSpace(cheat.Id) || string.IsNullOrWhiteSpace(cheat.GameId) || string.IsNullOrWhiteSpace(cheat.Code))
                    return OperationResult.Fail("Invalid cheat");

                cheat.Id = cheat.Id.Trim();
                cheat.GameId = cheat.GameId.Trim();
                cheat.Code = cheat.Code.Trim();
                cheat.Name = string.IsNullOrWhiteSpace(cheat.Name) ? null : cheat.Name.Trim();

                if (gameId == null) gameId = cheat.GameId;
                else if (!string.Equals(gameId, cheat.GameId, StringComparison.OrdinalIgnoreCase))
                    return OperationResult.Fail("Cheat file cannot contain multiple game ids");
            }

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                var tempPath = path + ".tmp";
                var text = serializer.Serialize(items);
                File.WriteAllText(tempPath, text, new UTF8Encoding(false));

                if (File.Exists(path))
                {
                    var backupPath = path + ".bak";
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                    File.Replace(tempPath, path, backupPath);
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                try
                {
                    var tempPath = path + ".tmp";
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch { }

                return OperationResult.Fail(ex.Message);
            }
        }

        public OperationResult ImportInto(CheatDatabase database, string path, string gameId)
        {
            if (database == null) return OperationResult.Fail("Cheat database is required");
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return OperationResult.Fail("Cheat file does not exist");

            return database.ImportRetroArch(gameId, File.ReadAllText(path, Encoding.UTF8));
        }
    }
}
