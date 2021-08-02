/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TagLib;

namespace MusicCatalog.Converters
{
    public class AlbumArtConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                //BitmapImage bi = new BitmapImage();
                //bi.BeginInit();

                //bi.UriSource = new Uri(@"C:\Users\blake\AppData\Local\MusicCatalog\Cache\Unknown.png",
                //    UriKind.Absolute);

                var tags = TagLib.File.Create(value.ToString());

                if (tags.Tag == null)
                {
                    return App.DefaultAlbumArt;
                }

                var cs = tags.Tag.Pictures.FirstOrDefault();

                if (cs == default(IPicture)
                    || cs.Type == PictureType.NotAPicture
                    || cs.Type == PictureType.Other)
                {
                    return App.DefaultAlbumArt;
                }

                try
                {
                    using (var stream = new MemoryStream(cs.Data.Data))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        //bitmap.DecodePixelHeight = 256;
                        //bitmap.DecodePixelHeight = 256;
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    return App.DefaultAlbumArt;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
