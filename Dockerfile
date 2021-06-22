FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY . /p
WORKDIR "/p/"
RUN dotnet publish "src/ModSink.CLI/ModSink.CLI.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:5.0
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ModSink.CLI.dll"]
CMD ["--help"]
