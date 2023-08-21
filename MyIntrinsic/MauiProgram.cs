using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using Microsoft.Extensions.DependencyInjection;
using MyIntrinsic.Services.Networking;


namespace MyIntrinsic;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
                fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsBold");
                fonts.AddFont("fa-solid-900.ttf", "FAS");
                fonts.AddFont("fa-regular-400.ttf", "FAR");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        AppCenter.Start("windowsdesktop=c6e691bf-530a-4761-913e-e307094f3fa6;" +
                "android=57f01b90-6be7-4234-b6bd-87a3f8b49f02;" +
                "ios=a528564b-5fe0-4444-acb7-bc98d7165697;",
                typeof(Analytics), typeof(Crashes));

#if DEBUG
        builder.Logging.AddDebug();
#endif
        //Create and inflate dependencies
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<SignUpPage>();
        builder.Services.AddTransient<SignUpViewModel>();
        builder.Services.AddTransient<ProvisioningService>();
        builder.Services.AddSingleton<ILoginService, LoginService>();
        builder.Services.AddSingleton<CustomerDataService>();
        builder.Services.AddSingleton<WiFiService>();
        builder.Services.AddSingleton<IChargerRepository, ChargerRepository>();
        //builder.Services.AddSingleton<ICustomerDataRepository, CustomerDataRepositoryMock>();
        //builder.Services.AddSingleton<IDataStore<Charger>, ChargerService>();
        builder.Services.AddSingleton<DataMngr>();
        builder.Services.AddSingleton<SelectChargerView>();
        builder.Services.AddSingleton<SelectChargerViewModel>();
        builder.Services.AddSingleton<AddChargerView>();
        builder.Services.AddSingleton<AddChargerViewModel>();
        builder.Services.AddSingleton<ConfigureChargerView>();
        builder.Services.AddSingleton<ConfigureChargerViewModel>();
        builder.Services.AddSingleton<AboutView>();
        builder.Services.AddSingleton<AboutViewModel>();
        builder.Services.AddSingleton<IConnectivity>(Microsoft.Maui.Networking.Connectivity.Current);
        builder.Services.AddTransient<AuthenticationDelegatingHandler>();
        //builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        //builder.Services.AddSingleton<IMap>(Map.Default);

        //Need to create a new charger each time the details page is launched.
        //builder.Services.AddTransient<ChargerDetailsViewModel>();
        //builder.Services.AddTransient<ChargerDetailsPage>();

        builder.Services.AddHttpClient("AwsClient", client =>
        {
            client.BaseAddress = new Uri("https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com");
            //client.Timeout = new TimeSpan(0, 0, 30);
            client.DefaultRequestHeaders.Clear();
        }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

        //.ConfigurePrimaryHttpMessageHandler(handler =>
        //    new HttpClientHandler()
        //    {
        //        AutomaticDecompression = System.Net.DecompressionMethods.GZip
        //    });

        builder.Services.AddHttpClient<ICustomerDataRepository, CustomerDataRepository>()
                .ConfigurePrimaryHttpMessageHandler(handler =>
           new HttpClientHandler()
           {
               //AutomaticDecompression = System.Net.DecompressionMethods.GZip
           }).AddHttpMessageHandler<AuthenticationDelegatingHandler>(); 

        return builder.Build();
    }
}
