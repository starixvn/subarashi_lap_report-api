FROM registry.nexdev.net:5050/docker/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY NuGet.Config .

COPY Fanex.Athena.Report/Fanex.Athena.Report.Resources/Fanex.Athena.Report.Resources.csproj ./Fanex.Athena.Report.Resources/
RUN dotnet restore Fanex.Athena.Report.Resources/Fanex.Athena.Report.Resources.csproj -v n

COPY Fanex.Athena.Report/Fanex.Athena.Report.Common/Fanex.Athena.Report.Common.csproj ./Fanex.Athena.Report.Common/
RUN dotnet restore Fanex.Athena.Report.Common/Fanex.Athena.Report.Common.csproj -v n

COPY Fanex.Athena.Report/Fanex.Athena.Report.Core/Fanex.Athena.Report.Core.csproj ./Fanex.Athena.Report.Core/
RUN dotnet restore Fanex.Athena.Report.Core/Fanex.Athena.Report.Core.csproj -v n

COPY Fanex.Athena.Report/Modules/Fanex.Athena.Report.ByPlayer/Fanex.Athena.Report.ByPlayer.csproj ./Modules/Fanex.Athena.Report.ByPlayer/
RUN dotnet restore Modules/Fanex.Athena.Report.ByPlayer/Fanex.Athena.Report.ByPlayer.csproj -v n

COPY Fanex.Athena.Report/Modules/Fanex.Athena.Report.BySite/Fanex.Athena.Report.BySite.csproj ./Modules/Fanex.Athena.Report.BySite/
RUN dotnet restore Modules/Fanex.Athena.Report.BySite/Fanex.Athena.Report.BySite.csproj -v n

COPY Fanex.Athena.Report/Fanex.Athena.Report/Fanex.Athena.Report.csproj ./Fanex.Athena.Report/
RUN dotnet restore Fanex.Athena.Report/Fanex.Athena.Report.csproj -v n

COPY Fanex.Athena.Report/Fanex.Athena.Report.Resources ./Fanex.Athena.Report.Resources
COPY Fanex.Athena.Report/Fanex.Athena.Report.Common ./Fanex.Athena.Report.Common
COPY Fanex.Athena.Report/Fanex.Athena.Report.Core ./Fanex.Athena.Report.Core
COPY Fanex.Athena.Report/Modules/Fanex.Athena.Report.ByPlayer ./Modules/Fanex.Athena.Report.ByPlayer
COPY Fanex.Athena.Report/Modules/Fanex.Athena.Report.BySite ./Modules/Fanex.Athena.Report.BySite
COPY Fanex.Athena.Report/Fanex.Athena.Report ./Fanex.Athena.Report

RUN dotnet publish "Fanex.Athena.Report/Fanex.Athena.Report.csproj" -c Release -o /publish \
  --runtime alpine-x64 \
  --self-contained false

FROM registry.nexdev.net:5050/docker/dotnet/core/aspnet:5.0-buster-slim

ENV ASPNETCORE_URLS=http://*:5000
EXPOSE 5000

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf

WORKDIR /app

COPY --from=builder /publish .

ENTRYPOINT ["dotnet", "Fanex.Athena.Report.dll"]