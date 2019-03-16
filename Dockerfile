FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
COPY ["src/ModSink.CLI/ModSink.CLI.csproj", "src/ModSink.CLI/"]
RUN dotnet restore "src/ModSink.CLI/ModSink.CLI.csproj"
COPY . .
WORKDIR "src/ModSink.CLI/"
RUN dotnet publish "ModSink.CLI.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ModSink.CLI.dll"]
CMD ["--help"]
