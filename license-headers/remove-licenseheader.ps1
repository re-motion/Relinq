param
(
  [string]$rootPath = @( throw "Please specify a rootPath." ),
  [string]$logFile = @( throw "Please specify a logFile." )
)


filter remove-licenseHeader([string]$pathToOldLicenseHeaderFile)
{
  [string]$tempFile = 'c:\temp.txt'
  [string]$pathToSourceFile = $_.FullName
  
  $oldLicenseHeader = get-content $pathToOldLicenseHeaderFile
  $source = get-content $pathToSourceFile
    
  if ($source.Length -lt $oldLicenseHeader.Length)
  {
    $error = "File $_ has a license header different from the old license header. Source file contains less lines than license file."
    write-host $error
    add-content -path $logFile -value $error
    return
  }

  for ($i = 0; $i -lt $oldLicenseHeader.Length; $i++)
  {
    if ($source[$i] -eq $oldLicenseHeader[$i])
    {
      $source[$i] = $null;
    }
    else
    {
      $error = "File $_ has a license header different from the old license header. Difference at line $($i+1)."
      write-host $error
      return
    }
  }
  
  if ($source.Length -ge $oldLicenseHeader.Length -and $source[$oldLicenseHeader.Length].Length -eq 0)
  {
    $source[$oldLicenseHeader.Length] = $null;
  }

  set-content -path $tempFile -value $source
  
  write-host Removing license header to file $_.FullName
  move-item $tempFile $pathToSourceFile -force
}

get-ChildItem -path $rootPath\Remotion  -include '*.cs' -exclude '*.designer.cs' -recurse | remove-licenseHeader 'licenseHeader_old.cs.txt' 
#get-ChildItem -path $rootPath\SecurityManager -include '*.designer.cs' -recurse | remove-licenseHeader 'licenseHeaderRestrict.cs.txt' 
#get-ChildItem -path $rootPath\SecurityManager -include '*.designer.cs' -recurse | remove-licenseHeader 'licenseHeaderRestrict_old.cs.txt' 
#get-ChildItem -path $rootPath\DMS -include '*.designer.cs' -recurse | remove-licenseHeader 'licenseHeaderRevision.cs.txt'
