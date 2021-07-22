/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows.Input;

namespace MusicCatalog.Common.Commands
{
    public static class DataTemplateCommands
    {
        public static RoutedCommand CopyFilePath = new RoutedCommand("CopyFilePath", typeof(DataTemplateCommands));
    }
}
