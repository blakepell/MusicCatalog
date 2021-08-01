/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 /*

/*
 * ThemeManagerProxy.cs originates from:
 *
 * @project           : ModernWpf
 * @project lead      : Kinnara
 * @website           : https://github.com/Kinnara/ModernWpf
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ModernWpf;
using MusicCatalog.Common;

namespace MusicCatalog.Theme
{
    public class AppThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case ApplicationTheme.Light:
                    return AppTheme.Light;
                case ApplicationTheme.Dark:
                    return AppTheme.Dark;
                default:
                    return AppTheme.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppTheme appTheme)
            {
                return appTheme.Value;
            }

            return AppTheme.Default;
        }
    }
}
