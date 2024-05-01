Set-Location $PSScriptRoot
Copy-Item -Path ..\MSURandomizerLibrary\bin\Release\*.nupkg -Destination $PSScriptRoot\Output
Copy-Item -Path ..\MSURandomizerUI\bin\Release\*.nupkg -Destination $PSScriptRoot\Output
Copy-Item -Path ..\MSURandomizer\bin\Release\*.nupkg -Destination $PSScriptRoot\Output