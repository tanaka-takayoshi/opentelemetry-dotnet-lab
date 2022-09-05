


### How to run the otel exporter on Windows?

Create `.env` file which contains the following lines.

```
SET OTEL_EXPORTER_OTLP_ENDPOINT=https://otlp.nr-data.net:4317
SET NEW_RELIC_LICENSE_KEY=<REPLACE_YOUR_LICENSEKEY>
```

Run the following command in case of running docker on Windows.

```
docker run --env-file .env -p 4318:4318 -p 4317:4317 -v %cd%/otel-collector-config.yaml:/etc/otel-collector-config.yaml otel/opentelemetry-collector:latest  --config=/etc/otel-collector-config.yaml
``` 
