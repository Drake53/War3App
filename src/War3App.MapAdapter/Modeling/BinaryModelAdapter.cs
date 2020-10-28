﻿using System;
using System.IO;
using System.Text;

using War3App.MapAdapter.Extensions;

using War3Net.Build.Common;
using War3Net.Common.Extensions;

namespace War3App.MapAdapter.Modeling
{
    public sealed class BinaryModelAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "Binary Model";

        public bool IsTextFile => false;

        public bool CanAdaptFile(string s)
        {
            return string.Equals(s.GetFileExtension(), ".mdx", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanAdaptFile(Stream stream)
        {
            // TODO: War3Net.Modeling.BinaryModelParser.IsBinaryModel(stream)

            if (stream.Length < 4)
            {
                return false;
            }

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            int header;
            try
            {
                header = reader.ReadInt32();
                stream.Position = 0;
            }
            catch (ArgumentException)
            {
                stream.Position = 0;
                return false;
            }

            return header == "MDLX".FromRawcode();
        }

        public AdaptResult AdaptFile(Stream stream, GamePatch targetPatch, GamePatch originPatch)
        {
            try
            {
                // TODO: War3Net.Modeling.BinaryModelParser.Parse(stream)

                using var reader = new BinaryReader(stream);
                reader.ReadInt32();

                while (stream.Length - stream.Position > 8)
                {
                    // Find VERS chunk.
                    var chunkTag = new string(reader.ReadChars(4));
                    var chunkSize = reader.ReadInt32();
                    if (chunkTag == "VERS")
                    {
                        var version = reader.ReadUInt32();
                        if (version > 800 && targetPatch < GamePatch.v1_32_0)
                        {
                            return new AdaptResult
                            {
                                Status = MapFileStatus.Unadaptable,
                                Diagnostics = new[] { $"Model version not supported: v{version}" },
                            };
                        }
                        else
                        {
                            return new AdaptResult
                            {
                                Status = MapFileStatus.Compatible,
                            };
                        }
                    }
                    else
                    {
                        reader.ReadBytes(chunkSize);
                    }
                }

                // Unable to find VERS chunk.
                return new AdaptResult
                {
                    // Status = MapFileStatus.ParseError,
                    Status = MapFileStatus.Unknown,
                };
            }
            catch (Exception e)
            {
                return new AdaptResult
                {
                    Status = MapFileStatus.ParseError,
                    Diagnostics = new[] { e.Message },
                };
            }
        }
    }
}