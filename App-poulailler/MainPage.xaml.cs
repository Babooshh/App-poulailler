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
            if (sender is Switch interrupteur)
            {
                if (e.Value)
                {
                    interrupteur.ThumbColor = Colors.Green;
                    interrupteur.OnColor = Colors.Green;
                    DisplayAlert("Action", "Ouvrir", "OK");
                }
                else
                {
                    interrupteur.ThumbColor = Colors.Red;
                    interrupteur.OnColor = Colors.Red;
                    DisplayAlert("Action", "Fermer", "OK");
                }
            }
        }
    }
}
