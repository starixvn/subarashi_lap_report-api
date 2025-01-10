FROM hub.nexdev.net/base/dotnet/core/sdk:5.0.102-ca-patch-buster-slim AS builder

WORKDIR /src

COPY NuGet.Config .
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2

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

COPY src/Modules/Fanex.Athena.Report.BetList/Fanex.Athena.Report.BetList.csproj ./Modules/Fanex.Athena.Report.BetList/
RUN --mount=type=cache,target=/root/.nuget/packages \
 dotnet restore Modules/Fanex.Athena.Report.BetList/Fanex.Athena.Report.BetList.csproj -v n

COPY src/Fanex.Athena.Report/Fanex.Athena.Report.csproj ./Fanex.Athena.Report/
RUN --mount=type=cache,target=/root/.nuget/packages \
 dotnet restore Fanex.Athena.Report/Fanex.Athena.Report.csproj -v n

COPY src/Fanex.Athena.Report.Resources ./Fanex.Athena.Report.Resources
COPY src/Fanex.Athena.Report.Common ./Fanex.Athena.Report.Common
COPY src/Fanex.Athena.Report.Core ./Fanex.Athena.Report.Core
COPY src/Modules/Fanex.Athena.Report.ByPlayer ./Modules/Fanex.Athena.Report.ByPlayer
COPY src/Modules/Fanex.Athena.Report.BySite ./Modules/Fanex.Athena.Report.BySite
COPY src/Modules/Fanex.Athena.Report.Base ./Modules/Fanex.Athena.Report.Base
COPY src/Modules/Fanex.Athena.Report.BetList ./Modules/Fanex.Athena.Report.BetList
COPY src/Fanex.Athena.Report ./Fanex.Athena.Report


RUN dotnet publish "Fanex.Athena.Report/Fanex.Athena.Report.csproj" -c Release -o /publish \
  --runtime alpine-x64 \
  --self-contained false

FROM hub.nexdev.net/base/dotnet/core/aspnet:5.0-alpine

ENV ASPNETCORE_URLS=http://*:5000
EXPOSE 5000

WORKDIR /app

COPY --from=builder /publish .

ENTRYPOINT ["dotnet", "Fanex.Athena.Report.dll"]