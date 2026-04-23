# Part 1 — Project Setup

> **Prerequisites:** you've finished Part 0 (design on paper). You have your field table, your main menu, and your sample file line. If any of that is missing, go back — Visual Studio isn't going to help you design.

## What this part builds

Nothing visible yet. By the end of this part you will have:

1. A new Console App project in Visual Studio, targeting **.NET Framework 4.7.2**.
2. An empty `Main` that compiles and runs.
3. A basic understanding of the folder layout Visual Studio creates for you.
4. Confidence that you can build the project in both **Debug** and **Release** mode.

That's the whole goal. It sounds small, but "I can create an empty project and run it" is the single most common place beginners get stuck. Every later part assumes this works. If your build is broken here, nothing you do in Part 2 will help you.

> **Rule of thumb:** don't try to write real code until an empty program already compiles and runs. You want a known-good starting point to fall back to.

---

## Part 1a — The framework choice (read before you click anything)

There are two families of .NET you'll see in Visual Studio:

- **.NET Framework** — the older Windows-only stack. Versions like 4.6, 4.7.2, 4.8.
- **.NET** (sometimes called .NET Core / .NET 5 / 6 / 7 / 8) — the newer cross-platform stack.

They look similar on screen. They are **not interchangeable** for this project.

**You must target .NET Framework 4.7.2.**

### Why

- The competition deliverable is a **single standalone `.exe`** that judges run by double-clicking. .NET Framework apps compile to exactly that on any modern Windows machine.
- Modern .NET (5/6/7/8) console apps *can* be packaged as a single file, but doing so is extra publish-configuration work you don't need to learn to win this comp.
- The reference code and every later part of this workbook is written against 4.7.2. Templates, APIs, and the `.csproj` format all differ between the two families.

If you accidentally pick "Console App" without checking — you'll often get the new .NET, not .NET Framework. Read the project template name carefully. You want the one that says **Console App (.NET Framework)**.

> **If you don't see `(.NET Framework)` in the template list:** your Visual Studio install is missing the .NET desktop development workload. Open **Visual Studio Installer**, click **Modify** on your VS install, and check **".NET desktop development"** on the **Workloads** tab. This is a one-time fix.

---

## Part 1b — Create the project

Open Visual Studio.

1. On the start window, click **Create a new project**.
2. In the template search box, type `console framework`.
3. In the results, pick the template **Console App (.NET Framework)** — the one that says *C#* and *(.NET Framework)*. Be sure it is **not** plain "Console App" without the "Framework" label.
4. Click **Next**.

On the next screen:

