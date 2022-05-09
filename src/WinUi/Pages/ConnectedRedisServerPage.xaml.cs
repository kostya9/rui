using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ConnectedRedisServerPage : Page
{
    private ConnectedRedisServerPageNavigationArgs? _navArgs;

    public ConnectedRedisServerPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navArgs = (ConnectedRedisServerPageNavigationArgs)e.Parameter;
        base.OnNavigatedTo(e);
    }

    public record ConnectedRedisServerPageNavigationArgs(ConnectedRedisServer ConnectedServer);
}
