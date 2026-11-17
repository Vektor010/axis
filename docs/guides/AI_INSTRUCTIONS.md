# 🤖 WinTweaker: Инструкция для ИИ-разработчиков (AI Context & Architecture)

Этот документ предназначен для языковых моделей (LLM / AI Agents — Gemini, Claude, GPT, Cursor, Copilot, Antigravity), которые продолжают разработку, тестирование, рефакторинг или расширение проекта **WinTweaker**.

---

## 1. Стек технологий и среда сборки

- **Язык**: C# 5.0 (строгая совместимость с компилятором `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`).
  - ⚠️ **Запрещено использовать синтаксис C# 6.0+**: интерполяцию строк `$"..."`, оператор `?.`, автосвойства только для чтения `{ get; } = val;`, `nameof(...)`. Используйте `string.Format(...)`, явные проверки на `null`, полные свойства с private-полями.
- **Платформа**: .NET Framework 4.6.2 (WPF / XAML).
- **Размер бинарника**: строго **< 5.0 МБ** (текущий размер `Axis.exe` ~ 1.22 МБ). Запрещено подключать тяжеловесные внешние NuGet-пакеты.
- **Скрипт сборки и обязательной проверки**: `powershell -ExecutionPolicy Bypass -File .\build.ps1` (собирает `dist/Axis.exe` и `dist/Tests.exe`, а затем **автоматически запускает `verify-all.ps1`**).
- **Сверка с первоисточником (`nohuto/win-config` и `noverse.dev`)**: `powershell -ExecutionPolicy Bypass -File .\sync-upstream.ps1`.

---

## 2. Структура проекта

```
win-tweaker/
├── app.xaml / app.xaml.cs          # Точка входа WPF, словарь ресурсов темы Tachyon
├── mainwindow.xaml / .cs           # Главное окно, навигация, оверлейный скроллбар, заголовок
├── tweaker.csproj                  # Проект MSBuild для Tweaker.exe
├── build.ps1 / build.cmd           # Автономная сборка через .NET Framework MSBuild
├── data/
│   ├── tweaks.json                 # База из 856 твиков системы
│   ├── tweaks.schema.json          # JSON-схема валидации твиков
│   └── source/win-config/          # Первоисточники desc.md (верификация диапазонов строк)
├── src/
│   ├── viewmodels/                 # MainViewModel, TweakCardViewModel, NavigationItemViewModel
│   ├── engine/                     # TweakEngine, SystemInfoService, JournalManager, MockRegistryStore
│   └── common/                     # NativeMethods (DWM / Win32 API), Converters, Placement
├── scripts/                        # 50 готовых автономных PowerShell (.ps1) скриптов
└── tests/                          # 12 тест-сьюитов, 7760 тестов (dist/Tests.exe)
```

---

## 3. Архитектура твиков и безопасность

### Формат данных (`data/tweaks.json`)
Каждый твик обязан соответствовать `data/tweaks.schema.json`:
- `id` — уникальный строковый идентификатор.
- `name_ru` и `description_ru` — название и описание на русском языке.
- `category` — категория (`security`, `visibility`, `system`, `privacy`, `network`, `nvidia`, `steam`, `peripheral`, `power`, `cleanup`, `affinities`, `misc`).
- `source_ref` — ссылка на источник: `<category>/desc.md:<line_from>-<line_to>`.
- `apply` и `revert` — действия в реестре (путь, имя параметра, тип, данные).
- `presets` — массив тегов пресетов.

### Режимы работы
- **Реальный боевой режим (по умолчанию)**: `_isRealMode = true`. Приложение сразу готово применять твики к системному реестру Windows без лишних тестовых заглушек и всплывающих подтверждений (при запуске без прав администратора автоматически запрашивается UAC через манифест).
- **Тестовый (песочница) режим**: активируется флагом `--test` / `--mock` (и используется автотестами `Tests.exe` на виртуальном `MockRegistryStore`).
- **Действия очистки (`category == "cleanup"`)**: отображаются в UI как кнопки `[🗑 Очистить]` (свойство `IsActionTweak`), а не как тумблеры включения/выключения.

---

## 4. Стандарты UI (Tachyon Design 2026)

