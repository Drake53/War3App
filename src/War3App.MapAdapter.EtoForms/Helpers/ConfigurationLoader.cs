using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eto.Forms;
using Microsoft.Extensions.Configuration;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.EtoForms.Forms;

namespace War3App.MapAdapter.EtoForms.Helpers
{
    public static class ConfigurationLoader
    {
        public static IConfigurationRoot? LoadOrInitialize()
        {
            if (!File.Exists(FileName.AppSettings))
            {
                var initialSetupDialog = new ConfigureGamePathForm();

                var initialSetupDialogResult = initialSetupDialog.ShowModal();
                if (initialSetupDialogResult != DialogResult.Ok)
                {
                    return null;
                }

                var appSettings = new AppSettings
                {
                    TargetPatches = new List<TargetPatch>()
                    {
                        new()
                        {
                            GameDataPath = initialSetupDialog.GameDirectory,
                            Patch = initialSetupDialog.GamePatch,
                            GameDataContainerType = ContainerType.Directory,
                        },
                    },
                };

                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                File.WriteAllText(FileName.AppSettings, JsonSerializer.Serialize(appSettings, jsonSerializerOptions));
            }

            return new ConfigurationBuilder()
                .AddJsonFile(FileName.AppSettings, optional: true)
                .Build();
        }
    }
}