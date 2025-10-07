using App_poulailler.Services;
using Microsoft.Maui.ApplicationModel;
using System.Text.Json;

namespace App_poulailler
{
    public partial class MainPage : ContentPage
    {
        private readonly IMqttService _mqtt;
        private bool _mqttInitialized;
        private readonly Dictionary<string, Label> _chickenLabels = new();
        private readonly Dictionary<string, string> _chickenLocation = new(); // id -> "interieur" | "exterieur"
        private bool _suppressToggleHandler = false;

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
                _mqtt.MessageReceived += OnMqttMessageReceived;
                await _mqtt.SubscribeAsync("poulailler/poules");
            }
            catch (Exception ex)
            {
                await DisplayAlert("MQTT", $"Connexion impossible: {ex.Message}", "OK");
            }
        }

        private void OnMqttMessageReceived(object? sender, string payload)
        {
            // Expected payload example:
            // {"id":"029EC135","nom":"Poule2","etat":"dedans","horodatage":"2025-10-07 11:06:58"}
            try
            {
                var update = JsonSerializer.Deserialize<PouleUpdate>(payload);
                if (update == null || string.IsNullOrWhiteSpace(update.id)) return;

                var location = NormalizeEtat(update.etat);
                if (location == null) return; // unknown state

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!_chickenLabels.TryGetValue(update.id, out var label))
                    {
                        label = new Label { Text = FormatChickenText(update), FontSize = 16 };
                        _chickenLabels[update.id] = label;
                    }
                    else
                    {
                        label.Text = FormatChickenText(update);
                        // Remove from previous container if any
                        if (_chickenLocation.TryGetValue(update.id, out var prev))
                        {
                            if (prev == "interieur") InterieurList.Children.Remove(label);
                            else if (prev == "exterieur") ExterieurList.Children.Remove(label);
                        }
                    }

                    // Add to new container
                    if (location == "interieur")
                        InterieurList.Children.Add(label);
                    else
                        ExterieurList.Children.Add(label);

                    _chickenLocation[update.id] = location;
                });
            }
            catch
            {
                // ignore malformed payloads
            }
        }

        private static string? NormalizeEtat(string? etat)
        {
            if (string.IsNullOrWhiteSpace(etat)) return null;
            etat = etat.Trim().ToLowerInvariant();
            return etat switch
            {
                "dedans" or "interieur" or "int" => "interieur",
                "dehors" or "exterieur" or "ext" => "exterieur",
                _ => null
            };
        }

        private static string FormatChickenText(PouleUpdate update)
        {
            var name = string.IsNullOrWhiteSpace(update.nom) ? update.id : update.nom;
            return $"{name} ({update.id})";
        }

        private class PouleUpdate
        {
            public string? id { get; set; }
            public string? nom { get; set; }
            public string? etat { get; set; }
            public string? horodatage { get; set; }
        }

        private async void OnInterrupteurToggled(object? sender, ToggledEventArgs e)
        {
            if (_suppressToggleHandler) return;
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
                    // Empêcher la fermeture si toutes les poules ne sont pas à l'intérieur
                    if (!AllChickensInside())
                    {
                        _suppressToggleHandler = true;
                        interrupteur.IsToggled = true; // revert
                        _suppressToggleHandler = false;
                        await DisplayAlert("Sécurité", "Impossible de fermer: toutes les poules ne sont pas à l'intérieur.", "OK");
                        return;
                    }
                    interrupteur.ThumbColor = Colors.Red;
                    interrupteur.OnColor = Colors.Red;
                    await PublishMqttSafe("poulailler/porte/cmd", "close");
                }
            }
        }

        private bool AllChickensInside()
        {
            // Si aucune info, on considère que la fermeture n'est pas autorisée
            if (_chickenLocation.Count == 0) return false;
            foreach (var kv in _chickenLocation)
            {
                if (!string.Equals(kv.Value, "interieur", StringComparison.Ordinal))
                    return false;
            }
            return true;
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
