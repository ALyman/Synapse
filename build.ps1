param(
    [Parameter(Position = 1)]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',
    
    [switch] $Clean = $false,
    [switch] $Compile = $true,
    [switch] $Sign = $false,
    [switch] $Test = $true,
    [switch] $Coverage = $false,
    [switch] $Package = $false,
    [switch] $Deploy = $false,
    [switch] $Publish = $false,
    
    [System.IO.FileInfo] $PrivateKeyFile = "D:\Key-Private.snk"
)

$BasePath = Split-Path -Parent $MyInvocation.MyCommand.Path;
$MsBuildPath = (Get-Command msbuild | where { $_.FileVersionInfo.ProductVersion.StartsWith('4.0') }).Path;
$SNPath = (Get-Command sn | where { $_.FileVersionInfo.ProductVersion.StartsWith('4.0') }).Path;
$GitPath = (Get-Command git | where { $_.Extension -eq '.exe'}).Path;
$NuGetPath = Get-Item $BasePath\tools\NuGet.exe
$GallioEchoPath = Get-Item $BasePath\tools\Gallio\Gallio.Echo.exe
$GallioUtilityPath = Get-Item $BasePath\tools\Gallio\Gallio.Utility.exe
$DotCoverPath = Get-Item (Join-Path -Path (Split-Path -Parent ((Get-Item HKLM:\SOFTWARE\Classes\dotCover.Snapshot\DefaultIcon\).GetValue($null).Replace('"',''))) dotCover.exe)

[System.IO.FileInfo] $PublicKeyFile = Get-Item $BasePath\Key-Public.snk

