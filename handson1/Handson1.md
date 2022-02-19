OpenTelemetry .NET ハンズオン#1

### 事前準備

- [.NET 6 SDKのインストール](https://dotnet.microsoft.com/en-us/download)
- [opentelemetry-dotnet-lab リポジトリ](https://github.com/tanaka-takayoshi/opentelemetry-dotnet-lab)のクローンもしくはダウンロード
- New Relicアカウントの作成（イベント参加者で希望される方には共有アカウントを提供するため不要です） *

>  * Exporterをご自身で設定する場合、New Relicアカウントの作成・利用は不要です。

このフォルダにある handson1.slnを開く（Visual Studioの場合）、もしくはこのフォルダをワーキングディレクトリとして開き（Visual Studio Codeの場合）、プロジェクトが実行できることを確認してください。

実行した後、以下のURL（ホストとポートはデフォルトの設定なので、ご自身の実行設定によっては異なります）にアクセスし、期待通りのレスポンスがブラウザに表示されることを確認してください。

1. https://localhost:5001/weatherforecast

以下のようなJSONが表示されます。（値はアクセスするたびにかわります）

```
[{"date":"2022-02-14T22:22:15.6460201+09:00","temperatureC":42,"summary":"Warm","temperatureF":107},{"date":"2022-02-15T22:22:15.6465033+09:00","temperatureC":35,"summary":"Bracing","temperatureF":94},{"date":"2022-02-16T22:22:15.6465067+09:00","temperatureC":17,"summary":"Chilly","temperatureF":62},{"date":"2022-02-17T22:22:15.6465068+09:00","temperatureC":35,"summary":"Scorching","temperatureF":94},{"date":"2022-02-18T22:22:15.6465069+09:00","temperatureC":8,"summary":"Chilly","temperatureF":46}]
```

2. https://localhost:5001/external

以下のようなJSONが表示されます。（値はアクセスするたびに変わります）

```
{
    "args": {}, 
    "headers": {
      "Host": "httpbin.org", 
      "Traceparent": "00-2732b8b723e722ed87f4ef579b6c994c-ea8318c97d46b90d-00", 
      "X-Amzn-Trace-Id": "Root=1-6209062a-1a743e9b48a5f21851ee8000"
    }, 
    "origin": "113.150.182.209", 
    "url": "http://httpbin.org/get"
}
```

3. https://localhost:5001/external/error

`502` と表示されます。

4. https://localhost:5001/db

以下のJSONが表示されます。（値は同じ）

```
[{"id":1,"age":57,"name":"Shinji"},{"id":2,"age":25,"name":"Taro"},{"id":3,"age":17,"name":"Jiro"}]
```

5. https://localhost:5001/error

ハンドルされない例外がスローされ、ASP.NET Coreのエラー画面が表示されます。（エラー画面の内容は起動の仕方により変わります）

### 課題1 ASP.NET Coreの自動計装

3つのNuGetライブラリを追加します。handson1.csproj を編集して3行追加するか、`dotnet add package` コマンドで追加してください。

```
	<ItemGroup>
		<PackageReference Include="Microsoft.Data.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
        <!-- 以下3行を追加 -->
		<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.2.0-rc2" />
		<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc9" />
	</ItemGroup>
```

Program.csを開いて以下の通り編集します。apiKeyには事前に共有したAPIキーを、appNameは自分でわかるようにHello-Otel-01など別の名前を適当につけてください。

```
using Microsoft.EntityFrameworkCore;
//using追加
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//以下追加
var tags = new Dictionary<string, object>
    {
        { "Environment", "Production" },
        { "Level", 99 },
    };
var appName = "Hello-Otel";
var endpoint = "https://otlp.nr-data.net:4317/";
var apiKey = "REPLACE_YOUR_APIKEY";

builder.Services.AddOpenTelemetryTracing((builder) => builder
        .AddAspNetCoreInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName).AddAttributes(tags))
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(endpoint);
            options.Headers = $"api-key={apiKey}";
        })
    );
```

プロジェクトを起動し、エンドポイントに適当にアクセスしてください。
[New Relic OneのExplorer](https://onenr.io/0M8jq9GOOjl)にアクセスして、appNameに指定したアプリが表示されていればOKです。
アプリ全体のスループット、経過時間などのメトリクスが表示され、アクセスしたURLがDistributed Tracingとして記録されていることを確認してください。

### トランザクションに情報を追加する

先ほど計測したトランザクションにタグで情報を追加します。
今回は固定値を追加していますが、実際では、クエリパラメーターを付与することクエリパラメーターによるパフォーマンスの違いなどを分析するのに役立てます。
また、 .NET では `System.Disgnositcs.Activity` はOpenTelemetryでいうトレースに相当しており、ActivityのAPIを操作することで OpenTelemetryのトレースを操作できるようになっています。

```
//using追加
using System.Diagnostics;
//中略

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    var activity = Activity.Current;
    activity?.SetTag("operation.value", 1);
    activity?.SetTag("operation.name", "Saying hello!");
    using (var childActivity = activity?.Source.StartActivity("Child Span"))
    {
        Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
    }
```

### 外部HTTP呼び出しを記録する

先ほどと同様、NuGetパッケージOpenTelemetry.Instrumentation.Httpを追加します。

```
	<ItemGroup>
		<PackageReference Include="Microsoft.Data.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
		<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.2.0-rc2" />
		<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc9" />
        //以下1行追加
		<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.0.0-rc9" />
	</ItemGroup>
```

Program.csで以下の1行を追加します。

```
builder.Services.AddOpenTelemetryTracing((builder) => builder
        .AddAspNetCoreInstrumentation()
        //1行追加
        .AddHttpClientInstrumentation()
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName).AddAttributes(tags))
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(endpoint);
            options.Headers = $"api-key={apiKey}";
        })
    );
```

https://localhost:5001/external/ や https://localhost:5001/external/error にアクセスします。

New Relicの画面で新規に記録されたトレースを見ると、HTTP呼び出しのスパンが追加され、http.** という属性が見えるはずです。

### DB呼び出し（SQL処理）を記録する

NuGetパッケージ OpenTelemetry.Contrib.Instrumentation.EntityFrameworkCore を追加します。

```
	<ItemGroup>
		<PackageReference Include="Microsoft.Data.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
        //以下1行追加
		<PackageReference Include="OpenTelemetry.Contrib.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta2" />
		<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.2.0-rc2" />
		<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.0.0-rc9" />
	</ItemGroup>
```

Program.cs で以下のメソッド呼び出しを追加します。

```
builder.Services.AddOpenTelemetryTracing((builder) => builder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        //以下のメソッド呼び出しを追加。
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForStoredProcedure = true;
            options.SetDbStatementForText = true;
        })
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName).AddAttributes(tags))
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(endpoint);
            options.Headers = $"api-key={apiKey}";
        })
    );
```

再起動後、 https://localhost:5001/db にアクセスします。

New Relicの画面で新規に記録されたトランザクションにDB呼び出しのスパンが記録されているはずです。

### Logを送信する

NuGetパッケージ OpenTelemetry.Exporter.OpenTelemetryProtocol.Logs を追加します。

```
	<ItemGroup>
		<PackageReference Include="Microsoft.Data.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="6.0.2" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="6.0.2">
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
			<PrivateAssets>all</PrivateAssets>
		</PackageReference>
		<PackageReference Include="OpenTelemetry.Contrib.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta2" />
		<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.2.0-rc2" />
        //以下1行追加
		<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol.Logs" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.0.0-rc9" />
		<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.0.0-rc9" />
	</ItemGroup>
```

Program.csに以下のusing1行と、`builder.Logging.AddOpenTelemetry`の呼び出しをAddOpenTelemetryTracing 呼び出しの後に追加します。

```
using OpenTelemetry.Logs;

builder.Logging.AddOpenTelemetry(builder =>
{
    builder.IncludeFormattedMessage = true;
    builder.IncludeScopes = true;
    builder.ParseStateValues = true;
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Hello-Otel").AddAttributes(tags))
    .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(endpoint);
            options.Headers = $"api-key={apiKey}";
            //不具合があるため、Windows環境では必須
            options.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
        });
});
```

再起動後、https://localhost:5001/weatherforecast にアクセスします。

New Relicの画面でLogsメニューを表示するとログが表示されるはずです。
