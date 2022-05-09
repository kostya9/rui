using Microsoft.UI.Xaml.Controls;

namespace WinUi.Infrastructure;
public class Navigation
{
    private NavigationView? _navigationView;

    public Navigation()
    {

    }

    public void InitializeNavigationBase(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    public void TryNavigateToServer(ConnectedRedisServer connectedServer)
    {
        if (_navigationView is null)
            return;

        foreach (var item in _navigationView.MenuItems)
        {
            if (item is NavigationViewItem { Tag: ConnectedRedisServer menuServer} && menuServer == connectedServer)
            {
                if (item == _navigationView.SelectedItem)
                    return;

                _ = _navigationView.DispatcherQueue.TryEnqueue(() => _navigationView.SelectedItem = item);
                return;
            }
        }
    }

}
