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

        private void OnSaveScheduleClicked(object? sender, EventArgs e)
        {
            var ouverture = TimePickerOuverture?.Time;
            var fermeture = TimePickerFermeture?.Time;
            if (ouverture != null && fermeture != null)
            {
                string message = $"Ouverture : {ouverture.Value.ToString(@"hh\:mm")}\nFermeture : {fermeture.Value.ToString(@"hh\:mm")}";
                DisplayAlert("Programmation enregistrée", message, "OK");
            }
            else
            {
                DisplayAlert("Erreur", "Veuillez sélectionner les deux horaires.", "OK");
            }
        }
    }
}
