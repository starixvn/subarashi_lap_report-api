FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder

WORKDIR /src

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

COPY src/Fanex.Athena.Report.Resources/Fanex.Athena.Report.Resources.csproj ./Fanex.Athena.Report.Resources/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Fanex.Athena.Report.Resources/Fanex.Athena.Report.Resources.csproj -v n

COPY src/Fanex.Athena.Report.Common/Fanex.Athena.Report.Common.csproj ./Fanex.Athena.Report.Common/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Fanex.Athena.Report.Common/Fanex.Athena.Report.Common.csproj -v n

COPY src/Fanex.Athena.Report.Core/Fanex.Athena.Report.Core.csproj ./Fanex.Athena.Report.Core/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Fanex.Athena.Report.Core/Fanex.Athena.Report.Core.csproj -v n

COPY src/Modules/Fanex.Athena.Report.ByPlayer/Fanex.Athena.Report.ByPlayer.csproj ./Modules/Fanex.Athena.Report.ByPlayer/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Modules/Fanex.Athena.Report.ByPlayer/Fanex.Athena.Report.ByPlayer.csproj -v n

COPY src/Modules/Fanex.Athena.Report.BySite/Fanex.Athena.Report.BySite.csproj ./Modules/Fanex.Athena.Report.BySite/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Modules/Fanex.Athena.Report.BySite/Fanex.Athena.Report.BySite.csproj -v n

COPY src/Modules/Fanex.Athena.Report.Base/Fanex.Athena.Report.Base.csproj ./Modules/Fanex.Athena.Report.Base/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Modules/Fanex.Athena.Report.Base/Fanex.Athena.Report.Base.csproj -v n

COPY src/Fanex.Athena.Report/Fanex.Athena.Report.csproj ./Fanex.Athena.Report/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore Fanex.Athena.Report/Fanex.Athena.Report.csproj -v n

COPY src/Fanex.Athena.Report.Resources ./Fanex.Athena.Report.Resources
COPY src/Fanex.Athena.Report.Common ./Fanex.Athena.Report.Common
COPY src/Fanex.Athena.Report.Core ./Fanex.Athena.Report.Core
COPY src/Modules/Fanex.Athena.Report.ByPlayer ./Modules/Fanex.Athena.Report.ByPlayer
COPY src/Modules/Fanex.Athena.Report.BySite ./Modules/Fanex.Athena.Report.BySite
COPY src/Modules/Fanex.Athena.Report.Base ./Modules/Fanex.Athena.Report.Base
COPY src/Fanex.Athena.Report ./Fanex.Athena.Report

RUN dotnet publish "Fanex.Athena.Report/Fanex.Athena.Report.csproj" \
    -c Release -o /publish --runtime linux-x64 --self-contained false

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine-25h2 AS final

ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000

# Dont run as root
RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp && \
    mkdir -p /LapReportTempExcel && chown -R dotnetuser /LapReportTempExcel
USER dotnetuser
#

WORKDIR /app

COPY --from=builder /publish .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Fanex.Athena.Report.dll"]