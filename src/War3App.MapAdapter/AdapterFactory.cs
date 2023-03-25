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
            yield return new("BLP0".FromRawcode(), new BlpImageAdapter());
            yield return new("BLP1".FromRawcode(), new BlpImageAdapter());
            yield return new("BLP2".FromRawcode(), new BlpImageAdapter());

            yield return new("MDLX".FromRawcode(), new BinaryModelAdapter());

            yield return new("WTG!".FromRawcode(), new MapTriggersAdapter());
        }

        private static IEnumerable<KeyValuePair<string, IMapFileAdapter>> GetAdaptersByFileName()
        {
            // No file extension.
            yield return new(Attributes.FileName, new AttributesAdapter());
            yield return new(ListFile.FileName, new ListFileAdapter());

            // Different file types with same file extension.
            yield return new(MapDoodads.FileName, new MapDoodadsAdapter());
            yield return new(MapUnits.FileName, new MapUnitsAdapter());
        }

        private static IEnumerable<KeyValuePair<string, IMapFileAdapter>> GetAdaptersByFileExtension()
        {
            yield return new(MapSounds.FileExtension, new MapSoundsAdapter());
            yield return new(".flac", new FlacAdapter());
            yield return new(".mp3", new Mp3Adapter());
            yield return new(".ogg", new OggAdapter());
            yield return new(".wav", new WavAdapter());

            yield return new(".blp", new BlpImageAdapter());
            yield return new(".dds", new DdsImageAdapter());
            yield return new(".tga", new TgaImageAdapter());

            yield return new(MapCameras.FileExtension, new MapCamerasAdapter());
            yield return new(MapEnvironment.FileExtension, new MapEnvironmentAdapter());
            yield return new(MapPathingMap.FileExtension, new MapPathingMapAdapter());
            yield return new(MapPreviewIcons.FileExtension, new MapPreviewIconsAdapter());
            yield return new(MapRegions.FileExtension, new MapRegionsAdapter());
            yield return new(MapShadowMap.FileExtension, new MapShadowMapAdapter());

            yield return new(CampaignInfo.FileExtension, new CampaignInfoAdapter());
            yield return new(MapInfo.FileExtension, new MapInfoAdapter());

            yield return new(".mdx", new BinaryModelAdapter());
            yield return new(".mdl", new TextModelAdapter());

            yield return new(AbilityObjectData.FileExtension, new AbilityObjectDataAdapter());
            yield return new(BuffObjectData.FileExtension, new BuffObjectDataAdapter());
            yield return new(DestructableObjectData.FileExtension, new DestructableObjectDataAdapter());
            yield return new(DoodadObjectData.FileExtension, new DoodadObjectDataAdapter());
            yield return new(ItemObjectData.FileExtension, new ItemObjectDataAdapter());
            // TODO: .w3o
            yield return new(UnitObjectData.FileExtension, new UnitObjectDataAdapter());
            yield return new(UpgradeObjectData.FileExtension, new UpgradeObjectDataAdapter());

            yield return new(MapCustomTextTriggers.FileExtension, new MapCustomTextTriggersAdapter());
            yield return new(MapTriggers.FileExtension, new MapTriggersAdapter());
            yield return new(TriggerStrings.FileExtension, new TriggerStringsAdapter());
            yield return new(JassMapScript.FileName.GetFileExtension(), new JassMapScriptAdapter());
            yield return new(LuaMapScript.FileName.GetFileExtension(), new MapScriptRegexAdapter());
        }
    }
}