# This example uses .NET 6.0.  For other versions, see https://hub.docker.com/_/microsoft-dotnet-sdk/
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base

# Build your application
WORKDIR /src
COPY . hello-otel-autoinstrument
RUN dotnet publish -c Release -o /app ./hello-otel-autoinstrument

# The runtime tag version should match the SDK tag version
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

WORKDIR /app

# Install the agent
#COPY ../opentelemetry-dotnet-instrumentation-macos-0.2.0-beta.1 /app/opentelemetry-dotnet-instrumentation-macos-0.2.0-beta.1
RUN apt update && \
    apt install -y unzip curl && \
    curl "https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v0.2.0-beta.1/opentelemetry-dotnet-instrumentation-linux-glibc.zip" -o "opentelemetry-dotnet-instrumentation.zip" -L && \
    unzip opentelemetry-dotnet-instrumentation.zip -d opentelemetry-dotnet-instrumentation && \
    apt remove -y unzip curl

# Enable the agent
ENV InstallationLocation=/app/opentelemetry-dotnet-instrumentation
ENV COR_ENABLE_PROFILING=1 \
COR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318} \
CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318} \
DOTNET_ADDITIONAL_DEPS=$InstallationLocation/AdditionalDeps \
DOTNET_SHARED_STORE=$InstallationLocation/store \
DOTNET_STARTUP_HOOKS=$InstallationLocation/netcoreapp3.1/OpenTelemetry.AutoInstrumentation.StartupHook.dll \
OTEL_DOTNET_AUTO_HOME=$InstallationLocation \
OTEL_DOTNET_AUTO_INTEGRATIONS_FILE=$InstallationLocation/integrations.json \
CORECLR_PROFILER_PATH=$InstallationLocation/OpenTelemetry.AutoInstrumentation.Native.so \
OTEL_SERVICE_NAME=opentelemetrylabs-auto-instrumentation-web-service \
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=staging,service.version=1.0.0 \
OTEL_TRACES_EXPORTER=otlp \
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318 \
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf \
OTEL_DOTNET_AUTO_DEBUG=true \
OTEL_DOTNET_AUTO_ENABLED=true \ 
OTEL_DOTNET_AUTO_TRACES_ENABLED_INSTRUMENTATIONS="AspNet,HttpClient" \
OTEL_DOTNET_AUTO_ENABLED_INSTRUMENTATIONS="AspNet,HttpClient,NetRuntime"  \
OTEL_DOTNET_AUTO_LOG_DIRECTORY=$InstallationLocation/logs \
OTEL_DOTNET_AUTO_TRACES_CONSOLE_EXPORTER_ENABLED=true \
OTEL_DOTNET_AUTO_METRICS_CONSOLE_EXPORTER_ENABLED=true 

COPY --from=base /app .

ENTRYPOINT ["dotnet", "./hello-otel-autoinstrument.dll"]