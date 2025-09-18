namespace App_poulailler
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnInterrupteurToggled(object? sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                DisplayAlert("Action", "Ouvrir", "OK");
            }
            else
            {
                DisplayAlert("Action", "Fermer", "OK");
            }
        }
    }
}
