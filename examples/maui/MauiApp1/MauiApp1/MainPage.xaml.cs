using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
	int count = 0;
    ILogger logger;
    private HttpClient client;

    public MainPage()
	{
		InitializeComponent();
        logger = MauiProgram.LoggerProvider.CreateLogger(nameof(MainPage));
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        var MyActivitySource = new ActivitySource(MauiProgram.serviceName);
        using var activity = MyActivitySource.StartActivity("CounterClick", ActivityKind.Consumer);
        activity?.SetTag("foo", 1);
        activity?.SetTag("bar", "Hello, World!");
        activity?.SetTag("baz", new int[] { 1, 2, 3 });
        logger.LogInformation("OnCounterClicked {count}", count);
        count++;


        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        client = new HttpClient();
        var res = await client.GetAsync("https://localhost:7181/");
        res.EnsureSuccessStatusCode();

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void OnUnhandledExceptionClicked(object sender, EventArgs e)
    {
        var MyActivitySource = new ActivitySource(MauiProgram.serviceName);
        using var activity = MyActivitySource.StartActivity("OnUnhandledExceptionClicked", ActivityKind.Consumer);
        new ApplicationException("intended exception");
    }

    private void OnBackgroundThreadUnhandledExceptionClicked(object sender, EventArgs e)
    {
        var thread = new Thread(() => throw new ApplicationException(""));
        thread.Start();
    }

    private void OnCapturedExceptionClicked(object sender, EventArgs e)
    {
        var MyActivitySource = new ActivitySource(MauiProgram.serviceName);
        using var activity = MyActivitySource.StartActivity("OnCapturedExceptionClicked", ActivityKind.Consumer);
        try
        {
            throw new ApplicationException("This exception was thrown and captured manually, without crashing the app.");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(Status.Error);
            activity?.RecordException(ex);
        }
    }

}

