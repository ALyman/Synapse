param(
    [Parameter(Position = 1)]
    [ValidateSet('Debug', 'Release', 'Official')]
    [string] $Configuration = 'Release',
    
    [switch] $Compile = $true,
    [switch] $Test = $true,
    [switch] $Package = $true
)

$Targets = @();
if ($Compile) { $Targets += "Compile"; }
if ($Test) { $Targets += "Test"; }
if ($Package) { $Targets += "Package"; }

msbuild `
    .\build\build.proj `
    /t:$([string]::Join(";", $Targets)) `
    /p:Configuration=$Configuration