﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RedisServers : Page
    {
        private LoadedConnections _connections;

        public RedisServers()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this._connections = ((RedisServerNavigationParameters)e.Parameter).Connections;
            base.OnNavigatedTo(e);
        }

        private async void addNewServerBtn_Click(object sender, RoutedEventArgs e)
        {
            var addRedisServerDialog = new AddRedisServerDialog();
            addRedisServerDialog.XamlRoot = Content.XamlRoot;

            var result = await addRedisServerDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (addRedisServerDialog.Result != null)
                {
                    _connections.RedisConnections.Add(addRedisServerDialog.Result);
                }
            }
        }

        public class RedisServerNavigationParameters
        {
            public LoadedConnections Connections { get; set; }
        }
    }
}
