# Aljaras (الجرس) — School Bell System

**Aljaras** is a Windows desktop **school bell / alarm system**. It plays
scheduled audio announcements (class start/end, breaks, exam periods, the
morning assembly, emergencies) through the PC's speakers, runs quietly in the
system tray, and has a fully **bilingual UI — Arabic and English**.

> الجرس: برنامج جرس مدرسي لويندوز يشغّل تنبيهات صوتية مجدولة (بداية/نهاية الحصص،
> الفسحة، الاختبارات، طابور الصباح، الطوارئ) ويعمل في الخلفية، بواجهة عربية/إنجليزية.

Authored by **Mustafa Al-Klieb**. Free to use (freeware — no activation/licensing).

---

## ✨ Features

- **Schedules & alarms** — group timed announcements into schedules, with
  per-weekday flags (Sun–Sat) and a custom audio file per alarm.
- **Per-alarm volume** — set each announcement's loudness (0–100%).
- **Holidays & reminders** — mark holidays and optional reminder announcements.
- **Suspend a schedule for today** — skip a schedule's bells for the day; it
  auto-resumes tomorrow.
- **Date-range (term) scheduling** — make a schedule active only between a
  start and end date (e.g. one semester).
- **Live intercom** — speak through the PC mic to the speakers in real time.
- **Emergency broadcast** — one click to loop an emergency announcement.
- **Test Sound** — preview the next alarm's audio without waiting.
- **Automatic backups** — daily self-contained `.jrsbck` backups (audio + data).
- **Import / export** — move your setup between machines as a single file.
- **File logging** — every fired alarm and error is logged for troubleshooting.
- **Runs at startup** and lives in the **system tray**.

---

## 🖥️ Requirements

- **Windows 10 / 11** (this is a WPF app — Windows only).
- To **run a published build**: nothing extra if it's *self-contained* (see below).
- To **build from source**: [.NET 10 SDK](https://dotnet.microsoft.com/download)
  with the **WPF / Windows Desktop** workload (Visual Studio 2022 17.2+ or the
  `dotnet` CLI).

---

## 🚀 Getting started (run from source)

```powershell
git clone https://github.com/AbuDergham/Aljaras.git
cd Aljaras
dotnet run --project Aljaras/Aljaras.csproj
```

Or open `Aljaras.sln` in Visual Studio 2022 and press **F5**.

---

## 📦 Building a real, distributable `.exe`

You usually want a **self-contained single file** so the school PCs don't need
.NET installed.

```powershell
dotnet publish Aljaras/Aljaras.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output lands in:

```
Aljaras/bin/Release/net10.0-windows/win-x64/publish/
```

That `publish` folder contains **`Aljaras.exe`** plus the `Audio`, `Languages`,
and `Guide` asset folders — **copy the whole folder** to the target PC and run
`Aljaras.exe`. (The bundled `.mp3` announcements and language files live next to
the exe; they are not embedded inside it.)

**Options:**

| Goal | Command tweak |
|---|---|
| Smaller exe, but PC must have .NET 10 installed | drop `--self-contained true` (framework-dependent) |
| 32-bit machines | use `-r win-x86` |
| Keep assets beside the exe (recommended) | leave as-is |

> 💡 First launch asks to **run as administrator** so it can register
> *start with Windows* and create a desktop shortcut. Approve it once.

You can also publish from Visual Studio: **right-click the Aljaras project →
Publish → Folder**, then choose *win-x64*, *self-contained*, *single file*.

---

## 📖 How to use

1. **Add a schedule** — open **Alarm**, type a title, click **Save**.
2. **Add alarms** — select the schedule, set title, time (hour / minute /
   AM-PM), pick the days, choose an audio file, set the **volume**, click **Save**.
   Use **Test/Play** to hear it.
3. **Monitoring (home)** — shows the system clock, the next alarm, the
   countdown, and the upcoming list. **Test Sound** previews the next alarm.
4. **Holidays** — add dates to skip and optional reminder announcements.
5. **Suspend / term limits** — use the ⏸ button on a schedule to pause it for
   the day, or enable **Date Range** to limit it to a term.
6. **Settings** — language (Arabic/English), emergency clip, *Start with
   Windows*, *Shutdown on close*, **Auto Backup**, and Import/Export/Delete data.
7. **Intercom & Emergency** — live mic broadcast and the emergency loop are on
   the Monitoring screen.

The app keeps running in the **system tray** — double-click the tray icon to
reopen the window, right-click for **Open / Exit**.

---

## 🗂️ Where your data lives

Everything is stored **next to the exe**, per Windows user:

| What | Location |
|---|---|
| Database (schedules/alarms/holidays/settings) | `<UserName>Aljaras.jrsdb` |
| Your imported audio | `Audio\<UserName>\` |
| Automatic backups (newest 14 kept) | `Backups\` |
| Logs | `Logs\Aljaras-<date>.log` |

**Backups** are `.jrsbck` files (a zip of your audio + database). Restore one via
**Settings → Import**. Moving to a new PC? Export on the old one, import on the new.

---

## 🌐 Languages

The UI ships in **English** (`Languages/en.xml`) and **Arabic**
(`Languages/عربي.xml`) and mirrors right-to-left for Arabic. Switch under
**Settings → Language**. To add another language, copy an XML file, translate the
values, and it appears automatically in the language list.

---

## 🛠️ Tech stack

.NET 10 · WPF · MVVM ([CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)) ·
[LiteDB](https://www.litedb.org/) (embedded database) · [NAudio](https://github.com/naudio/NAudio) (audio).

See [`CLAUDE.md`](CLAUDE.md) for architecture and contributor notes.

---

## 📄 License

Freeware — free to use and distribute. © Mustafa Al-Klieb.
