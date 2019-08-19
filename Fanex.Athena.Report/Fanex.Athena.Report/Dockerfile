FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Fanex.Athena.Report/Fanex.Athena.Report.csproj", "Fanex.Athena.Report/"]
RUN dotnet restore "Fanex.Athena.Report/Fanex.Athena.Report.csproj"
COPY . .
WORKDIR "/src/Fanex.Athena.Report"
RUN dotnet build "Fanex.Athena.Report.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Fanex.Athena.Report.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Fanex.Athena.Report.dll"]