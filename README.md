# JobPortalBackend

```powershell
cd C:\Users\asus\Documents\JobPortal

dotnet new tool-manifest --force
dotnet tool update dotnet-sonarscanner
if ($LASTEXITCODE -ne 0) { dotnet tool install dotnet-sonarscanner }

if (-not $env:SONAR_TOKEN) {
    $env:SONAR_TOKEN = Read-Host "Enter SonarQube token"
}

dotnet tool run dotnet-sonarscanner begin `
    /k:"JobPortal_FullStack" `
    /v:"1.0" `
    /d:sonar.host.url="http://localhost:9000" `
    /d:sonar.token="$env:SONAR_TOKEN" `
    /d:sonar.projectBaseDir="C:\Users\asus\Documents\JobPortal" `
    /d:sonar.scm.disabled="true" `
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
    /d:sonar.coverage.exclusions="**/Program.cs,**/Migrations/**,**/DTOs/**,**/Models/**,**/Consumers/**,**/wwwroot/**,JobPortalFrontend/**" `
    /d:sonar.exclusions="**/bin/**,**/obj/**,**/node_modules/**,**/.angular/**,**/.sonarqube/**,**/coverage/**,**/dist/**,**/*@tmp/**,**/Migrations/**"

dotnet build JobPortal.sln --configuration Release
dotnet test JobPortal.sln --configuration Release --no-build --collect:"XPlat Code Coverage" --results-directory ".\TestResults" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
dotnet tool run dotnet-sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
```
