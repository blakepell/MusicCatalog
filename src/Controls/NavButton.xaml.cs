/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using MusicCatalog.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicCatalog.Controls
{
    public partial class NavButton
    {
        public NavButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(NavButton), new PropertyMetadata("Menu Item"));

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty ActiveBrushProperty = DependencyProperty.Register(
            nameof(ActiveBrush), typeof(Brush), typeof(NavButton), new PropertyMetadata(Brushes.DodgerBlue));

        public Brush ActiveBrush
        {
            get => (Brush) GetValue(ActiveBrushProperty);
            set => SetValue(ActiveBrushProperty, value);
        }

        public static readonly DependencyProperty InactiveBrushProperty = DependencyProperty.Register(
            nameof(InactiveBrush), typeof(Brush), typeof(NavButton), new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#66CCCCCC")));

        public Brush InactiveBrush
        {
            get => (Brush) GetValue(InactiveBrushProperty);
            set => SetValue(InactiveBrushProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(NavButton), new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(IconControl), typeof(Control), typeof(NavButton));

        public object IconControl
        {
            get => this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty AssociatedFrameProperty = DependencyProperty.Register(
            nameof(AssociatedFrame), typeof(ModernWpf.Controls.Frame), typeof(NavButton), new PropertyMetadata(null));

        public ModernWpf.Controls.Frame AssociatedFrame
        {
            get => (ModernWpf.Controls.Frame) GetValue(AssociatedFrameProperty);
            set => SetValue(AssociatedFrameProperty, value);
        }

        public static readonly DependencyProperty NavigationPageTypeProperty = DependencyProperty.Register(
            nameof(NavigationPageType), typeof(string), typeof(NavButton), new PropertyMetadata(default(string)));

        public string NavigationPageType
        {
            get => (string) GetValue(NavigationPageTypeProperty);
            set => SetValue(NavigationPageTypeProperty, value);
        }

        public static readonly DependencyProperty SkipNavigateIfAlreadyLoadedProperty = DependencyProperty.Register(
            nameof(SkipNavigateIfAlreadyLoaded), typeof(bool), typeof(NavButton), new PropertyMetadata(false));

        public bool SkipNavigateIfAlreadyLoaded
        {
            get => (bool) GetValue(SkipNavigateIfAlreadyLoadedProperty);
            set => SetValue(SkipNavigateIfAlreadyLoadedProperty, value);
        }

        public static readonly DependencyProperty ClickBehaviorProperty = DependencyProperty.Register(
            nameof(ClickBehavior), typeof(NavItemClickBehavior), typeof(NavButton), new PropertyMetadata(NavItemClickBehavior.MakeActive));

        public NavItemClickBehavior ClickBehavior
        {
            get => (NavItemClickBehavior) GetValue(ClickBehaviorProperty);
            set => SetValue(ClickBehaviorProperty, value);
        }

        public event EventHandler Click;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            switch (this.ClickBehavior)
            {
                case NavItemClickBehavior.MakeActive:
                    var win = Window.GetWindow(this);

                    foreach (var navItem in win.FindVisualChildren<NavButton>())
                    {
                        navItem.IsActive = navItem == this;
                    }

                    break;
                case NavItemClickBehavior.ToggleIsActive:
                    this.IsActive = !this.IsActive;
                    break;
                case NavItemClickBehavior.Ignore:
                    break;
            }

            if (this.AssociatedFrame != null && !string.IsNullOrWhiteSpace(this.NavigationPageType))
            {
                Type t = Type.GetType(this.NavigationPageType);

                if (t != null)
                {
                    if (!this.SkipNavigateIfAlreadyLoaded)
                    {
                        this.AssociatedFrame.Navigate(t);
                    }
                    else
                    {
                        if (t != this.AssociatedFrame.CurrentSourcePageType)
                        {
                            this.AssociatedFrame.Navigate(t);
                        }
                    }
                }
            }

            this.Click?.Invoke(this, EventArgs.Empty);
        }
    }
}