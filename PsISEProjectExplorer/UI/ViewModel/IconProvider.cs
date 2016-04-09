using GongSolutions.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PsISEProjectExplorer.UI.ViewModel
{
    public class IconProvider
    {
        private IDictionary<String, BitmapImage> iconsMap = new Dictionary<String, BitmapImage>();

        public IconProvider()
        {
            foreach (string fileName in this.GetNodeResourceNames())
            {
                string shortName = fileName.Replace(@"resources/node_", "").Replace(".png", "").ToLowerInvariant();
                iconsMap.Add(shortName, GetBitmapImage(fileName));
            }
        }

        private List<String> GetNodeResourceNames()
        {
            var asm = Assembly.GetExecutingAssembly();
            string resName = asm.GetName().Name + ".g.resources";
            using (var stream = asm.GetManifestResourceStream(resName))
            using (var reader = new System.Resources.ResourceReader(stream))
            {
                return reader.Cast<DictionaryEntry>().Select(entry => (string)entry.Key).Where(key => key.Contains("node_")).ToList();
            }
        }
    

        public ImageSource GetImageSourceForFileSystemEntry(string path, bool isExcluded, bool isValid)
        {
            try
            {
                ShellItem shellItem = new ShellItem(new Uri(path));
                var icon = shellItem.ShellIcon;
                ImageSource shellIcon = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                if (!isExcluded && isValid)
                {
                    return shellIcon;
                }

                BitmapImage overlayIcon;
                if (!isValid)
                {
                    overlayIcon = iconsMap["overlay_invalid"];
                }
                else
                {
                    overlayIcon = iconsMap["overlay_excluded"];
                }

                Rect rect = new Rect(new Size(16, 16));
                DrawingGroup iconOverlays = new DrawingGroup();
                iconOverlays.Children.Add(new ImageDrawing(shellIcon, rect));
                iconOverlays.Children.Add(new ImageDrawing(overlayIcon, rect));

                return new DrawingImage(iconOverlays);
            }
            catch (Exception e)
            {
                if (File.Exists(path))
                {
                    return iconsMap["file"];
                }
                if (Directory.Exists(path))
                {
                    return iconsMap["directory"];
                }
                return null;
            }
        }

        public ImageSource GetImageSourceForPowershellItemEntry(string nodeType)
        {
            BitmapImage result;
            bool found = iconsMap.TryGetValue(nodeType.ToLowerInvariant(), out result);
            return (found ? result : null);
        }

        private BitmapImage GetBitmapImage(String fileName)
        {
            return new BitmapImage(new Uri(@"pack://application:,,,/PsISEProjectExplorer;component/" + fileName));
        }
    }
}
