param
(
  [string]$rootPath = @( throw "Please specify a rootPath." )
)


filter add-licenseHeader([string]$pathToLicenseHeaderFile)
{
  [string]$tempFile = 'c:\temp.txt'
  [string]$pathToSourceFile = $_.FullName
  
  $licenseHeader = get-content $pathToLicenseHeaderFile
  $source = get-content $pathToSourceFile
  
  if ($source.Length -gt 0 -and $source[0] -eq $licenseHeader[0])
  {
    write-host File $_.FullName already has a license header.
    return
  }
 
  set-content -path $tempFile -value $licenseHeader
  add-content -path $tempFile -value $source
  
  write-host Adding license header to file $_.FullName
  move-item $tempFile $pathToSourceFile -force
}

get-ChildItem -path $rootPath\Remotion -include '*.cs' -exclude '*.designer.cs' -recurse | add-licenseHeader 'licenseHeader.cs.txt'
#get-ChildItem -path $rootPath -include '*.js' -exclude 'prereq\*' -recurse | add-licenseHeader 'licenseHeader.cs.txt'
#get-ChildItem -path $rootPath -include '*.aspx' -exclude 'prereq\*' -recurse | add-licenseHeader 'licenseHeader.asx.txt'
#get-ChildItem -path $rootPath -include '*.ascx' -exclude 'prereq\*' -recurse | add-licenseHeader 'licenseHeader.asx.txt'
