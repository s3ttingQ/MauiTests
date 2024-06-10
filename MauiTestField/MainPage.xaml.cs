using MauiTestField.Views;

namespace MauiTestField
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            IsEnabled = false;

            await new EnterSerial().ShowAsync();

            IsEnabled = true;
        }
    }

}
