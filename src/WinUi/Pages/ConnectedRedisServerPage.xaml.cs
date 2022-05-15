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

                _page.Page.ConnectedServer.VisibleKeys.Clear();
                _page.Page.ConnectedServer.VisibleKeys.Clear();
                await foreach (var key in server.KeysAsync(pageSize: limit))
                {
                    _page.Page.ConnectedServer.VisibleKeys.Add(new RedisKey(key));
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
}
