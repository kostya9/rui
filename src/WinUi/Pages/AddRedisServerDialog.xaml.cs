using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StackExchange.Redis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages
{
    public sealed partial class AddRedisServerDialog : ContentDialog
    {
        public RedisConnection? Result { get; private set; }

        public bool IsFormValid => !string.IsNullOrWhiteSpace(serverTxt.Text) && this.portTxt.Value > 0;

        public AddRedisServerDialog()
        {
            this.InitializeComponent();
            serverTxt.TextChanged += (_, _) => ValuesChanged();
            portTxt.ValueChanged += (_, _) => ValuesChanged();
        }

        /// <summary>
        /// This should be called every time computed values should be re-computed
        /// </summary>
        private void ValuesChanged()
        {
            this.Bindings.Update();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // If you're performing async operations in the button click handler,
            // get a deferral before you await the operation. Then, complete the
            // deferral when the async operation is complete.
            ContentDialogButtonClickDeferral deferral = args.GetDeferral();

            try
            {
                OnConnectionStart();

                var url = this.serverTxt.Text;
                var port = (int)this.portTxt.Value;
                var configOptions = new ConfigurationOptions()
                {
                    EndPoints =
                    {
                        { url, port }
                    },
                    User = this.usernameTxt.Text,
                    Password = this.passwordTxt.Password
                };
                await ConnectionMultiplexer.ConnectAsync(configOptions);
                this.Result = new RedisConnection(url, port, configOptions.User, configOptions.Password);
                OnSuccessfulConnection();
            }
            catch (Exception ex)
            {
                OnFailedConnection();
                args.Cancel = true;
                Console.WriteLine($"Failed to connect to redis: {ex}");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnConnectionStart()
        {
            this.IsPrimaryButtonEnabled = false;
            this.PrimaryButtonText = "Connecting...";
        }

        private void OnFailedConnection()
        {
            this.PrimaryButtonText = DefaultPrimaryButtonText;
            this.IsPrimaryButtonEnabled = true;
            this.errorTextBlock.Text = "Failed to connect";
        }

        private void OnSuccessfulConnection()
        {
            this.PrimaryButtonText = DefaultPrimaryButtonText;
            this.IsPrimaryButtonEnabled = true;
        }

        private readonly string DefaultPrimaryButtonText = "Add";
    }
}
