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
    
  if ($source.Length -lt $oldLicenseHeader.Length)
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

#get-ChildItem -path $rootPath\Remotion -include '*.cs'   -exclude '*.designer.cs' -recurse | replace-licenseHeader 'licenseHeader.cs.txt' 'licenseHeader_old.cs.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.js'   -recurse | replace-licenseHeader 'licenseHeader.cs.txt' 'licenseHeader_old.cs.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.aspx' -recurse | replace-licenseHeader 'licenseHeader.asx.txt' 'licenseHeader_old.asx.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.ascx' -recurse | replace-licenseHeader 'licenseHeader.asx.txt' 'licenseHeader_old.asx.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.asax' -recurse | replace-licenseHeader 'licenseHeader.asx.txt' 'licenseHeader_old.asx.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.xml' -recurse | replace-licenseHeader 'licenseHeader.xml.txt' 'licenseHeader_old.xml.txt'
#get-ChildItem -path $rootPath\Remotion -include '*.build' -recurse | replace-licenseHeader 'licenseHeader.xml.txt' 'licenseHeader_old.xml.txt'

#get-ChildItem -path $rootPath\SecurityManager -include '*.cs'   -exclude '*.designer.cs' -recurse | replace-licenseHeader 'licenseHeaderRestrict.cs.txt' 'licenseHeaderRestrict_old.cs.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.aspx' -recurse | replace-licenseHeader 'licenseHeaderRestrict.asx.txt' 'licenseHeaderRestrict_old.asx.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.ascx' -recurse | replace-licenseHeader 'licenseHeaderRestrict.asx.txt' 'licenseHeaderRestrict_old.asx.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.asax' -recurse | replace-licenseHeader 'licenseHeaderRestrict.asx.txt' 'licenseHeaderRestrict_old.asx.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.master' -recurse | replace-licenseHeader 'licenseHeaderRestrict.asx.txt' 'licenseHeaderRestrict_old.asx.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.xml' -recurse | replace-licenseHeader 'licenseHeaderRestrict.xml.txt' 'licenseHeaderRestrict_old.xml.txt'
#get-ChildItem -path $rootPath\SecurityManager -include '*.build' -recurse | replace-licenseHeader 'licenseHeaderRestrict.xml.txt' 'licenseHeaderRestrict_old.xml.txt'

#get-ChildItem -path $rootPath\DMS -include '*.cs'  -exclude '*.designer.cs' -recurse | replace-licenseHeader 'licenseHeaderRevision.cs.txt' 'licenseHeaderRevision_old.cs.txt'
#get-ChildItem -path $rootPath\DMS -include '*.xml' -recurse | replace-licenseHeader 'licenseHeaderRevision.xml.txt' 'licenseHeaderRevision_old.xml.txt'
#get-ChildItem -path $rootPath\DMS -include '*.build' -recurse | replace-licenseHeader 'licenseHeaderRevision.xml.txt' 'licenseHeaderRevision_old.xml.txt'
