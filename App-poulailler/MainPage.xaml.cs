using App_poulailler.Services;

namespace App_poulailler
{
    public partial class MainPage : ContentPage
    {
        private readonly IMqttService _mqtt;
        private bool _mqttInitialized;

        public MainPage(IMqttService mqttService)
        {
            InitializeComponent();
            _mqtt = mqttService;
            _ = EnsureMqttAsync();
        }

        private async Task EnsureMqttAsync()
        {
            if (_mqttInitialized) return;
            try
            {
                await _mqtt.ConnectAsync();
                _mqttInitialized = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("MQTT", $"Connexion impossible: {ex.Message}", "OK");
            }
        }

        private async void OnInterrupteurToggled(object? sender, ToggledEventArgs e)
        {
            if (sender is Switch interrupteur)
            {
                if (e.Value)
                {
                    interrupteur.ThumbColor = Colors.Green;
                    interrupteur.OnColor = Colors.Green;
                    await PublishMqttSafe("poulailler/porte/cmd", "open");
                }
                else
                {
                    interrupteur.ThumbColor = Colors.Red;
                    interrupteur.OnColor = Colors.Red;
                    await PublishMqttSafe("poulailler/porte/cmd", "close");
                }
            }
        }

        private async void OnSaveScheduleClicked(object? sender, EventArgs e)
        {
            var ouverture = TimePickerOuverture?.Time;
            var fermeture = TimePickerFermeture?.Time;
            if (ouverture != null && fermeture != null)
            {
                string message = $"Ouverture : {ouverture.Value:hh\\:mm}\nFermeture : {fermeture.Value:hh\\:mm}";
                await PublishMqttSafe("poulailler/porte/schedule", $"{{\"open\":\"{ouverture.Value:hh\\:mm}\",\"close\":\"{fermeture.Value:hh\\:mm}\"}}");
                await DisplayAlert("Programmation enregistrée", message, "OK");
            }
            else
            {
                DisplayAlert("Erreur", "Veuillez sélectionner les deux horaires.", "OK");
            }
        }

        private async Task PublishMqttSafe(string topic, string payload)
        {
            try
            {
                await EnsureMqttAsync();
                await _mqtt.PublishAsync(topic, payload);
            }
            catch (Exception ex)
            {
                await DisplayAlert("MQTT", $"Erreur publication: {ex.Message}", "OK");
            }
        }
    }
}
