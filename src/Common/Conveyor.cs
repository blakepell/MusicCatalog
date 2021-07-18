/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Windows;

namespace MusicCatalog.Common
{
    /// <summary>
    /// Provides thread safe access to common and persistent UI elements on the <see cref="MainWindow"/>.
    /// </summary>
    public class Conveyor
    {
        private MainWindow _mainWindow;

        private AppSettings _appSettings;

        public Conveyor(MainWindow win, AppSettings settings)
        {
            _appSettings = settings;
            _mainWindow = win;
        }

        /// <summary>
        /// Updates the information overlay status.
        /// </summary>
        /// <param name="topText">The top text.  Blank sets text to blank but null will ignore set operation and leave the current text in place.</param>
        /// <param name="bottomText">The bottom text.  Blank sets text to blank but null will ignore set operation and leave the current text in place.</param>
        /// <param name="visible"></param>
        /// <param name="progressIsActive"></param>
        public void UpdateInfoOverlay(string topText, string bottomText, bool visible, bool progressIsActive)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => UpdateInfoOverlay(topText, bottomText, visible, progressIsActive)));
                return;
            }

            _mainWindow.InfoOverlay.ProgressIsActive = progressIsActive;
            _mainWindow.InfoOverlay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            if (topText != null)
            {
                _mainWindow.InfoOverlay.TopText = topText;
            }

            if (bottomText != null)
            {
                _mainWindow.InfoOverlay.BottomText = bottomText;
            }
        }

        /// <summary>
        /// Hides the info overlay.
        /// </summary>
        public void HideInfoOverlay()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(this.HideInfoOverlay));
                return;
            }

            _mainWindow.InfoOverlay.ProgressIsActive = false;
            _mainWindow.InfoOverlay.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the now playing information.
        /// </summary>
        /// <param name="songTitle"></param>
        /// <param name="artist"></param>
        /// <param name="album"></param>
        public void SetNowPlaying(string songTitle, string artist, string album)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(this.HideInfoOverlay));
                return;
            }

            _mainWindow.NowPlayingSongTitle = songTitle;
            _mainWindow.NowPlayingArtist = artist;
            _mainWindow.NowPlayingAlbum = album;
        }


    }
}
