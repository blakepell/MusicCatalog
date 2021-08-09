/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ModernWpf;
using System.Windows.Media;

namespace MusicCatalog.Theme
{
    /// <summary>
    /// Extension of <see cref="ColorPaletteResources"/> with an additional key used to
    /// identify and find this class in the applications merged resources.
    /// </summary>
    public class ColorPaletteResourcesEx : ColorPaletteResources
    {
        public ColorPaletteResourcesEx()
        {
            this.Add("ColorPalette_AppearanceService", Brushes.White);
        }
    }
}