- **Project name:** `SkillsOntarioSampleProject2026`
  (Match this exactly. The namespace in every code sample in this workbook is `SkillsOntarioSampleProject2026`. If you name the project differently, your `namespace` lines will disagree with the samples and the code won't find itself.)
- **Location:** somewhere you can find it again. `C:\Users\<you>\source\repos\` is the Visual Studio default and is fine.
- **Solution name:** leave as the auto-filled copy of the project name.
- **Framework:** pick **.NET Framework 4.7.2** from the dropdown. **This is the step you are most likely to get wrong.** If 4.7.2 isn't listed, pick the closest 4.7.x or 4.8. If only .NET Core / .NET 6+ show up, you picked the wrong template — cancel and start over.

Click **Create**. Visual Studio builds the project and drops you into `Program.cs`.

### What you should see

A single source file, `Program.cs`, that looks roughly like this:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsOntarioSampleProject2026
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
```

An empty `Main`. That's exactly right.

> **The `using` lines at the top.** These import parts of the standard library so you can use common types without typing `System.Console.WriteLine` every time. You can ignore them for now — Visual Studio adds and removes them automatically as you use types. Don't delete them manually "to clean up."

---

## Part 1c — Make sure it actually runs

Before you change a single character, verify that the empty project compiles and runs. You want a known-good starting point.

### Run with F5

Press **F5**. This is the most common Visual Studio shortcut you'll use — it means **Build and run with the debugger attached**.

What happens:

1. Visual Studio compiles your code.
2. A black console window opens.
3. The window closes almost immediately.

That's it. `Main` runs, `Main` does nothing, `Main` returns, and the process exits. The quick flash is correct behaviour for an empty program — there's nothing to pause on.

### If the console window flashed too fast to read anything

That's fine. There was nothing *to* read. You're not broken — your program just finished instantly.

If you want to see a window stay open so you can confirm something ran, add a single line for this one test:

```csharp
static void Main(string[] args)
{
    Console.ReadKey();
}
```

Press F5 again. Now the window stays open until you press any key. **Delete that line again** before moving on — you don't want stray test code sticking around.

### If the build failed

Read the **Error List** panel at the bottom of Visual Studio. The most common causes:

- You named the project something other than `SkillsOntarioSampleProject2026` **and** later pasted sample code that uses a different namespace.
- You picked the wrong template and are now on modern .NET, not .NET Framework. (See Part 1a.)

Red squigglies, missing references, or "the project isn't loaded" usually mean one of the above. Don't fight them — if setup is genuinely wrong, it's faster to delete the solution folder and start Part 1b over than to try to rescue a broken project.

---

## Part 1d — A tour of the folder Visual Studio made for you

Minimise Visual Studio and open File Explorer in the folder where you saved the solution. You'll see something like:

```
SkillsOntarioSampleProject2026\
    SkillsOntarioSampleProject2026.sln
    SkillsOntarioSampleProject2026\
        SkillsOntarioSampleProject2026.csproj
        Program.cs
        App.config
        Properties\
        bin\
        obj\
```

You don't need to understand all of this. The bits that matter:

### `.sln` — the Solution file

The `.sln` file sits at the top. A "solution" is Visual Studio's name for a collection of one or more **projects**. You only have one project here, but the structure is the same as a big multi-project app. **Open this file** any time you want to re-open the project later — double-clicking it launches Visual Studio with everything loaded.

### `.csproj` — the Project file

Inside the inner folder, `SkillsOntarioSampleProject2026.csproj` describes **your project**: what framework it targets, what source files belong to it, what external libraries it references.

You'll open this file exactly once, in a moment, just to confirm the framework target. After that, **leave it alone** — Visual Studio edits it for you whenever you add or remove files. Editing it by hand when you don't have to is a good way to break your build.

### `Program.cs`

Your code. This is where `Main` lives. Every part of this workbook after Part 1 adds or modifies a `.cs` file next to this one.

### `bin\` and `obj\` — the build output

Visual Studio creates these automatically when you build. You never put your own files in them.

- **`bin\Debug\`** — where the **Debug** build of your `.exe` ends up. This is what runs when you press F5.
- **`bin\Release\`** — where the **Release** build ends up. This is what you'll ship to judges. Doesn't exist yet — you'll create it in Part 4.
- **`obj\`** — scratch space the compiler uses. You will never need to look inside it.

Both of these folders are **safe to delete** if something feels wrong. Visual Studio regenerates them on the next build. Right-click the solution in **Solution Explorer** → **Clean Solution** does the same thing.

> **Rule of thumb:** if a build is misbehaving in a way you can't explain, delete `bin\` and `obj\`, then build again. Ninety percent of "weird build" problems go away.

### `Properties\AssemblyInfo.cs`

Boilerplate about the assembly (name, version, company). You'll never open it. Leave it alone.

### `App.config`

An XML config file. You don't need it for this project; ignore it.

---

## Part 1e — Confirm the framework target

Open the `.csproj` file once, just to see the framework line with your own eyes.

1. In **Solution Explorer** (right side of Visual Studio), double-click the project name.
   (If that opens an editor for project *properties* in a tab, that's fine too — look for the **Target framework** dropdown there. In some Visual Studio versions, you have to right-click the project → **Properties** → **Application** to see it.)
2. Look for this line in the file's XML, or this value in the Properties dialog:

```xml
<TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
```

If it says `v4.7.2`, you're fine. If it says `v4.6.1`, `v4.8`, or similar 4.x — also fine for the purposes of this workbook, but prefer `v4.7.2` to match the reference solution.

If it says anything involving `<TargetFramework>net6.0</TargetFramework>` (no *"Version"* in the name, and a number like `net5.0` / `net6.0` / `net7.0`) — **stop**. That's modern .NET, not .NET Framework. Close Visual Studio, delete the solution folder, and redo Part 1b with the correct template. Do not try to "upgrade" or "migrate" — just start clean.

> **Why the paranoia about the framework:** later you'll copy `.exe` files to a clean folder and confirm they run standalone. Modern .NET apps compiled the default way don't run standalone without extra publish steps. .NET Framework apps do, because the framework is already installed on every modern Windows machine. Getting this right now saves a deployment headache in Part 4.

---

## Part 1f — Debug vs Release (quick tour)

Look at the toolbar at the top of Visual Studio. Near the play button, you'll see a dropdown that says **Debug**. That's the **configuration selector**. Click it — you'll see two options:

- **Debug** — the configuration you develop in. Slower, bigger `.exe`, includes debug symbols (the data the debugger uses to show line numbers, step through code, etc.).
- **Release** — the configuration you ship. The compiler applies optimizations, strips debug symbols, and produces a smaller, faster `.exe`.

For most of this workbook you'll stay on **Debug**. You only switch to Release when it's time to produce the `.exe` you hand in — that's covered in Part 4.

### The two build commands worth memorising

- **F5** — Build, then run with the debugger attached. This is what you use almost every time you want to test a change. It rebuilds only what changed, launches the program, and lets you set breakpoints.
- **Ctrl+Shift+B** — Build the whole solution, **without running**. Use this when you just want to confirm the project compiles — you're not ready to run it because, say, you just added a class with no menu wired up yet.

There are other shortcuts (Ctrl+F5 runs without a debugger, F10 / F11 step through code). You'll discover them as you need them. For now, F5 and Ctrl+Shift+B cover 95% of your workflow.

---

## Before moving on, check yourself

- [ ] The project is named **SkillsOntarioSampleProject2026** and the solution opens without errors.
- [ ] The target framework is **.NET Framework 4.7.2** (or another 4.x if 4.7.2 isn't available).
- [ ] Pressing **F5** builds and runs the project. Even if the window flashes past, no red "build failed" dialog appears.
- [ ] Pressing **Ctrl+Shift+B** shows **"Build succeeded"** in the status bar at the bottom of Visual Studio.
- [ ] I can find `Program.cs`, the `.csproj` file, and the `bin\Debug\` folder in File Explorer without looking it up.
- [ ] I know what **Debug** and **Release** mean, even though I'm only using Debug right now.

When every box is checked, your empty project is solid ground. **Now you're ready for Part 2: the Animal class.**
