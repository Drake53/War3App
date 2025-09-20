using System;
using Eto.Forms;
using War3App.MapAdapter.EtoForms.Constants;
using War3App.MapAdapter.EtoForms.Forms;
using War3App.MapAdapter.EtoForms.Helpers;

namespace War3App.MapAdapter.EtoForms.Gtk
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Eto.Style.Add<Eto.GtkSharp.Forms.Menu.ButtonMenuItemHandler>(Styles.MenuIcons, handler =>
            {
                if (handler.Control is global::Gtk.ImageMenuItem imageMenuItem)
                {
#pragma warning disable CS0612 // Type or member is obsolete

                    imageMenuItem.AlwaysShowImage = true;
#pragma warning restore CS0612 // Type or member is obsolete

                }
            });

            var application = new Application(Eto.Platforms.Gtk);

            var configuration = ConfigurationLoader.LoadOrInitialize();
            if (configuration is null)
            {
                return;
            }

            application.Run(new MainForm(configuration));
        }
    }
}