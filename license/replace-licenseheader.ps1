param
(
  [string]$rootPath = @( throw "Please specify a rootPath." ),
  [string]$logFile = @( throw "Please specify a logFile." )
)


filter replace-licenseHeader([string]$pathToLicenseHeaderFile, [string]$pathToOldLicenseHeaderFile)
{
  [string]$tempFile = 'c:\temp.txt'
  [string]$pathToSourceFile = $_.FullName
  
  $licenseHeader = get-content $pathToLicenseHeaderFile
  $oldLicenseHeader = get-content $pathToOldLicenseHeaderFile
  $source = get-content $pathToSourceFile
    
  if ($source.Length -lt $licenseHeader.Length)
  {
    $error = "File $_ already has a license header different from the old license header. Source file contains less lines than license file."
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
      $error = "File $_ already has a license header different from the old license header. Difference at line $($i+1)."
      write-host $error
      add-content -path $logFile -value $error
      return
    }
  }
  
  if ($source.Length -ge $oldLicenseHeader.Length -and $source[$oldLicenseHeader.Length].Length -eq 0)
  {
    $source[$oldLicenseHeader.Length] = $null;
  }

  set-content -path $tempFile -value $licenseHeader
  add-content -path $tempFile -value $source
  
  write-host Replacing license header to file $_.FullName
  move-item $tempFile $pathToSourceFile -force
}

#get-ChildItem -path $rootPath -include '*.cs' -exclude 'prereq\*' -recurse | replace-licenseHeader 'licenseHeader.cs.txt' 'licenseHeader_old.cs.txt'
get-ChildItem -path $rootPath -include '*.cs' -exclude 'prereq\*' -recurse | replace-licenseHeader 'licenseHeaderRestrict.cs.txt' 'licenseHeader.cs.txt'
#get-ChildItem -path $rootPath -include '*.js' -exclude 'prereq\*' -recurse | replace-licenseHeader 'licenseHeader.cs.txt'
#get-ChildItem -path $rootPath -include '*.aspx' -exclude 'prereq\*' -recurse | replace-licenseHeader 'licenseHeader.asx.txt'
#get-ChildItem -path $rootPath -include '*.ascx' -exclude 'prereq\*' -recurse | replace-licenseHeader 'licenseHeader.asx.txt'
