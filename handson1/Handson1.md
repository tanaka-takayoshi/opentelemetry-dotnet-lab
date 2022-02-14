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

