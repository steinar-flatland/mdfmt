<#
This script requires an environment variable, MDFMT_ROOT, to be set.  MDFMT_ROOT is the directory
that contains the Mdfmt code, i.e., it is the root directory of the local Github repo. This script
deletes (if present) and rebuilds the directory, $env:MDFMT_ROOT/release.  This release directory
is populated with the binary release files that will be attached to the release in GitHub.
#>

function ConfirmEnvironment {
    if (-not $env:MDFMT_ROOT) {
        throw "The environment variable MDFMT_ROOT needs to be set to the directory that contains the Mdfmt code that was cloned from GitHub."
    }
}

function GetVersion {
    param (
        [string]$filePath
    )

    # Use Select-String to find the matching line
    $result = Select-String -Pattern "Version =" -Path $filePath

    # Extract the version using a regex match
    if ($result -match 'Version\s*=\s*\"([\d\.]+)\"') {
        return $matches[1] # Return the captured version
    }

    throw "Unable to find Version in $filePath"
}

function DeleteDirectory {
    param (
        [string]$directoryPath
    )

    if (Test-Path -Path $directoryPath) {
        Remove-Item -Path $directoryPath -Recurse -Force -ErrorAction Stop
        Write-Host "Deleted directory '$directoryPath'." -ForegroundColor Green
    } else {
        Write-Host "Directory '$directoryPath' does not exist." -ForegroundColor Green
    }
}

function PackageZip {
    param (
        [string]$runtime,
        [bool]$selfContained,
        [string]$zipFileName
    )

    Write-Host 
    Write-Host "Packaging zip for $zipFileName" -ForegroundColor Cyan

    dotnet publish $mdfmtRoot/src/Mdfmt/Mdfmt.csproj -c Release -r $runtime --self-contained $selfContained --artifacts-path $releaseBuildDir
    Copy-Item -Recurse -Path $releaseBuildDir/publish/Mdfmt/release_$runtime $releaseBuildDir/mdfmt
    7z a -tzip $releaseDir/$zipFileName $releaseBuildDir/mdfmt
    if ($LASTEXITCODE -ne 0) {
        throw "7z command failed with exit code $LASTEXITCODE."
    }
    DeleteDirectory -directoryPath $releaseBuildDir
}

# Enable fail-fast behavior
$ErrorActionPreference = "Stop"

try {

    ConfirmEnvironment

    $mdfmtRoot = $env:MDFMT_ROOT -replace '\\', '/'
    Write-Host "MDFMT_ROOT = $mdfmtRoot." -ForegroundColor Green

    # Get the version of the software being released.
    $version = GetVersion -filePath "$mdfmtRoot/src/Mdfmt/Program.cs"
    $version = $version -replace '\.', '-'
    Write-Host "Version from Program.cs is $version." -ForegroundColor Green

    # Build the code here
    $releaseBuildDir = "$mdfmtRoot/releasebuild"
    DeleteDirectory -directoryPath $releaseBuildDir

    # Package the code here
    $releaseDir = "$mdfmtRoot/release" 
    DeleteDirectory -directoryPath $releaseDir

    PackageZip -runtime "win-x64" -selfContained $true -zipFileName "mdfmt_${version}_win-x64_self-contained-net8.0.zip"
    PackageZip -runtime "linux-x64" -selfContained $true -zipFileName "mdfmt_${version}_linux-x64_self-contained-net8.0.zip"
    PackageZip -runtime "win-x64" -selfContained $false -zipFileName "mdfmt_${version}_win-x64_framework-dependent-net8.0.zip"
    PackageZip -runtime "linux-x64" -selfContained $false -zipFileName "mdfmt_${version}_linux-x64_framework-dependent-net8.0.zip"

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Exit 1
}

Write-Host "Successful exit" -ForegroundColor Green
Exit 0
