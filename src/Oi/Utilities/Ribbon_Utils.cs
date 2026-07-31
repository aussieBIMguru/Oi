using System.IO;
using System.Windows.Media.Imaging;

// The class belongs to the Utilities namespace
namespace Oi.Utilities
{
    /// <summary>
    /// Static methods container related to the Ribbon.
    /// </summary>
    public static class Ribbon_Utils
    {
        /// <summary>
        /// Converts a command class to a base name for tooltip/icon finding.
        /// </summary>
        /// <param name="commandClass">The name of the command class.</param>
        /// <returns>A string.</returns>
        public static string CommandClassToBaseName(string commandClass)
        {
            // Example: AddinName.Commands.Cmds_Example.Cmd_Example
            // Step 1: Example.Cmd_Example
            // Step 2: Example_Example

            // Example: AddinName.Commands.Cmds_Example
            // Step 1: Example
            // Step 2: Example
            return commandClass.Replace($"{Globals.ADDIN_NAME}.Commands.Cmds_", "").Replace(".Cmd", "");
        }

        /// <summary>
        /// Creates PushButtonData (to stack, generally).
        /// </summary>
        /// <typeparam name="CommandClass">The related Command class.</typeparam>
        /// <param name="buttonName">The name for the button.</param>
        /// <returns>A PushButtonData object</returns>
        public static PushButtonData NewPushButtonData<CommandClass>(string buttonName)
        {
            // Strip the command class name to basics
            string commandClass = typeof(CommandClass).FullName;
            string baseName = CommandClassToBaseName(commandClass);

            // Make pushbuttondata
            var pushButtonData = new PushButtonData(baseName, buttonName, Globals.ADDIN_ASSEMBLY_PATH, commandClass)
            {
                ToolTip = UtilDat.GetDictValue(Globals.DICT_TOOLTIPS, baseName),
                LargeImage = GetImageSource(baseName, resolution: 32),
                Image = GetImageSource(baseName, resolution: 16)
            };

            // Return the data
            return pushButtonData;
        }

        /// <summary>
        /// Creates PulldownButtonData (to stack, generally).
        /// </summary>
        /// <param name="buttonName">The name for the button.</param>
        /// <param name="nameSpace">The namespace the commands relate to.</param>
        /// <returns>A PulldownButtonData object.</returns>
        public static PulldownButtonData NewPulldownButtonData(string buttonName, string nameSpace)
        {
            // Strip the command class name to basics
            string baseName = CommandClassToBaseName(nameSpace);

            // Make pushbuttondata
            var pulldownButtonData = new PulldownButtonData(baseName, buttonName)
            {
                ToolTip = UtilDat.GetDictValue(Globals.DICT_TOOLTIPS, baseName),
                LargeImage = GetImageSource(baseName, resolution: 32),
                Image = GetImageSource(baseName, resolution: 16)
            };

            // Return the data
            return pulldownButtonData;
        }

        /// <summary>
        /// Prepares an image source from a Png resource.
        /// </summary>
        /// <param name="iconName">The name of the icon (without format, resolution).</param>
        /// <param name="resolution">The resolution suffix (16 or 32, typically).</param>
        /// <param name="suffix">An additional suffix (optional).</param>
        /// <returns>An ImageSource object.</returns>
        public static System.Windows.Media.ImageSource GetImageSource(string iconName, int resolution = 32, string suffix = "")
        {
            // Construct the resource path
            string resourcePath = $"{Globals.ADDIN_NAME}.Resources.Icons{resolution}.{iconName}{resolution}{suffix}.png";

            // Read the resource from its full path
            using (Stream stream = Globals.ADDIN_ASSEMBLY.GetManifestResourceStream(resourcePath))
            {
                // Throw exception if stream not made
                if (stream == null)
                {
                    return null;
                }

                // Decode the png resource
                PngBitmapDecoder decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.Default);

                // Decode to image source
                return decoder.Frames[0];
            }
        }
    }
}