function Invoke-BuildProcess {
    $Deploy = $Deploy -or $Publish;
    $Sign = $Sign -or $Deploy;
    $Package = $Package -or $Deploy;
    $Test = $Test -or $Package;
    $Test = $Test -or $Coverage;
    $Compile = $Compile -or $Test;

    if ($Sign) {
        $BuildOutputBasePath = Join-Path $BasePath build\$Configuration-Signed
    } else {
        $BuildOutputBasePath = Join-Path $BasePath build\$Configuration
    }
    $TestOutputBasePath = Join-Path $BuildOutputBasePath tests
    $PackageOutputBasePath = Join-Path $BuildOutputBasePath package

    Write-Host "Building: $Configuration " -NoNewline
    if ($Clean) { Write-Host "Clean " -NoNewline }
    if ($Compile) { Write-Host "Compile " -NoNewline }
    if ($Sign) { Write-Host "Sign " -NoNewline }
    if ($Test) { Write-Host "Test " -NoNewline }
    if ($Coverage) { Write-Host "Coverage " -NoNewline }
    if ($Package) { Write-Host "Package " -NoNewline }
    if ($Deploy) { Write-Host "Deploy " -NoNewline }
    Write-Host
    Write-Host "Output Path: $BuildOutputBasePath";

    $Errors = 0;

    if ($Sign -and -not (Test-Path $PrivateKeyFile)) {
        Write-Host -ForegroundColor Red "Error: Can not sign, private key file not found: '$PrivateKeyFile'"
        $Errors++;
    }
    
    if ((Test-Path $PrivateKeyFile) -and -not (Test-Path $PublicKeyFile)) {
        $PublicKeyFile = Join-Path $env:TEMP ([System.IO.Path]::GetRandomFileName());
        & $SNPath -q -p `"$PrivateKeyFile`" `"$PublicKeyFile`"
    }

    if ($Package -and ($Configuration -eq 'Debug')) {
        Write-Host -ForegroundColor Red "Error: Can not package with configuration '$Configuration', must be 'Release' or 'Official'"
        $Errors++;
    }

    if ($Deploy -and (-not $Sign)) {
        Write-Host -ForegroundColor Red "Error: Can not deploy unsigned binaries"
        $Errors++;
    }

    $PendingChanges = & $GitPath status "$BasePath" --porcelain --untracked-files=all

    if ($Deploy -and ($PendingChanges)) {
        Write-Host -ForegroundColor Red "Error: Can not package with pending changes:";
        $PendingChanges | %{ "    $_" } | Write-Host -ForegroundColor Yellow
        $Errors++;
    }

    if ($Errors -gt 0) { return; }

    if ($Clean -and (Test-Path $BuildOutputBasePath)) { Remove-Item $BuildOutputBasePath -Recurse -Force }

    New-Item -Type Directory -Force -Path $BuildOutputBasePath,$TestOutputBasePath,$PackageOutputBasePath | Out-Null

    $SourceProjects = @(Join-Path $BasePath source\Synapse\Synapse.csproj);
    $TestProjects = @(Join-Path $BasePath tests\Synapse.Tests\Synapse.Tests.csproj);

    if ($Compile) { Build-Project $BasePath\Synapse.sln; }
    
    $SourceAssemblies = $SourceProjects | Get-ProjectAssemblyFile
    $TestAssemblies = $TestProjects | Get-ProjectAssemblyFile
    $Version = $SourceAssemblies | Get-AssemblyVersion | Select-Object -Index 0
    $SourcePackageSpecifications = @(dir $BasePath\source -Recurse -Include *.nuspec);
    
    if ($Sign) {
        Write-Host -ForegroundColor DarkGreen "Checking to make sure assemblies were signed... "
        $PublicKeyToken = Get-PublicKeyToken $PublicKeyFile
        @($SourceAssemblies, $TestAssemblies) | %{
            $AssemblyPublicKeyToken = Get-AssemblyPublicKeyToken $_
            if ($AssemblyPublicKeyToken -ne $PublicKeyToken) {
                Write-Host -ForegroundColor Red "Error: Signing failed for assembly $_; incorrect public key ($AssemblyPublicKeyToken -eq $PublicKeyToken)";
                throw;
            }
        }
        Write-Host -ForegroundColor Green "Done."
    }

    if ($Test) { $TestAssemblies | Test-Assembly }
    if ($Package) { $SourcePackageSpecifications | %{ Build-Package $_ -Version $Version } }
    
    $SourcePackages = @(dir $PackageOutputBasePath -Recurse -Include *.$Version.nupkg);
    
    if ($Deploy) {
        & $GitPath tag `"v$Version`" -m "Deployed to NuGet feed"
        $SourcePackages | % { Publish-Package $_ }
    }
}

####################################################
# Build Steps
####################################################

