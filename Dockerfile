FROM microsoft/dotnet:3.0-sdk
COPY ["src/ModSink.CLI/ModSink.CLI.csproj", "src/ModSink.CLI/"]
RUN dotnet restore "src/ModSink.CLI/ModSink.CLI.csproj"
COPY . .
WORKDIR "src/ModSink.CLI/"
RUN dotnet build "ModSink.CLI.csproj" -c Release
ENTRYPOINT ["dotnet", "run", "-c", "Release", "--"]
CMD ["--help"]
