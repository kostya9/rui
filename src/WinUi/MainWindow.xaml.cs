using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;
using Windows.Foundation;
using WinUi.Pages;
using static WinUi.Pages.RedisServers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private LoadedConnections _connections = new();

        public MainWindow()
        {
            this.InitializeComponent();

            this.Closed += OnClosed;

            OnStarted();
        }

        private void OnStarted()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (folder != null)
            {
                var connectionsCachePath = Path.Combine(folder, "connections.json");
                if(File.Exists(connectionsCachePath))
                {
                    try
                    {
                        var connectionsData = File.ReadAllText(connectionsCachePath);
                        _connections = LoadedConnections.Deserialize(connectionsData);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Could not deserialize stateL {ex}");
                        _connections = new LoadedConnections();
                    }
                }
            }

            this.navigation.SelectedItem = this.navigation.MenuItems[0];
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (folder != null)
            {
                var connectionsCachePath = Path.Combine(folder, "connections.json");
                var serializedState = _connections.Serialize();
                File.WriteAllText(connectionsCachePath, serializedState);
            }
        }

        private void NavigationSelected(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            contentFrame.NavigateToType(typeof(RedisServers), new RedisServerNavigationParameters { Connections = _connections }, null);
        }
    }

    public record RedisConnection(string Address, int Port, string Username, string Password);

    public class LoadedConnections
    {
        public ObservableCollection<RedisConnection> RedisConnections { get; set; } = new();

        public string Serialize()
        {
            // TODO: Make a better structure
            return JsonSerializer.Serialize(this);
        }

        public static LoadedConnections Deserialize(string data)
        {
            return JsonSerializer.Deserialize<LoadedConnections>(data);
        }
    }
}
