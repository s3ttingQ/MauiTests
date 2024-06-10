using RGPopup.Maui.Extensions;
using RGPopup.Maui.Pages;
using RGPopup.Maui.Services;
using static MauiTestField.Common;

namespace MauiTestField.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterSerial : PopupPage
    {
        TaskCompletionSource<bool> _tcs = null;

        public EnterSerial()
        {
            InitializeComponent();

            SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
            btnConnect.Clicked += async (sender, e) =>
            {
                if (mutex.CurrentCount == 0)
                {
                    return;
                }

                try
                {
                    await mutex.WaitAsync();

                    await Navigation.RemovePopupPageAsync(this);
                    SetTaskResult(true);
                    await new ScanPage().OnScanProcedureAsync(tbSerial.Text);
                }
                finally
                {
                    mutex.Release();
                }
            };

            btnClose.Clicked += async (sender, e) =>
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

            Appearing += (s, e) =>
            {
                tbSerial.Focus();
            };          
        }

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