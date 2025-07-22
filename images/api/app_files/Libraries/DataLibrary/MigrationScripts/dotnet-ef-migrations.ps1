<#
## stup
dotnet tool install --global dotnet-ef --version 8.0.14
# add dotnet-ef to path
# $env:PATH += "$ENV:PROFILE\.dotnet\tools\;"
# run docker\inventory\images\api\dotnet-ef-migrations.ps1
# copy the migration script to docker\inventory\images\api\app_files\Libraries\DataLibrary\MigrationScripts to docker\inventory\images\sql\migrations
#>

# Set-Location $StartupProjectLocation
# [xml]$csproj = Get-Content .\app.csproj
# $version = ($csproj.Project.ItemGroup.PackageReference | Where-Object{$_.Include -eq "Microsoft.EntityFrameworkCore"}).Version
# dotnet add package Microsoft.EntityFrameworkCore.Design --version $version

# Set-Location $ContextsProjectLocation
# [xml]$csproj = Get-Content .\DataLibrary.csproj
# $version = ($csproj.Project.ItemGroup.PackageReference | Where-Object{$_.Include -eq "Microsoft.EntityFrameworkCore"}).Version
# dotnet add package Microsoft.EntityFrameworkCore.Design --version $version

# dotnet tool install --global dotnet-ef --version $version

Clear-Host

$IsMajor = $false
$IsMinor = $false
$IsPatch = $true

$ErrorActionPreference = "Stop"

try {
    $StartupProjectLocation = "C:\inventory\images\api\app_files\app"
    $ContextsProjectLocation = "C:\inventory\images\api\app_files\Libraries\DataLibrary"
    $MigrationScriptsLocation = "C:\inventory\images\api\app_files\Libraries\DataLibrary\MigrationScripts"

    [version]$VersionFile = Get-Content -Raw "C:\inventory\VERSION"

    if ($IsMajor -and $IsMinor) {
        Write-Error "Cannot set both IsMajor and IsMinor to true. Exiting."
        break
    }
    elseif ($IsMajor -and $IsPatch) {
        Write-Error "Cannot set both IsMajor and IsPatch to true. Exiting."
        break
    }
    elseif ($IsMinor -and $IsPatch) {
        Write-Error "Cannot set both IsMinor and IsPatch to true. Exiting."
        break
    }
    elseif (-not $IsMajor -and -not $IsMinor -and -not $IsPatch) {
        Write-Error "Must set at least one of IsMajor, IsMinor, or IsPatch to true. Exiting."
        break
    }

    [version]$version = ($IsMajor ? "$($VersionFile.Major + 1).0.0" : `
            $IsMinor ? "$($VersionFile.Major).$($VersionFile.Minor + 1).0" : `
            "$($VersionFile.Major).$($VersionFile.Minor).$($VersionFile.Build + 1)")


    Write-Host "Getting database contexts" -ForegroundColor Green
    Set-Location $ContextsProjectLocation
    $contexts = @(dotnet-ef dbcontext list | Where-Object { $_ -like "*Contexts*" })

    if ($LASTEXITCODE -gt 0) {
        Write-Error "Error getting contexts.`n$context.`nExiting."
        break
    }

    Get-ChildItem $MigrationScriptsLocation -Recurse -Force -Filter "*.sql" |
    ForEach-Object {
        Write-Host "Removing $($_.Name)" -ForegroundColor DarkGray
        Remove-Item $_.FullName -Force
    }

    foreach ($context in $contexts) {
        Write-Host "Found context $context" -ForegroundColor Green

        $migrations = @(dotnet-ef migrations list --context $context --startup-project $StartupProjectLocation)

        if ($LASTEXITCODE -gt 0) {
            Write-Error "Error getting migrations.`n$migrations.`nExiting."
            break
        }

        $ScriptVersion = "v$version.0"

        if ([string]$migrations -notlike '*No migrations were found*') {
            [string]$lastMigration = $migrations[-1]
            if ($lastMigration -notmatch "\(Pending\)") {
                if ($IsPatch) {
                    [int]$newPatchNumber = [Int64]::Parse($lastMigration[-1]) + 1
                    $ScriptVersion = ($ScriptVersion[0..($ScriptVersion.Length - 2)] -join "") + $newPatchNumber
                }
                dotnet-ef --startup-project $StartupProjectLocation migrations add $ScriptVersion --context $context
            }
            else {
                Write-Host ""
                Write-Host "Pending migrations found $lastMigration." -ForegroundColor DarkYellow
            }
        }
        else {
            Write-Host ""
            Write-Host "No previous migrations. Creating Initial Migration $ScriptVersion"
            dotnet-ef --startup-project $StartupProjectLocation migrations add $ScriptVersion --context $context
        }

        if ($LASTEXITCODE -gt 0) {
            Write-Error "Error running dotnet-ef."
            break
        }

        Write-Host ""
        Write-Host "adding migration script $MigrationScriptsLocation\$context`_$ScriptVersion.sql" -ForegroundColor Green
        dotnet-ef --startup-project $StartupProjectLocation migrations script --context $context -i -o "$MigrationScriptsLocation\$context`_$ScriptVersion.sql"
        if ($LASTEXITCODE -gt 0) {
            Write-Error "Error running dotnet-ef."
            break
        }

        Write-Host ""
        Write-Host "Updating database for context $context" -ForegroundColor Green
        dotnet-ef database update --startup-project $StartupProjectLocation --context $context
        if ($LASTEXITCODE -gt 0) {
            Write-Error "Error running dotnet-ef."
            break
        }
    }
}
catch {
    Write-Error "Error running dotnet-ef. $_"
    break
}
finally {
    Write-Host "Done." -ForegroundColor Green
}