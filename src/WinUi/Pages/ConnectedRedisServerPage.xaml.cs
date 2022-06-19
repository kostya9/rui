using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinUi.Annotations;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ConnectedRedisServerPage : Page
{
    private ConnectedRedisServerPageNavigationArgs? _navArgs;

    private readonly ConnectedPage _page = new();

    public ConnectedRedisServerPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navArgs = (ConnectedRedisServerPageNavigationArgs)e.Parameter;

        _page.Page = new CurrentConnectedPage(_navArgs.ConnectedServer);

        if (_page.Page.ConnectedServer.VisibleKeys.Count == 0)
        {
            var _ = LoadKeysAsync();
        }

        base.OnNavigatedTo(e);
    }

    private async Task LoadKeysAsync()
    {
        _page.Page.ConnectedServer.State = ConnectedRedisServer.State_.Refreshing;

        try
        {
            var connectedMultiplexer = _page.Page.ConnectedServer.Connection;
            var endpoints = connectedMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = connectedMultiplexer.GetServer(endpoint);
                const int limit = 50;
                
                var visibleKeys = _page.Page.ConnectedServer.VisibleKeys;
                var clientKeys = visibleKeys.ToHashSet();

                var serverKeys = new HashSet<RedisKey>(_page.Page.ConnectedServer.VisibleKeys.Count);
                await foreach (var key in server.KeysAsync(pageSize: limit))
                {
                    var stringKey = key.ToString();
                    serverKeys.Add(new RedisKey(stringKey));
                }

                // TODO: I mean, this is calculating a full diff, do we need it in every case?
                // or can we do better?
                var toAdd = new HashSet<RedisKey>(serverKeys);
                toAdd.ExceptWith(clientKeys);
                foreach (var item in toAdd)
                {
                    visibleKeys.Add(item);
                }

                var toRemove = new HashSet<RedisKey>(clientKeys);
                toRemove.ExceptWith(serverKeys);
                foreach (var item in toRemove)
                {
                    visibleKeys.Remove(item);
                }
            }
        }
        finally
        {
            _page.Page.ConnectedServer.State = ConnectedRedisServer.State_.Idle;
        }
    }

    public record ConnectedRedisServerPageNavigationArgs(ConnectedRedisServer ConnectedServer);

    private record CurrentConnectedPage(ConnectedRedisServer ConnectedServer);

    private class ConnectedPage : INotifyPropertyChanged
    {
        private CurrentConnectedPage _page = null!;
        public CurrentConnectedPage Page
        {
            get
            {
                return _page;
            }
            set
            {
                if(_page == value)
                    return;

                _page = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    };

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        _ = LoadKeysAsync();
    }

    private void flyoutOpen_Click(object sender, object e)
    {
        if (sender is MenuFlyout { Target: ListViewItem listViewItem } menu)
        {
            listViewItem.IsSelected = true;
        }
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { DataContext: RedisKey key })
        {
            await this._page.Page.ConnectedServer.Connection.GetDatabase(0).KeyDeleteAsync(key.Key);
            _ = LoadKeysAsync();
        }
    }
}
