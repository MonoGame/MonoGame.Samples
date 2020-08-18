try {
        [string]$TargetTemplateName  = Read-Host "Enter a project name for your new sample"
        [string]$SampleTemplateName = 'SampleTemplate'
        $excludes = @('*.ps1', '*.png', '*.ico','*.bmp')
        Get-ChildItem -File -Recurse -Exclude $excludes | ForEach-Object { Rename-Item -Path $_.PSPath -NewName $_.Name.replace("$SampleTemplateName","$TargetTemplateName")}
        Get-ChildItem -Directory -Recurse -Include "*$SampleTemplateName*" | rename-item -NewName {$_.Name -replace "$SampleTemplateName","$TargetTemplateName"}
        Get-ChildItem -File -Recurse -Exclude $excludes | ForEach-Object  {(Get-Content $_.FullName) -replace $SampleTemplateName, $TargetTemplateName | Set-Content $_.FullName -Force }
        Remove-Item "InitializeTemplate.ps1"
    } 
        catch {
            Write-Error $_.Exception.Message
    }
