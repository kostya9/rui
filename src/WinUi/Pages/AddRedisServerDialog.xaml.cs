using Microsoft.UI.Xaml.Controls;
using StackExchange.Redis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages
{
    public sealed partial class AddRedisServerDialog : ContentDialog
    {
        public RedisConnection? Result { get; private set; }

        public AddRedisServerDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // If you're performing async operations in the button click handler,
            // get a deferral before you await the operation. Then, complete the
            // deferral when the async operation is complete.
            ContentDialogButtonClickDeferral deferral = args.GetDeferral();

            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
