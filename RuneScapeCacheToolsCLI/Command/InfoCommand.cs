using System;
using System.Linq;
using System.Text;
using Villermen.RuneScapeCacheTools.CLI.Argument;
using Villermen.RuneScapeCacheTools.Model;
using Villermen.RuneScapeCacheTools.Utility;

namespace Villermen.RuneScapeCacheTools.CLI.Command
{
    public class InfoCommand : BaseCommand
    {
        public InfoCommand(ArgumentParser argumentParser) : base(argumentParser)
        {
            this.ArgumentParser.AddCommon(CommonArgument.Cache);
            this.ArgumentParser.AddCommon(CommonArgument.Files);
        }

        public override int Run()
        {
            using var sourceCache = this.ArgumentParser.Cache;
            if (sourceCache == null)
            {
                Console.WriteLine("No cache source specified.");
                return Program.ExitCodeInvalidArgument;
            }
            if (this.ArgumentParser.FileFilter == null || this.ArgumentParser.FileFilter.Item1.Length == 0)
            {
                Console.WriteLine("No index/file(s) specified.");
                return Program.ExitCodeInvalidArgument;
            }
            if (this.ArgumentParser.FileFilter.Item1.Length > 1)
            {
                Console.WriteLine("Multiple indexes specified.");
                return Program.ExitCodeInvalidArgument;
            }

            var index = this.ArgumentParser.FileFilter.Item1[0];
            var fileIds = this.ArgumentParser.FileFilter.Item2;

            // Index information
            if (fileIds.Length == 0)
            {
                Console.WriteLine($"Retrieving info for index {(int)index}...");

                var indexName = Enum.GetName(typeof(CacheIndex), index) ?? "IHaveNoIdea";
                Console.WriteLine($"Contents: {indexName} (probably)");

                var referenceTable = sourceCache.GetReferenceTable(index);

                var versionAsTime = "";
                if (referenceTable.Version != null && referenceTable.Version.Value > 946684800)
                {
                    var formattedTime = DateTimeOffset.FromUnixTimeSeconds(referenceTable.Version.Value).ToString("u");
                    versionAsTime = $" ({formattedTime})";
                }
                Console.WriteLine($"Files: {referenceTable.FileIds.Count():N0}");
                Console.WriteLine($"Format: {referenceTable.Format}");
                Console.WriteLine($"Version: {referenceTable.Version}{versionAsTime}");
                Console.WriteLine($"Options: {referenceTable.Options}");

                if (referenceTable.FileIds.Any())
                {
                    var firstFileIds = referenceTable.FileIds.Take(Math.Min(referenceTable.FileIds.Count(), 5));
                    var lastFileIds = referenceTable.FileIds.Reverse().Take(Math.Min(referenceTable.FileIds.Count(), 5)).Reverse();
                    Console.WriteLine($"First files: {string.Join(", ", firstFileIds)}");
                    Console.WriteLine($"Last files: {string.Join(", ", lastFileIds)}");
                }

                return Program.ExitCodeOk;
            }

            // File information
            for (var i = 0; i < fileIds.Length; i++)
            {
                var fileId = fileIds[i];

                if (i > 0)
                {
                    Console.WriteLine();
                }
                Console.WriteLine($"Retrieving info for file {(int)index}/{fileId}...");

                var file = sourceCache.GetFile(index, fileId);

                Console.WriteLine($"Size: {file.Data.Length:N0}");

                Console.WriteLine($"Compression type: {file.Info.CompressionType}");
                if (file.Info.CompressedSize != null)
                {
                    Console.WriteLine($"Compressed size: {file.Info.CompressedSize:N0}");
                }

                if (file.Info.Version != null)
                {
                    var versionAsTime = "";
                    if (file.Info.Version.Value > 946684800)
                    {
                        var formattedTime = DateTimeOffset.FromUnixTimeSeconds(file.Info.Version.Value).ToString("u");
                        versionAsTime = $" ({formattedTime})";
                    }
                    Console.WriteLine($"Version: {file.Info.Version}{versionAsTime}");
                }
                if (file.Info.Crc != null)
                {
                    Console.WriteLine($"CRC: {file.Info.Crc}");
                }
                if (file.Info.HasEntries)
                {
                    Console.WriteLine($"Entries: {file.Info.Entries.Count:N0}");
                }
                if (file.Info.Identifier != null)
                {
                    Console.WriteLine($"Identifier: {file.Info.Identifier}");
                }
                if (file.Info.MysteryHash != null)
                {
                    Console.WriteLine($"Mystery hash: {file.Info.MysteryHash}");
                }
                if (file.Info.WhirlpoolDigest != null)
                {
                    Console.WriteLine($"Whirlpool: {Formatter.BytesToHexString(file.Info.WhirlpoolDigest)}");
                }
                if (file.Info.EncryptionKey != null)
                {
                    Console.WriteLine($"Identifier: {Formatter.BytesToHexString(file.Info.EncryptionKey)}");
                }

                if (file.Data.Length < 10)
                {
                    var bytes = file.Data;
                    Console.WriteLine($"Bytes: {Formatter.BytesToHexString(bytes)} ({Encoding.ASCII.GetString(bytes)})");
                }
                else
                {
                    var firstBytes = file.Data.Take(10).ToArray();
                    var lastBytes = file.Data.Reverse().Take(10).Reverse().ToArray();
                    Console.WriteLine($"First 10 bytes: {Formatter.BytesToHexString(firstBytes)} ({Formatter.BytesToAnsiString(firstBytes)})");
                    Console.WriteLine($"Last 10 bytes: {Formatter.BytesToHexString(lastBytes)} ({Formatter.BytesToAnsiString(lastBytes)})");
                }

                // TODO: Dive further into separate entries (like first and last bytes and size)?
            }

            return Program.ExitCodeOk;
        }
    }
}
