using static MauiTestField.Common;
using RGPopup.Maui.Pages;
using RGPopup.Maui.Extensions;
using RGPopup.Maui.Services;

namespace MauiTestField
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AlertPopUp : PopupPage
    {
        TaskCompletionSource<bool> _tcs = null;

        public bool WasClosed { get; private set; }

        //Thanks to Sharada Gururaj - profile (https://stackoverflow.com/users/7292772/sharada-gururaj) - question - https://stackoverflow.com/questions/45791902/how-to-pause-a-method-till-getting-response-from-rg-plugins-popup-in-xamarin-for
        public AlertPopUp(string title, string message, string buttonOKTxt, string buttonCancelTxt = "", bool bCancellable = false)
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblMsg.Text = message;
            btnOK.Text = buttonOKTxt;

            if (buttonCancelTxt != "")
            {
                btnCancel.Text = buttonCancelTxt;
            }
            else
            {
                btnCancel.IsVisible = false;
            }

            SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
            btnOK.Clicked += async (sender, e) =>
            {
                if (mutex.CurrentCount == 0)
                {
                    return;
                }

                await mutex.WaitAsync();

                await Navigation.RemovePopupPageAsync(this);

                SetTaskResult(true);

                mutex.Release();
            };

            if (buttonCancelTxt != "")
            {
                btnCancel.Clicked += async (sender, e) =>
                {
                    if (mutex.CurrentCount == 0)
                    {
                        return;
                    }

                    await mutex.WaitAsync();

                    await Navigation.RemovePopupPageAsync(this);

                    SetTaskResult(false);

                    mutex.Release();
                };
            }

            if (bCancellable)
            {
                btnClose.IsVisible = true;

                btnClose.Clicked += async (s, e) =>
                {
                    if (mutex.CurrentCount == 0)
                    {
                        return;
                    }

                    await mutex.WaitAsync();

                    await Navigation.RemovePopupPageAsync(this);

                    SetTaskResult(false);

                    WasClosed = true;

                    mutex.Release();
                };
            }
        }

        //Thanks to Dilmah - profile (https://stackoverflow.com/users/3253932/dilmah) - question - https://stackoverflow.com/questions/43471846/disallow-closing-rg-plugin-popup-in-xamarin-forms
        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        public async Task<bool> ShowAsync()
        {
            _tcs = new TaskCompletionSource<bool>();

            await RunAsyncOnMainThreadAsync(async () =>
            {
                if (!PopupNavigation.Instance.PopupStack.Contains(this))
                {
                    await Navigation.PushPopupAsync(this);
                }
            }).ConfigureAwait(false);

            return await _tcs.Task.ConfigureAwait(false);
        }

        private void SetTaskResult(bool value)
        {
            if (_tcs?.Task.Status != TaskStatus.RanToCompletion &&
                _tcs?.Task.Status != TaskStatus.Faulted &&
                _tcs?.Task.Status != TaskStatus.Canceled)
            {
                _tcs?.SetResult(value);
            }
        }
    }
}