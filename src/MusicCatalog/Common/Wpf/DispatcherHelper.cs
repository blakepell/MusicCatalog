/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 /*

/*
 * DispatcherHelper.cs originates from:
 *
 * @project           : ModernWpf
 * @project lead      : Kinnara
 * @website           : https://github.com/Kinnara/ModernWpf
 * @license           : MIT
 */

using System;
using System.Windows;
using System.Windows.Threading;

namespace MusicCatalog.Common.Wpf
{
    /// <summary>
    /// Methods to help with common <see cref="Dispatcher"/> tasks.
    /// </summary>
    public static class DispatcherHelper
    {
        /// <summary>
        /// Runs an action on the main thread (Application.Current).
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnMainThread(Action action)
        {
            RunOnUIThread(Application.Current, action);
        }

        /// <summary>
        /// Runs an action on the <see cref="Dispatcher"/> for the <see cref="DispatcherObject" />.
        /// This will first check if the caller has access and if not then BeginInvoke with the
        /// dispatcher (otherwise it will simply run it if it already has access).
        /// </summary>
        /// <param name="d"></param>
        /// <param name="action"></param>
        public static void RunOnUIThread(this DispatcherObject d, Action action)
        {
            var dispatcher = d.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }
    }
}