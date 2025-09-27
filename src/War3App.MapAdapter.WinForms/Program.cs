using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using War3App.MapAdapter.Constants;
using War3App.MapAdapter.WinForms.Forms;

namespace War3App.MapAdapter.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (!File.Exists(FileName.AppSettings))
            {
                var initialSetupDialog = new ConfigureGamePathForm();

                var initialSetupDialogResult = initialSetupDialog.ShowDialog();
                if (initialSetupDialogResult != DialogResult.OK)
                {
                    return;
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

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(FileName.AppSettings, optional: true)
                .Build();

            new MainForm(configuration).ShowDialog();
        }
    }
}