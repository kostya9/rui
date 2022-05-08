using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RedisServers : Page
{
    private LoadedServers? _connections;

    public RedisServers()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        this._connections = ((RedisServerNavigationParameters)e.Parameter).Servers;
        base.OnNavigatedTo(e);
    }

    private async void addNewServerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_connections == null)
            return;

        var addRedisServerDialog = new AddRedisServerDialog();
        addRedisServerDialog.XamlRoot = Content.XamlRoot;

        var result = await addRedisServerDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (addRedisServerDialog.Result != null)
            {
                _connections.RedisServers.Add(addRedisServerDialog.Result);
            }
        }
    }

    private void Servers_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        
    }

    public class RedisServerNavigationParameters
    {
        public LoadedServers Servers { get; set; } = new();
    }
}
