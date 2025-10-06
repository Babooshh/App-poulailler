# Intégration MQTT (App-poulailler)

Cette application utilise la bibliothèque [MQTTnet](https://github.com/dotnet/MQTTnet) pour communiquer avec un broker MQTT.

## Topics utilisés

- `poulailler/porte/cmd` : publication d'une commande `open` ou `close` lorsque l'interrupteur est actionné.
- `poulailler/porte/schedule` : publication d'un JSON de la forme:
  ```json
  {"open":"HH:MM","close":"HH:MM"}
  ```

## Configuration broker

Par défaut (hardcodé dans `MauiProgram.cs`):
```
Host: test.mosquitto.org
Port: 1883
Auth: aucune
```
Changez ces valeurs dans `MauiProgram.cs` pour votre broker privé (ex: Mosquitto local ou hébergé) :
```csharp
const string mqttHost = "192.168.1.50"; // exemple IP locale
const int mqttPort = 1883;
// Si besoin d'identifiants: adapter le constructeur MqttService
```

## Sécurisation / Améliorations possibles
- Support TLS (port 8883) via options MQTTnet supplémentaires.
- Authentification (username/password) ou certificats.
- Gestion reconnection avancée et file d'attente offline.
- Abonnements pour retour d'état : ajouter `SubscribeAsync("poulailler/porte/state")` et gérer `MessageReceived`.

## Extension UI (idées)
- Afficher l'état actuel de la porte (ouverte/fermée) selon un topic state.
- Historique des commandes envoyées.
- Indicateur de connexion MQTT (couleur / icône).

---
Pour toute modification plus avancée, étendre `IMqttService`.
