using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MauiApp1;

public static class MauiProgram
{
    internal static string serviceName = "Tanaka733.OpenTelemetryLabs.MauiApp";
    internal static string serviceVersion = "1.0.0";
    internal static TracerProvider TracerProvider;
    internal static OpenTelemetryLoggerProvider LoggerProvider;
    internal static ILogger logger;
	public static MauiApp CreateMauiApp()
	{
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var MyActivitySource = new ActivitySource(MauiProgram.serviceName);
            using var activity = MyActivitySource.StartActivity("UnhandledException");
            Activity.Current?.RecordException(args.ExceptionObject as Exception);
            logger.LogError(args.ExceptionObject as Exception, "");
        };

        AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
        {
            var MyActivitySource = new ActivitySource(MauiProgram.serviceName);
            using var activity = Activity.Current ?? MyActivitySource.StartActivity("FirstChanceException");
            activity?.SetStatus(ActivityStatusCode.Error, "error happened.");
            activity?.RecordException(args.Exception);
            logger.LogError(args.Exception, "FirstChanceException");
        };


        //#if IOS
        //        ObjCRuntime.Runtime.MarshalManagedException += (sender, args) =>
        //        {
        //            Console.WriteLine("In MarshalManagedException Handler");

        //            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        //        };

        //        ObjCRuntime.Runtime.MarshalObjectiveCException += (sender, args) =>
        //        {
        //            Console.WriteLine("In MarshalObjectiveCException Handler");
        //        };
        //#endif
        // Configure important OpenTelemetry settings and the console exporter
        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options =>
            {
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();
        LoggerProvider = Sdk.CreateLoggerProviderBuilder()
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .SetIncludeFormattedMessage(true)
            .SetIncludeScopes(true)
            .SetParseStateValues(true)
            .AddOtlpExporter(options =>
            {
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();
        logger = LoggerProvider.CreateLogger(nameof(MauiProgram));

        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
        builder.Services.AddTransient<MainPage>();
        
		return builder.Build();
	}

}
