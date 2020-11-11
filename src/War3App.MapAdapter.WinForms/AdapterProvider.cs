#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using War3App.MapAdapter.Audio;
using War3App.MapAdapter.Environment;
using War3App.MapAdapter.Info;
using War3App.MapAdapter.Modeling;
using War3App.MapAdapter.Mpq;
using War3App.MapAdapter.Object;
using War3App.MapAdapter.PreviewIcons;
using War3App.MapAdapter.Regions;
using War3App.MapAdapter.Script;
using War3App.MapAdapter.Widget;

namespace War3App.MapAdapter.WinForms
{
    internal static class AdapterProvider
    {
        private static readonly IList<IMapFileAdapter> _adapters = InitAdapters().ToList();

        internal static IMapFileAdapter? GetAdapter(ItemTag tag)
        {
            return TryGetAdapter(tag.FileName, tag.OriginalFileStream, out var adapter) ? adapter : null;
        }

        internal static bool TryGetAdapter(string? fileName, Stream stream, [NotNullWhen(true)] out IMapFileAdapter? adapter)
        {
            adapter = fileName is null
                ? _adapters.SingleOrDefault(adapter => adapter.CanAdaptFile(stream))
                : _adapters.SingleOrDefault(adapter => adapter.CanAdaptFile(fileName) || adapter.CanAdaptFile(stream));

            return adapter != null;
        }

        private static IEnumerable<IMapFileAdapter> InitAdapters()
        {
            yield return new FlacAdapter();
            yield return new MapSoundsAdapter();
            yield return new Mp3Adapter();
            yield return new OggAdapter();
            yield return new WavAdapter();

            yield return new BlpImageAdapter();
            yield return new TgaImageAdapter();

            yield return new MapCamerasAdapter();
            yield return new MapEnvironmentAdapter();
            yield return new MapPreviewIconsAdapter();
            yield return new MapRegionsAdapter();
            yield return new PathingMapAdapter();
            yield return new ShadowMapAdapter();

            yield return new CampaignInfoAdapter();
            yield return new MapInfoAdapter();

            yield return new BinaryModelAdapter();
            yield return new TextModelAdapter();

            yield return new AttributesAdapter();
            yield return new ListFileAdapter();

            yield return new AbilityObjectDataAdapter();
            yield return new BuffObjectDataAdapter();
            yield return new DestructableObjectDataAdapter();
            yield return new DoodadObjectDataAdapter();
            yield return new ItemObjectDataAdapter();
            // yield return new ObjectDataAdapter();
            yield return new UnitObjectDataAdapter();
            yield return new UpgradeObjectDataAdapter();

            yield return new MapCustomTextTriggersAdapter();
            yield return new MapTriggersAdapter();
            yield return new MapTriggerStringsAdapter();
            yield return new MapScriptRegexAdapter();

            yield return new MapDoodadsAdapter();
            yield return new MapUnitsAdapter();
        }
    }
}