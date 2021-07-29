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

using ModernWpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MusicCatalog.Theme
{
    public class AppThemes : List<AppTheme>
    {
        public AppThemes()
        {
            Add(AppTheme.Light);
            Add(AppTheme.Dark);
            Add(AppTheme.Default);
        }
    }

    public class AppTheme
    {
        public static AppTheme Light { get; } = new AppTheme("Light", ApplicationTheme.Light);
        public static AppTheme Dark { get; } = new AppTheme("Dark", ApplicationTheme.Dark);
        public static AppTheme Default { get; } = new AppTheme("Use system setting", null);

        private AppTheme(string name, ApplicationTheme? value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public ApplicationTheme? Value { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}