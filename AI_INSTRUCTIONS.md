# 🤖 WinTweaker: Инструкция для ИИ-разработчиков (AI Context & Architecture)

Этот документ предназначен для языковых моделей (LLM / AI Agents — Gemini, Claude, GPT, Cursor, Copilot, Antigravity), которые продолжают разработку, тестирование, рефакторинг или расширение проекта **WinTweaker**.

---

## 1. Стек технологий и среда сборки

- **Язык**: C# 5.0 (строгая совместимость с компилятором `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe`).
  - ⚠️ **Запрещено использовать синтаксис C# 6.0+**: интерполяцию строк `$"..."`, оператор `?.`, автосвойства только для чтения `{ get; } = val;`, `nameof(...)`. Используйте `string.Format(...)`, явные проверки на `null`, полные свойства с private-полями.
- **Платформа**: .NET Framework 4.6.2 (WPF / XAML).
- **Размер бинарника**: строго **< 5.0 МБ** (текущий размер `Tweaker.exe` ~ 1.06 МБ). Запрещено подключать тяжеловесные внешние NuGet-пакеты.
- **Скрипт сборки**: `powershell -ExecutionPolicy Bypass -File .\build.ps1` (собирает `dist/Tweaker.exe` и `dist/Tests.exe`).

---

## 2. Структура проекта

```
win-tweaker/
├── app.xaml / app.xaml.cs          # Точка входа WPF, словарь ресурсов темы Tachyon
├── mainwindow.xaml / .cs           # Главное окно, навигация, оверлейный скроллбар, заголовок
├── tweaker.csproj                  # Проект MSBuild для Tweaker.exe
├── build.ps1 / build.cmd           # Автономная сборка через .NET Framework MSBuild
├── data/
│   ├── tweaks.json                 # База из 850 твиков системы
│   ├── tweaks.schema.json          # JSON-схема валидации твиков
│   └── source/win-config/          # Первоисточники desc.md (верификация диапазонов строк)
├── src/
│   ├── viewmodels/                 # MainViewModel, TweakCardViewModel, NavigationItemViewModel
│   ├── engine/                     # TweakEngine, SystemInfoService, JournalManager, MockRegistryStore
│   └── common/                     # NativeMethods (DWM / Win32 API), Converters, Placement
├── scripts/                        # 50 готовых автономных PowerShell (.ps1) скриптов
└── tests/                          # 12 тест-сьюитов, 7660 тестов (dist/Tests.exe)
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
- **Тестовый режим (по умолчанию)**: `_isRealMode = false`. Все операции применяются исключительно к виртуальному слою `MockRegistryStore` в памяти. Физический реестр Windows не изменяется.
- **Реальный режим**: активируется флагом `--real`. Требует прав Администратора и отображает защитный диалог подтверждения перед запуском.
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

## 5. Система тестирования (Tests.exe)

- **Команда запуска тестов**: `& ".\dist\Tests.exe"`
- **Количество тестов**: ровно **7660 PASS / 0 FAIL**.
- **Тест-сьюиты**:
  0. Валидация JSON-схемы `tweaks.schema.json`.
  1. Сверка каждого твика с первоисточниками `desc.md`.
  2. Проверка UI-моделей и параметров категорий.
  3-6. Валидация пресетов, типов данных и отката.
  7. On-Demand NVCPL и сервисы NVIDIA.
  8. Game Mode и WNF подсистема (`gm_effects`).
  9. Планирование GPU и WDDM Hardware Flip Queue.
  10. MMCSS Task Service (`SystemResponsiveness`, `NetworkThrottlingIndex`).
  11. Тотальное сквозное тестирование 850 твиков на `MockRegistryStore`.
- ⚠️ **Критическое правило**: Любые изменения кода, реестра, структур или ViewModels обязаны сохранять результат **7660 PASS / 0 FAIL**.

---

## 6. CLI-аргументы Tweaker.exe

- `--real` — реальный режим с записью в системный реестр.
- `--light` — запуск со светлой темой.
- `--screenshot <path>` — безголовый рендеринг окна в PNG для автопроверки UI.
- `--tab <overview|tweaks|journal|history|settings|about>` — открытие конкретной вкладки.
- `--search <query>` — предустановленный поисковый запрос.
- `--category <id>` — выбор категории твиков.
- `--no-mica` — принудительное отключение Mica-бэкдропа.
