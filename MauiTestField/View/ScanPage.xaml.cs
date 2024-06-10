using Plugin.BLE.Abstractions.Contracts;
using static MauiTestField.Common;


namespace MauiTestField.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanPage : ContentPage
    {        
        public bool AppearCalled { get; private set; }
        public bool DisappearCalled { get; private set; }

        public ScanPage()
        {
            InitializeComponent();          
        }

        public async Task OnScanProcedureAsync(string uintSerial, Guid deviceId = default)
        {
            if (deviceId == default)
            {
                if (uintSerial == null)
                {
                    return;
                }
            }

            await RunAsyncOnMainThreadAsync(async () =>
            {
                Application.Current.MainPage.IsEnabled = false;
                await Navigation.PopAsync();
            }).ConfigureAwait(false);

            if (await BLEManager.CheckAdapterStatusAsync().ConfigureAwait(false))
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    if (!await Common.IsLocationServiceEnabled().ConfigureAwait(false))
                    {
                        await RunAsyncOnMainThreadAsync(async () =>
                        {
                            Application.Current.MainPage.IsEnabled = true;
                        }).ConfigureAwait(false);

                        return;
                    }
                }

                IDevice deviceFound;

                if (deviceId == default)
                {
                    deviceFound = await BLEManager.DiscoverDeviceAsync(uintSerial).ConfigureAwait(false);
                }
                else
                {
                    deviceFound = await BLEManager.DiscoverDeviceAsync("", deviceId).ConfigureAwait(false);
                }

                if (deviceFound != null)
                {
                    // do something
                }
            }

            await RunAsyncOnMainThreadAsync(async () =>
            {
                Application.Current.MainPage.IsEnabled = true;
            }).ConfigureAwait(true);
        }       
    }
}