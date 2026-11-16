$keys = @(
    @{ id='security-dynamic-lock'; p='HKCU\Software\Microsoft\Windows NT\CurrentVersion\Winlogon'; v='EnableGoodbye' },
    @{ id='visibility-icons-only'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='IconsOnly' },
    @{ id='visibility-use-compact-mode'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='UseCompactMode' },
    @{ id='visibility-show-type-overlay'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowTypeOverlay' },
    @{ id='visibility-folder-contents-info-tip'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='FolderContentsInfoTip' },
    @{ id='visibility-full-path'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\CabinetState'; v='FullPath' },
    @{ id='visibility-hide-drives-with-no-media'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='HideDrivesWithNoMedia' },
    @{ id='visibility-hide-file-ext'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='HideFileExt' },
    @{ id='visibility-hide-merge-conflicts'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='HideMergeConflicts' },
    @{ id='visibility-show-super-hidden'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowSuperHidden' },
    @{ id='visibility-separate-process'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='SeparateProcess' },
    @{ id='visibility-persist-browsers'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='PersistBrowsers' },
    @{ id='visibility-show-encrypt-compressed-color'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowEncryptCompressedColor' },
    @{ id='visibility-show-info-tip'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowInfoTip' },
    @{ id='visibility-show-preview-handlers'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowPreviewHandlers' },
    @{ id='visibility-show-status-bar'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowStatusBar' },
    @{ id='visibility-show-sync-provider-notifications'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='ShowSyncProviderNotifications' },
    @{ id='visibility-auto-check-select'; p='HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'; v='AutoCheckSelect' }
)
foreach ($k in $keys) {
    $val = reg query $k.p /v $k.v 2>&1
    if ($LASTEXITCODE -eq 0) {
        $line = ($val | Select-String $k.v).ToString().Trim()
        Write-Host "$($k.id) | $line"
    } else {
        Write-Host "$($k.id) | ключа нет"
    }
}
