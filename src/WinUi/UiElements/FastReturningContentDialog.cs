using Microsoft.UI.Xaml.Controls;

namespace WinUi.UiElements;

public class FastReturningContentDialog : ContentDialog
{
    /// <summary>
    /// Ordinarily, the ContentDialog returns with some delay. This method returns control as soon as any button is clicked
    /// </summary>
    public Task<ContentDialogResult> ShowWithFastReturnAsync()
    {
        var tcs = new TaskCompletionSource<ContentDialogResult>();

        PrimaryButtonClick += OnPrimaryClick;
        SecondaryButtonClick += OnSecondaryClick;
        CloseButtonClick += OnCloseClick;

        _ = ShowAsync();

        return tcs.Task.ContinueWith(x =>
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                PrimaryButtonClick -= OnPrimaryClick;
                SecondaryButtonClick -= OnSecondaryClick;
                CloseButtonClick -= OnCloseClick;
            });

            return x.Result;
        });

        void OnPrimaryClick(ContentDialog c, ContentDialogButtonClickEventArgs a)
        {
            tcs.SetResult(ContentDialogResult.Primary);
        }

        void OnSecondaryClick(ContentDialog c, ContentDialogButtonClickEventArgs a)
        {
            tcs.SetResult(ContentDialogResult.Secondary);
        }

        void OnCloseClick(ContentDialog c, ContentDialogButtonClickEventArgs a)
        {
            tcs.SetResult(ContentDialogResult.None);
        }
    }
}