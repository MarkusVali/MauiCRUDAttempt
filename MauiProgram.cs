using MauiCRUDAttempt.Data;
using MauiCRUDAttempt.ViewModels;
using Microsoft.Extensions.Logging;

namespace MauiCRUDAttempt
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

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<DatabaseContext>();
            builder.Services.AddSingleton<WishlistViewModel>();
            builder.Services.AddSingleton<CRUDPage>();

            return builder.Build();
        }
    }
}
