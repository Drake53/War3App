using System;
using System.IO;
using System.IO.Compression;

namespace War3App.MapAdapter.Extensions
{
    public static class TargetPatchExtensions
    {
        public static Stream? OpenGameDataFile(this TargetPatch targetPatch, string gameDataFilePath)
        {
            switch (targetPatch.GameDataContainerType)
            {
                case ContainerType.Directory:
                    return File.OpenRead(Path.Combine(targetPatch.GameDataPath, gameDataFilePath));

                case ContainerType.ZipArchive:
                {
                    using var zipArchiveStream = File.OpenRead(targetPatch.GameDataPath);
                    using var zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Read);

                    var zipEntry = zipArchive.GetEntryNormalized(gameDataFilePath);
                    if (zipEntry is null)
                    {
                        return null;
                    }

                    using var zipEntryStream = zipEntry.Open();

                    var resultStream = new MemoryStream();
                    zipEntryStream.CopyTo(resultStream);
                    resultStream.Position = 0;

                    return resultStream;
                }

                case ContainerType.MpqArchive:
                case ContainerType.CascArchive:
                    throw new NotSupportedException("MPQ and CASC archives are not supported.");

                default:
                    throw new ArgumentException("Invalid container type.", nameof(targetPatch));
            }
        }
    }
}