# CLAUDE.md

Guidance for AI assistants (and humans) working in this repository.

## What this project is

**Aljaras** (الجرس, "the bell") is a Windows desktop **school bell / alarm
system**. It plays scheduled audio announcements (class start/end, breaks,
exam periods, morning assembly, emergencies) through the PC's speakers, runs
in the background from the system tray, and supports both **Arabic** and
**English** UIs. It is authored by Mustafa Al-Klieb.

The solution contains a single app, `Aljaras` (`WinExe`) — the bell
application (scheduler, audio, settings, tray icon). It is **freeware**: there
is no licensing/activation gate (the former machine-bound activation and its
standalone key-generator tool were removed).

## Tech stack

- **.NET 10** targeting `net10.0-windows` (`<Nullable>enable</Nullable>`).
- **WPF** (`<UseWPF>true</UseWPF>`) — XAML views + code-behind.
- **MVVM** via [`CommunityToolkit.Mvvm`](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) `8.4.0` — source-generated `[ObservableProperty]` / `[RelayCommand]`.
- **LiteDB** `5.0.21` — embedded NoSQL document database (`*.jrsdb` files).
- **NAudio** `2.2.1` — audio playback, recording, and device enumeration.
- **`IWshRuntimeLibrary`** COM reference — desktop shortcut creation.
- **System.Windows.Forms** — used only for the tray `NotifyIcon`.

This is a Windows-only application. It **cannot be built or run on Linux**;
remote/CI sessions can read, edit, and reason about the code but cannot
execute `dotnet build`/`run` against the WPF target.

## Repository layout

```
Aljaras.sln                  Solution (two projects)
Aljaras/                     Main application
  App.xaml(.cs)              Application entry point, tray icon, single-instance mutex, global error handlers
  AssemblyInfo.cs
  Aljaras.csproj             Note: contains a huge list of <None Update> audio assets copied to output
  Core/                      Non-UI helpers (see below)
  MVVM/
    Model/                   Data/persistence models (Alarm, Schedule, Holiday, UserSettings, AppLanguage, ...)
    View/                    XAML views + code-behind (one per feature screen)
    ViewModel/               ViewModels (one per view + the GlobalViewModel singleton)
  Themes/                    Shared XAML resource dictionaries (Buttons, Colors, Path, GroupBox, SliderCheckBox)
  Languages/                 Localization XML: en.xml, عربي.xml (Arabic)
  Audio/                     Bundled .mp3 announcements (Arabic filenames) for classes/exams/breaks
  Guide/                     Onboarding slide images (Slide1..7.PNG) + source .pptx
  Images/                    App images (Avatar, icon assets)
  Properties/                Settings.settings / Settings.Designer.cs
Assets/                      Shared package assets (app icon image)
*.jrsdb                      Checked-in LiteDB databases (see "Data & persistence" caveat)
```

## Architecture

### MVVM with CommunityToolkit source generators

Every ViewModel and most Models are `partial` classes deriving from
`ObservableRecipient` (or `ObservableObject` for `UserSettings`). Follow the
existing conventions exactly:

- Declare observable state as a **private/lowercase field** annotated with
  `[ObservableProperty]`. The generator creates the public PascalCase
  property. Example: `private string currentView` → bind to `CurrentView`.
- Declare commands as methods annotated with `[RelayCommand]`. The generator
  creates `XxxCommand`. e.g. `void ShowAlarmView()` → `ShowAlarmViewCommand`.
- React to property changes with the generated `partial void OnXxxChanged(T value)` hooks (see `Alarm.cs`, `GlobalViewModel.cs`).
- ViewModels are `internal` (except `ActivationViewModel`); keep new ones `internal partial` to match.

### View ↔ ViewModel wiring

Navigation is **ViewModel-first**. `MainViewModel.CurrentView` holds an
`object` that is the active ViewModel, and `App.xaml` maps each ViewModel type
to its View via `DataTemplate DataType="{x:Type viewModel:XxxViewModel}"`.
`MainWindow` renders `CurrentView` in a `ContentControl`. To add a screen:

1. Create `XxxView.xaml(.cs)` in `MVVM/View/`.
2. Create `internal partial class XxxViewModel : ObservableRecipient` in `MVVM/ViewModel/`.
3. Register the `DataTemplate` in `App.xaml`.
4. Add a `[RelayCommand] void ShowXxxView()` on `MainViewModel` that sets `CurrentView = new XxxViewModel();` and bind a button to it.

Existing screens: **Monitoring** (default/home — shows next alarm + countdown),
**Alarm** (schedules & alarms), **Holidays**, **Settings**, **Guide**,
**AboutMe**, plus a `SplashScreen`.

### GlobalViewModel — the shared singleton

`GlobalViewModel.Instance` (`MVVM/ViewModel/GlobalViewModel.cs`) is the heart of
the app and is referenced by most other ViewModels via a `Global` property. It:

- Owns the **1-second background loop** (a `Task.Run` while-loop) that updates
  the clock and fires the matching alarm's audio when `DateTime.Now` reaches an
  alarm time (`TrimMilliseconds` comparison).
