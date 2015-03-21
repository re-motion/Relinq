Write-Host "Rebuilding project..."
$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
& $msbuild "PerformanceTests.csproj" "/property:Configuration=Release" "/target:Clean;Build" "/nologo" "/verbosity:quiet"

Write-Host "Launching performance test..."
$process = "bin\Release\Remotion.Linq.PerformanceTests.exe"
$timer = [System.Diagnostics.Stopwatch]::StartNew();
$timer.Start();
& $process "QueryParser"
$timer.Stop();

$elapsedTime = "{0:N0}" -f ($timer.ElapsedMilliseconds)
Write-Host "Launching process and creating the QueryParser took " $elapsedTime "ms, reference time: ~170ms"

$timer = [System.Diagnostics.Stopwatch]::StartNew();
$timer.Start();
& $process "NodeTypeProvider"
$timer.Stop();

$elapsedTime = "{0:N0}" -f ($timer.ElapsedMilliseconds)
Write-Host "Launching process and creating the NodeTypeProvider took " $elapsedTime "ms, reference time: ~160ms"
