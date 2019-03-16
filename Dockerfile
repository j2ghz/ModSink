FROM microsoft/dotnet:3.0-sdk AS build
COPY ["src/ModSink.CLI/ModSink.CLI.csproj", "src/ModSink.CLI/"]
RUN dotnet restore "src/ModSink.CLI/ModSink.CLI.csproj"
COPY . .
WORKDIR "src/ModSink.CLI/"
RUN dotnet publish "ModSink.CLI.csproj" -c Release -o /app

FROM microsoft/dotnet:3.0-runtime
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ModSink.CLI.dll"]
CMD ["--help"]
