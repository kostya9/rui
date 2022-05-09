using Microsoft.UI.Xaml.Controls;
using StackExchange.Redis;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUi.Pages;

public sealed partial class AddRedisServerDialog : ContentDialog
{
    public RedisServer? Result { get; private set; }

    private bool IsFormValid => !string.IsNullOrWhiteSpace(serverTxt.Text) && this.portTxt.Value > 0 && !string.IsNullOrWhiteSpace(nameTxt.Text);

    private bool FormEditingEnabled => _state == DialogState.Input;

    private bool IsPrimaryButtonAvailable => _state switch
    {
        DialogState.Input => IsFormValid,
        DialogState.Checking => false
    };

    // We cannot actually cancel checking state, so disabling that
    private bool IsSecondaryButtonAvailable => _state switch
    {
        DialogState.Input => true,
        DialogState.Checking => false
    };

    private string PrimaryButtonLabel => _state switch
    {
        DialogState.Input => "Add",
        DialogState.Checking => "Connecting..."
    };

    private DialogState _state = DialogState.Input;

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
        // We are already checking stuff
        if (_state == DialogState.Checking)
            return;

        // If you're performing async operations in the button click handler,
        // get a deferral before you await the operation. Then, complete the
        // deferral when the async operation is complete.
        ContentDialogButtonClickDeferral deferral = args.GetDeferral();

        try
        {
            OnConnectionStart();

            var name = this.nameTxt.Text;
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
            using var _ = await ConnectionMultiplexer.ConnectAsync(configOptions);
            this.Result = new RedisServer(name, url, port, configOptions.User, configOptions.Password);
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
        this.errorTextBlock.Text = "";

        _state = DialogState.Checking;
        this.ValuesChanged();
    }

    private void OnFailedConnection()
    {
        this.errorTextBlock.Text = "Failed to connect";

        this._state = DialogState.Input;
        this.ValuesChanged();
    }

    private void OnSuccessfulConnection()
    {
        _state = DialogState.Input;
        this.ValuesChanged();
    }

    private enum DialogState
    {
        Input, Checking
    }
}
