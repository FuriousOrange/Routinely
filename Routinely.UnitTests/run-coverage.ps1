# Generate Code Coverage Report for Routinely
# Runs tests with coverage and generates an HTML report with class-level detail

Write-Host "Running tests with code coverage..." -ForegroundColor Cyan

# Clean previous results
if (Test-Path ./TestResults) {
    Remove-Item ./TestResults -Recurse -Force
}

# Run tests with coverage
dotnet test `
    --configuration Release `
    --settings coverlet.runsettings `
    --collect:"XPlat Code Coverage" `
    --results-directory:./TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Tests passed!" -ForegroundColor Green

# Find coverage file
$coverageFile = Get-ChildItem -Path ./TestResults -Recurse -Filter "coverage.cobertura.xml" | Select-Object -First 1

if ($null -eq $coverageFile) {
    Write-Host "Coverage file not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found coverage file: $($coverageFile.FullName)" -ForegroundColor Green

# Generate HTML report with class-level detail
$reportDir = "./TestResults/CoverageReport"
$sourceDir = (Get-Item .).Parent.FullName

reportgenerator `
    "-reports:$($coverageFile.FullName)" `
    "-targetdir:$reportDir" `
    "-reporttypes:Html;Badges" `
    "-classfilters:-System.*;-Microsoft.*;-*Tests;-*TestBed;-*BenchMark" `
    "-sourcedirs:$sourceDir" `
    "-verbosity:Error"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Report generation failed!" -ForegroundColor Red
    Write-Host "Install reportgenerator: dotnet tool install --global dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
    exit $LASTEXITCODE
}

# Open report
$indexFile = "$reportDir/index.html"
Write-Host "Opening report: $indexFile" -ForegroundColor Cyan
Start-Process $indexFile
