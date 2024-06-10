using static MauiTestField.Common;
using RGPopup.Maui.Pages;
using RGPopup.Maui.Services;
using RGPopup.Maui.Extensions;

namespace MauiTestField.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPopUp : PopupPage
    {
        TaskCompletionSource<bool> _tcs = null;

        public LoadingPopUp(string text, bool bCancellable = true)
        {
            InitializeComponent();

            lblText.Text = text;

            SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

            btnCancel.Clicked += async (sender, e) =>
            {
                if (mutex.CurrentCount == 0)
                {
                    return;
                }

                await mutex.WaitAsync();

                await CloseAsync();

                mutex.Release();
            };

            if (!bCancellable)
            {
                btnCancel.IsVisible = false;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            SetTaskResult();
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

        public async Task CloseAsync()
        {
            await RunAsyncOnMainThreadAsync(async () =>
            {
                if (PopupNavigation.Instance.PopupStack.Contains(this))
                {
                    await Navigation.RemovePopupPageAsync(this);
                }
            }).ConfigureAwait(false);

            SetTaskResult();
        }

        private void SetTaskResult()
        {
            if (_tcs?.Task.Status != TaskStatus.RanToCompletion &&
                _tcs?.Task.Status != TaskStatus.Faulted &&
                _tcs?.Task.Status != TaskStatus.Canceled)
            {
                _tcs?.SetResult(true);
            }
        }
    }
}