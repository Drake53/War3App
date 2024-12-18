﻿using System.IO;

using War3Net.IO.Mpq;

namespace War3App.MapAdapter.Mpq
{
    public sealed class ListFileAdapter : IMapFileAdapter
    {
        public string MapFileDescription => "MPQ ListFile";

        public string DefaultFileName => ListFile.FileName;

        public bool IsTextFile => true;

        public bool IsJsonSerializationSupported => false;

        public AdaptResult AdaptFile(Stream stream, AdaptFileContext context)
        {
            return MapFileStatus.Compatible;
        }
    }
}