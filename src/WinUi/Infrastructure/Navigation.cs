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

    internal void NavigateToServersList()
    {
        if (_navigationView is null)
            return;

        _navigationView.DispatcherQueue.TryEnqueue(() => _navigationView.SelectedItem = _navigationView.MenuItems[0]);
    }

    public bool IsAtServer(ConnectedRedisServer connectedServer)
    {
        if (_navigationView is null)
            return false;

        var selectedItem = _navigationView.SelectedItem;

        if (selectedItem is NavigationViewItem { Tag: ConnectedRedisServer menuServer })
        {
            if (menuServer == connectedServer)
                return true;
        }

        return false;
    }
}
