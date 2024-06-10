using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using MauiTestField.Views;

namespace MauiTestField
{
    public static class BLEManager
    {
        public static IBluetoothLE BLE { get; } = CrossBluetoothLE.Current;
        public static IAdapter BLEAdapter { get; } = CrossBluetoothLE.Current.Adapter;
        private static EventHandler<DeviceEventArgs> onDiscover;

        static BLEManager()
        {
            BLEAdapter.ScanMode = ScanMode.LowLatency;
        }

        public static async Task<bool> CheckAdapterStatusAsync()
        {
            BluetoothState bleState = BLE.State;

            if (bleState == BluetoothState.Unavailable)
            {
                await App.Current.MainPage.DisplayAlert("BLE not supported", "Bluetooth low energy is not supported on the device.", "OK").ConfigureAwait(false);

                return false;
            }

            if (bleState != BluetoothState.On)
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS && bleState == BluetoothState.Unauthorized)
                {
                    if (await App.Current.MainPage.DisplayAlert("Bluetooth permission needed",
                        $"Bluetooth permission needs to be given to connect and discover.{Environment.NewLine}{Environment.NewLine}Do you want to go to settings to give the bluetooth permission?",
                        "Yes", "No").ConfigureAwait(false))
                    {
                        AppInfo.Current.ShowSettingsUI();
                    }

                    return false;
                }

                await App.Current.MainPage.DisplayAlert("Bluetooth needed", "Please enable bluetooth to discover and connect to modules.", "OK");
            }
            else
            {
                return true;
            }

            return false;
        }

        public static async Task<IDevice> DiscoverDeviceAsync(string serialNumberInHex, Guid deviceId = default)
        {
            IDevice deviceFound = null;

            while (true)
            {
                try
                {
                    LoadingPopUp loadingPopUp =
                        new LoadingPopUp(
                            $"Discovering ...{Environment.NewLine}The process will take up to {BLEAdapter.ScanTimeout / 1000} seconds.",
                            false);

                    onDiscover = new EventHandler<DeviceEventArgs>(async (s, a) =>
                    {
                        if (deviceId != default)
                        {
                            if (a.Device.Id == deviceId)
                            {
                                await Task.Run(async () =>
                                {
                                    deviceFound = a.Device;
                                    await BLEAdapter.StopScanningForDevicesAsync().ConfigureAwait(false);
                                }).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            if (a.Device.Name != null)
                            {
                                await Task.Run(async () =>
                                {
                                    if (a.Device.Name.ToUpper().StartsWith("XXX") && a.Device.Name.Length > 13 &&
                                        a.Device.Name.Contains(serialNumberInHex))
                                    {
                                        deviceFound = a.Device;
                                        await BLEAdapter.StopScanningForDevicesAsync().ConfigureAwait(false);
                                    }
                                }).ConfigureAwait(false);
                            }
                        }
                    });

                    BLEAdapter.DeviceDiscovered += onDiscover;

                    var discoverTask = BLEAdapter.StartScanningForDevicesAsync();
                    var loadingPopUpTask = loadingPopUp.ShowAsync();

                    try
                    {
                        await discoverTask.ConfigureAwait(false);
                    }
                    finally
                    {
                        await loadingPopUp.CloseAsync().ConfigureAwait(false);

                        await loadingPopUpTask.ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    BLEAdapter.DeviceDiscovered -= onDiscover;

                    return null;
                }

                if (deviceFound == null)
                {
                    // This part is working
                    /*bool res = true;
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        res = await App.Current.MainPage.DisplayAlert("Discover failed",
                            $"The scanned item could not be discovered.{Environment.NewLine}{Environment.NewLine}Do you want to try again?", "Yes", "No");
                    });

                    if (res == false) 
                    {
                        BLEAdapter.DeviceDiscovered -= onDiscover;

                        return null;
                    }*/


                    // This is the part that is not working TODO
                    if (!await new AlertPopUp("Discover failed",
                            $"The scanned item could not be discovered.{Environment.NewLine}{Environment.NewLine}Do you want to try again?",
                            "Yes", "No").ShowAsync().ConfigureAwait(false))
                    {
                        BLEAdapter.DeviceDiscovered -= onDiscover;

                        return null;
                    }
                }
                else
                {
                    BLEAdapter.DeviceDiscovered -= onDiscover;

                    return deviceFound;
                }
            }
        }
    }
}