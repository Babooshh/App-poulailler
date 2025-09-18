namespace App_poulailler
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnOuvrirClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Action", "Ouvrir", "OK");
        }

        private void OnFermerClicked(object? sender, EventArgs e)
        {
            DisplayAlert("Action", "Fermer", "OK");
        }
    }
}
