using Microsoft.Extensions.Logging;

namespace App_poulailler
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // TODO: externaliser dans config / secure storage
            const string mqttHost = "172.31.254.129"; // broker raspi pour tests
            const int mqttPort = 1883;
            builder.Services.AddSingleton<Services.IMqttService>(_ => new Services.MqttService(mqttHost, mqttPort));

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