function Build-Project {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $Project
    )
    
    Write-Host -ForegroundColor DarkGreen "Building $Project ... "
    
    & $MsBuildPath "$Project" `
        /p:Configuration=$Configuration `
        /p:BuildOutputBasePath=`"$BuildOutputBasePath`" `
        /nologo `
        /consoleloggerparameters:Verbosity=minimal `
        /p:SignAssembly=$Sign `
        /p:AssemblyOriginatorKeyFile=`"$PrivateKeyFile`" `
        /p:DefaultDefineConstants=TEST_MBUNIT
    
    if (-not $?) {
        throw "Failed";
    }

    Write-Host -ForegroundColor Green "Done."
}

function Test-Assembly {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $Assembly
    )

    Write-Host -ForegroundColor DarkGreen "Testing $Assembly ... "
    
    $GallioArguments = @(
            "`"$Assembly`"",
            "/report-type:Xml",
            "/report-directory:`"$TestOutputBasePath`"",
            "/report-name-format:`"$($Assembly.BaseName).TestResults.$Version`"",
            "/hint-directory:$BuildOutputBasePath\lib\net40"
    )
            
    if ($Coverage) {
        $CoverageParamsFile = Join-Path $ENV:TEMP ([System.IO.Path]::GetRandomFileName())
([XML] @"
    <CoverageParams>
        <TargetExecutable>$GallioEchoPath</TargetExecutable>
        <TargetArguments>$($GallioArguments -join ' ')</TargetArguments>
        <Output>$TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.dcvr</Output>
        <Filters>
          <IncludeFilters>
            <FilterEntry>
              <ModuleMask>*</ModuleMask>
              <ClassMask>*</ClassMask>
              <FunctionMask>*</FunctionMask>
            </FilterEntry>
          </IncludeFilters>
          <ExcludeFilters>
            <FilterEntry>
              <ModuleMask>*$($Assembly.BaseName)*</ModuleMask>
              <ClassMask>*</ClassMask>
              <FunctionMask>*</FunctionMask>
            </FilterEntry>
            <FilterEntry>
              <ModuleMask>*Gallio*</ModuleMask>
              <ClassMask>*</ClassMask>
              <FunctionMask>*</FunctionMask>
            </FilterEntry>
            <FilterEntry>
              <ModuleMask>*MbUnit*</ModuleMask>
              <ClassMask>*</ClassMask>
              <FunctionMask>*</FunctionMask>
            </FilterEntry>
          </ExcludeFilters>
        </Filters>
      </CoverageParams>
"@).OuterXml | Set-Content $CoverageParamsFile
        & $DotCoverPath cover `"$CoverageParamsFile`"
        & $DotCoverPath report `
            /Source=`"$TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.dcvr`" `
            /Output=`"$TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.xml`" `
            /ReportType=XML | Out-Null
        & $DotCoverPath report `
            /Source=`"$TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.dcvr`" `
            /Output=`"$TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.html`" `
            /ReportType=HTML | Out-Null
            
        $CoverageResults = [xml] (Get-Content $TestOutputBasePath\$($Assembly.BaseName).Coverage.$Version.xml)
        
        $CoveragePercent = [int] $CoverageResults.Root.CoveragePercent
        $CoveredStatements = [int] $CoverageResults.Root.CoveredStatements
        $TotalStatements = [int] $CoverageResults.Root.TotalStatements
        
        $color = 'Green';        
        if ($CoveragePercent -lt 100) { $color = 'Yellow' }
        if ($CoveragePercent -lt 90) { $color = 'Red' }
        Write-Host -ForegroundColor $color "Coverage: $CoveragePercent`%, covering $CoveredStatements of $TotalStatements statements."
    } else {
        & $GallioEchoPath $GallioArguments
    }
    
    & $GallioUtilityPath FormatReport `
        "$TestOutputBasePath\$($Assembly.BaseName).TestResults.$Version.xml" `
        /ReportType:Html `
        /ReportOutput:"$TestOutputBasePath"
    
    if (-not $?) {
        throw "Failed";
    }

    Write-Host -ForegroundColor Green "Done."
}

function Build-Package {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $PackageSpecFile,
        
        [Parameter(Mandatory = $true)]
        [System.Version] $Version
    )
    
    $PackageSpecFile = Copy-Item $PackageSpecFile $PackageOutputBasePath -PassThru
    
    (Get-Content $PackageSpecFile) | %{
        $_.Replace(
            '$(BasePath)', $BasePath
        ).Replace(
            '$(Version)', $Version
        )
    } |    Set-Content $PackageSpecFile
    
    $PackageFile = [System.IO.FileInfo] (Join-Path $PackageOutputBasePath ($PackageSpecFile.BaseName + ".$Version.nupkg"));
    $SymbolPackageFile = [System.IO.FileInfo] (Join-Path $PackageOutputBasePath ($PackageSpecFile.BaseName + ".$Version.Symbols.nupkg"));
    
    Write-Host -ForegroundColor DarkGreen "Packaging $PackageSpecFile (Symbols)... "
    
    if ((Test-Path $PackageFile) -and (Test-Path $SymbolPackageFile)) {
        Write-Host -ForegroundColor Yellow "Skipped."
        return;
    }
    
    & $NuGetPath pack "$PackageSpecFile" `
        /BasePath "$BuildOutputBasePath" `
        /OutputDirectory "$PackageOutputBasePath" `
        /Version "$Version"
        
    if (Test-Path $SymbolPackageFile) {
        Remove-Item $SymbolPackageFile
    }
    
    if (-not $?) {
        throw "Failed";
    }

    Write-Host -ForegroundColor Green "Done."
    
    Rename-Item `
        -Path $PackageFile `
        -NewName $SymbolPackageFile.Name `
        -Force;
    
    Write-Host -ForegroundColor DarkGreen "Packaging $PackageSpecFile (Binaries)... "
    
    & $NuGetPath pack "$PackageSpecFile" `
        /BasePath "$BuildOutputBasePath" `
        /OutputDirectory "$PackageOutputBasePath" `
        /Version "$Version" `
        /Exclude **\*.pdb `
        /Exclude **\*.cs
    
    if (-not $?) {
        throw "Failed";
    }

    Write-Host -ForegroundColor Green "Done."
}

