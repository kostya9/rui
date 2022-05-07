using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using WinUi.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        ObservableCollection<RedisConnection> RedisConnections { get; set; } = new();

        public MainWindow()
        {
            this.InitializeComponent();
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
                    RedisConnections.Add(addRedisServerDialog.Result);
                }
            }
        }
    }

    public record RedisConnection(string Address, int Port, string Username, string Password);
}
