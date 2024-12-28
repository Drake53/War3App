using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

using War3App.MapAdapter.Audio;
using War3App.MapAdapter.Drawing;
using War3App.MapAdapter.Environment;
using War3App.MapAdapter.Extensions;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.Modeling;
using War3App.MapAdapter.Mpq;
using War3App.MapAdapter.Object;
using War3App.MapAdapter.Script;
using War3App.MapAdapter.Widget;

using War3Net.Build.Audio;
using War3Net.Build.Environment;
using War3Net.Build.Info;
using War3Net.Build.Object;
using War3Net.Build.Script;
using War3Net.Build.Widget;
using War3Net.Common.Extensions;
using War3Net.IO.Mpq;

namespace War3App.MapAdapter
{
    public static class AdapterFactory
    {
        private static readonly Dictionary<int, IMapFileAdapter> _adaptersByFileHeader = new(GetAdaptersByFileHeader());
        private static readonly Dictionary<string, IMapFileAdapter> _adaptersByFileName = new(GetAdaptersByFileName(), StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, IMapFileAdapter> _adaptersByFileExtension = new(GetAdaptersByFileExtension(), StringComparer.OrdinalIgnoreCase);

        public static IMapFileAdapter? GetAdapter(Stream stream, string? fileName)
        {
            return TryGetAdapter(stream, fileName, out var adapter) ? adapter : null;
        }

        public static bool TryGetAdapter(Stream stream, string? fileName, [NotNullWhen(true)] out IMapFileAdapter? adapter)
        {
            var oldStreamPosition = stream.Position;
            if (oldStreamPosition + 4 <= stream.Length)
            {
                try
                {
                    int header;
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                    {
                        header = reader.ReadInt32();
                    }

                    if (_adaptersByFileHeader.TryGetValue(header, out adapter))
                    {
                        return true;
                    }
                }
                catch
                {
                }
                finally
                {
                    stream.Position = oldStreamPosition;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (_adaptersByFileName.TryGetValue(fileName, out adapter) || _adaptersByFileExtension.TryGetValue(fileName.GetFileExtension(), out adapter))
                {
                    return true;
                }
            }

            adapter = null;
            return false;
        }

        private static IEnumerable<KeyValuePair<int, IMapFileAdapter>> GetAdaptersByFileHeader()
        {
            yield return new("BLP0".FromRawcode(), BlpImageAdapter.Instance);
            yield return new("BLP1".FromRawcode(), BlpImageAdapter.Instance);
            yield return new("BLP2".FromRawcode(), BlpImageAdapter.Instance);

            yield return new("MDLX".FromRawcode(), BinaryModelAdapter.Instance);

            yield return new("WTG!".FromRawcode(), MapTriggersAdapter.Instance);
        }

        private static IEnumerable<KeyValuePair<string, IMapFileAdapter>> GetAdaptersByFileName()
        {
            // No file extension.
            yield return new(Attributes.FileName, AttributesAdapter.Instance);
            yield return new(ListFile.FileName, ListFileAdapter.Instance);

            // Different file types with same file extension.
            yield return new(MapDoodads.FileName, MapDoodadsAdapter.Instance);
            yield return new(MapUnits.FileName, MapUnitsAdapter.Instance);
        }

        private static IEnumerable<KeyValuePair<string, IMapFileAdapter>> GetAdaptersByFileExtension()
        {
            yield return new(MapSounds.FileExtension, MapSoundsAdapter.Instance);
            yield return new(".flac", FlacAdapter.Instance);
            yield return new(".mp3", Mp3Adapter.Instance);
            yield return new(".ogg", OggAdapter.Instance);
            yield return new(".wav", WavAdapter.Instance);

            yield return new(".blp", BlpImageAdapter.Instance);
            yield return new(".dds", DdsImageAdapter.Instance);
            yield return new(".tga", TgaImageAdapter.Instance);

            yield return new(MapCameras.FileExtension, MapCamerasAdapter.Instance);
            yield return new(MapEnvironment.FileExtension, MapEnvironmentAdapter.Instance);
            yield return new(MapPathingMap.FileExtension, MapPathingMapAdapter.Instance);
            yield return new(MapPreviewIcons.FileExtension, MapPreviewIconsAdapter.Instance);
            yield return new(MapRegions.FileExtension, MapRegionsAdapter.Instance);
            yield return new(MapShadowMap.FileExtension, MapShadowMapAdapter.Instance);

            yield return new(CampaignInfo.FileExtension, CampaignInfoAdapter.Instance);
            yield return new(MapInfo.FileExtension, MapInfoAdapter.Instance);

            yield return new(".mdx", BinaryModelAdapter.Instance);
            yield return new(".mdl", TextModelAdapter.Instance);

            yield return new(AbilityObjectData.FileExtension, AbilityObjectDataAdapter.Instance);
            yield return new(BuffObjectData.FileExtension, BuffObjectDataAdapter.Instance);
            yield return new(DestructableObjectData.FileExtension, DestructableObjectDataAdapter.Instance);
            yield return new(DoodadObjectData.FileExtension, DoodadObjectDataAdapter.Instance);
            yield return new(ItemObjectData.FileExtension, ItemObjectDataAdapter.Instance);
            // TODO: .w3o
            yield return new(UnitObjectData.FileExtension, UnitObjectDataAdapter.Instance);
            yield return new(UpgradeObjectData.FileExtension, UpgradeObjectDataAdapter.Instance);

            yield return new(MapCustomTextTriggers.FileExtension, MapCustomTextTriggersAdapter.Instance);
            yield return new(MapTriggers.FileExtension, MapTriggersAdapter.Instance);
            yield return new(TriggerStrings.FileExtension, TriggerStringsAdapter.Instance);
            yield return new(JassMapScript.FileName.GetFileExtension(), JassMapScriptAdapter.Instance);
            yield return new(LuaMapScript.FileName.GetFileExtension(), MapScriptRegexAdapter.Instance);
        }
    }
}