- Runs a `DispatcherTimer` (~40ms) to drive the countdown display (`ShowTimeLeft`).
- Loads schedules/alarms/holidays from LiteDB (`LoadMonitoringAlarmCollectionData`),
  computes the next alarm (`NextAlarm`), and applies the per-weekday flags
  (`Sun`..`Sat`) on each `Alarm`.
- Handles microphone recording → speaker passthrough (live announcements) and
  the **Emergency** broadcast toggle, both via NAudio.
- Loads user settings and the active language XML (`SetAppSettings`).
- Surfaces transient toast-style messages (`NewNotificationMessage` → auto-removed after 3s).

When changing scheduling, audio, or persistence behavior, this is almost
always the file to edit.

### Core/ helpers (main app)

- `GlobalVariables` (in `Core/Enums.cs`) — static app-wide constants: `AppName`,
  `AppLocation` (the exe directory), DB path/connection string, and the single
  shared `LiteDatabase db` instance. **All DB access goes through `GlobalVariables.db`.**
- `Enums.cs` — also defines `DbTables`, `GetDayTime`, `GetVisibility`, `MessageBackground`.
  Note the convention of stringifying enums for XAML bindings (e.g. `GetVisibility.Visible.ToString()`).
- `AudioFileOperations` — NAudio playback (play/pause/stop), emergency repeat
  loop, and copying selected audio files into the app's library folder.
- `StartUpManager` — admin elevation check + add app to all-users startup (registry).
- `ShortcutManager` — desktop shortcut via `IWshRuntimeLibrary`.
- `Logger` — thread-safe daily file log (`Logs\Aljaras-<date>.log`); never throws.
  Records fired alarms, startup, auto-backups, and unhandled exceptions.
- `BackupManager` — writes self-contained `.jrsbck` backups (audio + DB) to a
  `Backups\` folder, keeping the newest 14. Driven by the Auto-Backup setting.
- `Extensions.cs` — assorted extension methods.

### Data & persistence (LiteDB)

- Models persisted: `Schedule` (a named group), `Alarm` (a timed announcement
  belonging to a schedule, with per-weekday flags + audio path), `Holiday`
  (date + optional reminder), `UserSettings` (single row, `Id == 1`).
- Collections are named by the `DbTables` enum (`.ToString()`).
- Database file is **per-Windows-user**: `{AppLocation}{UserName}Aljaras.jrsdb`.
- There are committed `*.jrsdb` files (`Aljaras.jrsdb`, `PCAljaras.jrsdb`,
  `RellaxAljaras.jrsdb`) — these are sample/dev databases. Avoid treating them
  as schema sources of truth and don't commit machine-specific DBs.

### Localization

UI strings live in `Languages/en.xml` and `Languages/عربي.xml`, deserialized
into the `AppLanguage` model. `UserSettings.CurrentLang` ("en"/"عربي") selects
which file loads. When adding a user-facing string, add a property to
`AppLanguage` **and** entries in both XML files, then reference it as
`Global.AppLang.YourString` (never hard-code UI text). `MainViewModel`
regenerates `en.xml` from the `AppLanguage` defaults if it's missing.

## Notable features (beyond the basics)

- **Per-alarm volume** — `Alarm.Volume` (0–100); applied via `WaveChannel32` in
  `AudioFileOperations`. `VolumeFraction` treats legacy `0` as full volume.
- **Test Sound** — Monitoring button previews the next alarm's audio on demand.
- **Suspend for today** — `Schedule.SuspendedUntil`; the scheduler skips a
  schedule until it auto-resumes (toggle + pause marker in the schedule list).
- **Date-range / term scheduling** — `Schedule.UseDateRange` + `StartDate`/`EndDate`;
  the schedule only runs inside the range.
- **Auto-backup** + **file logging** — see `BackupManager` / `Logger` above.
- New persisted fields were added in a back-compatible way (legacy LiteDB rows
  deserialize to sensible defaults). When adding model fields, do the same.

## Conventions to follow

- **Match the existing MVVM style**: `[ObservableProperty]` fields and
  `[RelayCommand]` methods, not hand-written `INotifyPropertyChanged`.
- **No hard-coded UI text** — route through `AppLang`.
- **No hard-coded paths** — build paths from `GlobalVariables.AppLocation`.
- **All persistence via `GlobalVariables.db`** and the `DbTables` collection names.
- Visibility/enum values bound in XAML are passed as **strings** (`.ToString()`),
  matching the rest of the code.
- Regions (`#region Observable Properties`, `#region RelayCommands`,
  `#region Functions`) are used throughout — keep them for consistency.
- Code comments and UI are bilingual; Arabic filenames/strings are intentional —
  preserve their encoding (files are UTF-8; `.gitattributes` governs handling).

## Building & running

Requires **Windows** + .NET 10 SDK with the WPF workload (Visual Studio 2022 17.2+ or `dotnet`):

```powershell
dotnet build Aljaras.sln -c Debug
dotnet run --project Aljaras/Aljaras.csproj
```