- **Палитра Тёмной темы**:
  - `WindowBg`: `#0B0B12`
  - `SidebarBg`: `#0F0F17`
  - `HeaderBg`: `#101018`
  - `CardBg`: `#12121B` (Hover: `#181826`)
  - `CardBorder`: `#242434` (Hover: `#363650`)
  - `AccentBrush`: `#8B7CF6`
  - `TextPrimary`: `#E8E8F2`
  - `TextSecondary`: `#8B8B9E`
- **Окраска заголовка окна Windows 11 (DWM)**:
  В `NativeMethods.ApplyWindowFrameColors(hwnd, isDark)` формат цвета — Win32 COLORREF (`0x00BBGGRR`):
  - Тёмная тема: `captionColor = 0x00120B0B` (`#0B0B12`), `borderColor = 0x00342424`, `textColor = 0x00F2E8E8`.
  - Светлая тема: `captionColor = 0x00F8F3F3` (`#F3F3F8`), `borderColor = 0x00ECE1E1`, `textColor = 0x00281B1B`.
  - ⚠️ Запрещено менять эти цвета на несоответствующие фону окна.
- **Поиск**:
  - Фиксированная ширина `380 px`, без прыгающих анимаций ширины.
  - Каретка: `CaretBrush="{DynamicResource TextPrimary}"`.
  - Клик по контейнеру не блокирует события (`e.Handled` не выставляется в `true`).
- **Скроллбар**:
  - Ультратонкий плавающий оверлей `4 px` в том же Grid-слое, что и контент. Никогда не выделять скроллбару отдельную колонку (это вызывает горизонтальные рывки контента).

---

## 5. Система непрерывного тестирования и проверки кода (`verify-all.ps1` + `Tests.exe`)

- **Обязательная проверка при каждой сборке**: `.\build.ps1` автоматически вызывает `.\verify-all.ps1`:
  1. PowerShell AST-валидация всех 52 `.ps1` скриптов (PS 5.1 `ParseFile` + UTF-8 `ParseInput`).
  2. Глубокий аудит всех 856 твиков (`data/tweaks.json`): уникальность ID, чистота путей реестра без `\r\n`/`\\`, валидность типов, чистота русских текстов.
  3. Побайтовая SHA-256 сверка встроенного ресурса `tweaks.json` в `dist/Axis.exe` с `data/tweaks.json` + проверка лимита `< 5.0 МБ`.
  4. Прогон `dist/Tests.exe` — **13 тест-сьюитов, 7760 PASS / 0 FAIL**:
     - 0. Валидация JSON-схемы `tweaks.schema.json`.
     - 1. Сверка каждого твика с первоисточниками `desc.md` (диапазоны строк, полярность, маркер `(default)`).
     - 2–6. Проверка UI-моделей, `RegistryStore`, `TweakEngine`, изоляции, `RegistryWatcher`, `RestorePointManager`.
     - 7. Дизайн Tachyon, On-Demand NVCPL и сервисы NVIDIA.
     - 8. Game Mode и WNF подсистема (`gm_effects`).
     - 9. Планирование GPU и WDDM Hardware Flip Queue.
     - 10. MMCSS Task Service (`SystemResponsiveness`, `NetworkThrottlingIndex`).
     - 11. Тотальное сквозное тестирование 856 твиков на `MockRegistryStore` (Apply → Enabled → Revert).
     - 12. Глубокий аудит целостности кода каждого твика и скриптов `scripts/`.
- **CI/CD шаблон**: `docs/ci-verify.yml.template` (при желании скопировать в `.github/workflows/ci-verify.yml` для автозапуска `.\build.ps1` на Windows-раннере).
- ⚠️ **Критическое правило**: Любые изменения кода, реестра, структур или ViewModels обязаны проходить `.\build.ps1` со статусом **7760 PASS / 0 FAIL** и 0 AST-ошибок.

---

## 6. CLI-аргументы Axis.exe

- `--real` — реальный режим с записью в системный реестр.
- `--light` — запуск со светлой темой.
- `--screenshot <path>` — безголовый рендеринг окна в PNG для автопроверки UI.
- `--tab <overview|tweaks|journal|history|settings|about>` — открытие конкретной вкладки.
- `--search <query>` — предустановленный поисковый запрос.
- `--category <id>` — выбор категории твиков.
- `--no-mica` — принудительное отключение Mica-бэкдропа.
