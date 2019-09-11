dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=..\..\TestResults\Coverage\
dotnet ".\reportgenerator\4.2.19\tools\netcoreapp2.1\ReportGenerator.dll" "-reports:.\TestResults\Coverage\coverage.opencover.xml" "-targetdir:..\TestReports"
