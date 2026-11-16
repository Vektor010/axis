$ids = @(
    'security-disable-system-restore-config',
    'security-block-domain-picture-password',
    'peripheral-no-autorun',
    'peripheral-dont-set-autoplay-checkbox',
    'peripheral-wisp-panning-disabled',
    'peripheral-wisp-touch-gate',
    'peripheral-pen-flick-mode',
    'peripheral-pen-sample-time',
    'power-hiberboot-enabled',
    'power-hide-lock-option',
    'power-user-battery-discharge-estimator',
    'visibility-prevent-color-appearance-change',
    'visibility-prevent-changing-wallpaper',
    'visibility-prevent-changing-theme',
    'visibility-use-default-tile',
    'system-allow-automatic-app-archiving',
    'system-disable-search-ui',
    'privacy-disable-cloud-optimized-content',
    'privacy-disable-consumer-account-state',
    'privacy-disable-third-party-suggestions',
    'privacy-disable-windows-ink-workspace',
    'privacy-disable-suggested-apps-ink-workspace',
    'privacy-disallow-input-personalization',
    'privacy-disallow-speech-model-update',
    'privacy-turn-off-rsop-logging'
)
Write-Host "Count: $($ids.Count)"
Write-Host "Unique: $(($ids | Select-Object -Unique).Count)"