- The assembly version auto-increments on build (`<IncrementVersionOnBuild>1.yyyy.Mdd.Hmm</IncrementVersionOnBuild>`).
- The csproj copies a large set of `Audio/*.mp3`, `Guide/*.PNG`, and
  `Languages/*.xml` assets to the output directory (`CopyToOutputDirectory=Always`).
  When adding a new bundled audio file, add a matching `<None Update>` entry.
- `App.OnStartup` enforces a **single running instance** via a named `Mutex`
  and installs unhandled-exception dialogs for both UI and non-UI threads.
- There is **no test project, linter config, or CI workflow** in this repo.
  "Verifying" a change means building/running on Windows and exercising the UI.

## Git workflow

- Active development branch for this work: **`claude/claude-md-docs-2561qf`**.
  Develop, commit, and push there; do **not** push to `master` without explicit permission.
- Only open a pull request when explicitly asked.
- Push with `git push -u origin <branch>`, retrying with backoff on network errors.

---

## شرح تفصيلي بالعربية (Deep dive)

### دورة حياة التطبيق عند التشغيل
- `App.OnStartup`: يمنع تشغيل أكثر من نسخة عبر `Mutex` مُسمّى — وعند وجود نسخة أخرى
  فإن **النسخة الجديدة تقتل القديمة وتكمل** (الأحدث يفوز)، ثم يبني أيقونة علبة النظام.
- ثم يُحمَّل `MainWindow` ومعه `MainViewModel`، الذي يضبط `CurrentView` على
  `MonitoringViewModel` ويستدعي `GlobalViewModel.Instance` فيُشغّل الـ Singleton.
- توجد معالجات استثناءات عامة (رسالة "هل تريد المتابعة؟") لكنها تبدو **غير مربوطة فعليًا**
  بالحدث — أي قد تكون كودًا ميتًا. تحقّق قبل الاعتماد عليها.

### محرك الجدولة (قلب التطبيق)
- باني `GlobalViewModel` يُطلق حلقة خلفية دائمة (`Task.Run`) كل ١٠٠٠ مللي ثانية.
- **مطابقة بدقة الثانية** (`TrimMilliseconds`) → إذا تأخّر التنفيذ يُفوَّت الجرس بصمت.
- `LoadMonitoringAlarmCollectionData()` يقرأ الإجازات (+ تذكيراتها كمنبهات وهمية) ثم
  الجداول النشطة ومنبهاتها، ويفلتر كل منبه حسب **يوم الأسبوع** عبر Reflection مطابقًا
  أعلام `Sun`..`Sat`.
- `NextAlarm()` يجد أول منبه قادم ويضبط العدّ التنازلي؛ وعند انتهاء اليوم **يرحّل كل
  المنبهات يومًا** ويعيد الحساب. مؤقّت `DispatcherTimer` بـ ٤٠ مللي ثانية يحرّك العرض.

### النظام الصوتي (NAudio)
- التشغيل عبر `DirectSoundOut` + `BlockAlignReductionStream` (تفرّع mp3/wav).
- **نمط المكتبة**: `MoveAudioFileToLibrary` ينسخ الملف إلى `Audio\<اسم المستخدم>\` ويخزّن
  مسارًا **نسبيًا** في قاعدة البيانات (يجعل النُّسخ الاحتياطية قابلة للنقل).
- **وضع الطوارئ** يكرّر صوت الطوارئ ويُسكت الأجراس المجدولة؛ و**البثّ المباشر** يمرّر
  الميكروفون إلى السماعات (نظام نداء PA).

### نمط CRUD الموحّد (للكيانات الجديدة)
1. `Load…CollectionData()` يملأ القوائم ويضبط حالة "فارغ" (`IsNO…MessageVisible`).
2. الحفظ: إذا المفتاح `> 0` → `Update`، وإلا مفتاح `DateTime.Now.Ticks` جديد → `Insert`.
3. كل تعديل ينتهي بـ `Global.LoadMonitoringAlarmCollectionData()` + رسالة
   `Global.NewNotificationMessage(...)`.

### التفعيل/الترخيص
- أُزيل منطق التفعيل بالكامل — التطبيق الآن **مجاني (Freeware)** بلا أي قيود ترخيص،
  وحُذف مشروع `AljarasActivation` ومولّد المفاتيح وشاشة التفعيل.

### ⚠️ نقاط حذر / ديون تقنية (مرشّحة للإصلاح)
1. **`using (GlobalVariables.db)` على كائن Singleton مشترك** يتخلّص منه (Dispose) بعد كل عملية —
   المشتبه الأول بمشاكل الانهيار في تاريخ الـ commits. افحصه قبل أي تعديل على التخزين.
2. **المطابقة بدقة الثانية** قد تُفوّت جرسًا عند تأخّر التنفيذ.
3. **مطابقة المفاتيح الأجنبية نصّيًا** عبر `.ToString().Contains(...)` مطابقة جزئية لا تساوي تام.
4. حلقة `while(isLoading)` في `LoadMonitoringAlarmCollectionData` تخرج دائمًا من أول تكرار — بلا فائدة.
5. خلط `System.Windows.Forms` و `System.Windows` — انتبه للاسم المستعار `MessageBox`.
