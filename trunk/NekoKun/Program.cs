using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ICSharpCode.Core;
using orzTech.NekoKun.Base;

namespace orzTech.NekoKun
{
    public static class Start
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ToolStripManager.Renderer = new PhilWright.Office2007Renderer.Office2007Renderer();

            LoggingService.Info("Application start");
            Assembly exe = typeof(Start).Assembly;

            FileUtility.ApplicationRootPath = Path.GetDirectoryName(exe.Location);

            LoggingService.Info("Starting core services...");

            CoreStartup coreStartup = new CoreStartup("NekoKun");

            coreStartup.PropertiesName = "AppProperties";

            coreStartup.StartCoreServices();

            // Registeres the default (English) strings and images. They are compiled as
            // "EmbeddedResource" into Startup.exe.
            // Localized strings are automatically picked up when they are put into the
            // "data/resources" directory.
            ResourceService.RegisterNeutralStrings(new ResourceManager("orzTech.NekoKun.StringResources", exe));
            ResourceService.RegisterNeutralImages(new ResourceManager("orzTech.NekoKun.ImageResources", exe));

            LoggingService.Info("Looking for AddIns...");
            // Searches for ".addin" files in the application directory.
            coreStartup.AddAddInsFromDirectory(Path.Combine(FileUtility.ApplicationRootPath, "AddIns"));

            // Searches for a "AddIns.xml" in the user profile that specifies the names of the
            // add-ins that were deactivated by the user, and adds "external" AddIns.
            coreStartup.ConfigureExternalAddIns(Path.Combine(PropertyService.ConfigDirectory, "AddIns.xml"));

            // Searches for add-ins installed by the user into his profile directory. This also
            // performs the job of installing, uninstalling or upgrading add-ins if the user
            // requested it the last time this application was running.
            coreStartup.ConfigureUserAddIns(Path.Combine(PropertyService.ConfigDirectory, "AddInInstallTemp"),
                                            Path.Combine(PropertyService.ConfigDirectory, "AddIns"));

            LoggingService.Info("Loading AddInTree...");
            // Now finally initialize the application. This parses the ".addin" files and
            // creates the AddIn tree. It also automatically runs the commands in
            // "/Workspace/Autostart"
            coreStartup.RunInitialization();

            LoggingService.Info("Initializing Workbench...");
            // Workbench is our class from the base project, this method creates an instance
            // of the main form.
            Workbench.InitializeWorkbench();

            try
            {
                LoggingService.Info("Running application...");
                // Workbench.Instance is the instance of the main form, run the message loop.
                Application.Run(Workbench.Instance);
            }
            finally
            {
                try
                {
                    // Save changed properties
                    PropertyService.Save();
                }
                catch (Exception ex)
                {
                    MessageService.ShowError(ex, "Error storing properties");
                }
            }

            LoggingService.Info("Application shutdown");
        }
    }
}
