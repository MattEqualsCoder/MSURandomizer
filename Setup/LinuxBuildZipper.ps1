$parentFolder = Split-Path -parent $PSScriptRoot

# Get publish folder
$folder = "$parentFolder\MSURandomizer\bin\Release\net8.0\linux-x64\publish"
$winFolder = "$parentFolder\MSURandomizer\bin\Release\net8.0-windows\win-x86\publish"
if (-not (Test-Path $folder))
{
    $folder = "$parentFolder\MSURandomizer\bin\Release\net8.0\publish\linux-x64"
    $winFolder = "$parentFolder\MSURandomizer\bin\Release\net8.0-windows\publish\win-x86"
}

# Get version number from Randomizer.App
$version = "0.0.0"
if (Test-Path "$winFolder\MSURandomizer.App.exe") {
    $version = (Get-Item "$winFolder\MSURandomizer.exe").VersionInfo.ProductVersion
}
else {
    $version = (Get-Item "$folder\MSURandomizer.dll").VersionInfo.ProductVersion
}
$version = $version -replace "\+.*", ""

# Create package
$fullVersion = "MSURandomizerLinux_$version"
$outputFile = "$PSScriptRoot\Output\$fullVersion.tar.gz"
if (Test-Path $outputFile) {
    Remove-Item $outputFile -Force
}
if (-not (Test-Path $outputFile)) {
    Set-Location $folder
    tar -cvzf $outputFile *
}
Set-Location $PSScriptRoot