function Publish-Package {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $PackageFile
    )

    Write-Host -ForegroundColor DarkGreen "Deploying $PackageFile ... "
    
    if ($Publish) {
        & $NuGetPath push `"$PackageFile`"
    } else {
        & $NuGetPath push `"$PackageFile`" `
            /CreateOnly
    }
    
    if (-not $?) {
        throw "Deployment Failed";
    }

    Write-Host -ForegroundColor Green "Done."
}

####################################################
# Utility Functions
####################################################

function Get-ProjectProperty {
    param(
        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline=$true, ParameterSetName = "Project")]
        [System.IO.FileInfo] $Project,

        [Parameter(Mandatory = $true, Position = 1, ValueFromPipeline=$true, ParameterSetName = "ProjectXml")]
        [xml] $ProjectXml,

        [Parameter(Mandatory = $true, Position = 2)]
        [string] $Property
    )
    
    if (-not $ProjectXml) { $ProjectXml = [xml](Get-Content $Project) }
    
    $XmlNS = @{
        msbuild = 'http://schemas.microsoft.com/developer/msbuild/2003'
    };
    
    $Properties = @(Select-Xml $ProjectXml -XPath "/msbuild:Project/msbuild:PropertyGroup[not(@Condition)]/msbuild:$Property | /msbuild:Project/msbuild:PropertyGroup[@Condition = `" '`$(Configuration)|`$(Platform)' == '$Configuration|AnyCPU' `"]/msbuild:$Property" -Namespace $XmlNS)
    
    return ($Properties[$Properties.Length - 1]).Node.InnerText;
}

function Get-ProjectAssemblyFile {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $Project
    )
    
    $ProjectXml = [xml](Get-Content $Project);
    
    $OutputPath = (Get-ProjectProperty $ProjectXml 'OutputPath').Replace(
        '$(BuildOutputBasePath)', $BuildOutputBasePath
    );
    $AssemblyName = Get-ProjectProperty $ProjectXml 'AssemblyName'
    $OutputType = Get-ProjectProperty $ProjectXml 'OutputType'
    
    switch ($OutputType) {
        "Library" { $AssemblyFile = $AssemblyName + ".dll" }
        default { $AssemblyFile = $AssemblyName + ".exe" }
    }
    
    return Get-Item (Join-Path $OutputPath $AssemblyFile)
}

function Get-AssemblyVersion {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $AssemblyFile
    )
    
    return [System.Diagnostics.FileVersionInfo]::GetVersionInfo($AssemblyFile).ProductVersion
}

function Get-AssemblyPublicKeyToken {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline=$true)]
        [System.IO.FileInfo] $AssemblyFile
    )
    
    $Token = & $SNPath -q -T "$AssemblyFile"
    if ((-not $?) -or -not ($Token -match "Public key token is ([0-9a-f]+)")) {
        return $null;
    }
    return $Matches[1]
}

function Get-PublicKeyToken {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo] $KeyFile
    )
    
    $Token = & $SNPath -q -t "$KeyFile"
    if ((-not $?) -or -not ($Token -match "Public key token is ([0-9a-f]+)")) {
        return $null;
    }
    return $Matches[1]
}

Invoke-BuildProcess;
