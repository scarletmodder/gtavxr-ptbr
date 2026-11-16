**English** | [Português](architecture.pt-BR.md)

# Architecture

How GTAVXR PT-BR changes the GTAVXR Suite, and why the in-game menus are left alone.

## The pieces

```mermaid
flowchart LR
    setup["Installer<br/>(Inno Setup)"] -->|check, apply, restore| tool["GtavxrPtbr.exe<br/>(.NET Framework 4.8)"]
    tool -->|reads| tr["translations/<br/>suite + content-catalog"]
    tool -->|rewrites the Spanish column| suite["Your GTAVXR.exe"]
    tool -->|keeps| orig["GTAVXR.exe.pt-BR.original"]
    tool -->|"language: es"| settings["Suite settings.json"]
    tool -->|undo data| state["%LOCALAPPDATA%\GTAVXR-PTBR\state.json"]
```

- **`installer/gtavxr-ptbr.iss`**: the Inno Setup wizard. It never ships a file of GTAVXR. It finds the Suite, asks which copy to translate, runs `check` from its temporary folder before anything is copied, runs `apply` after copying the tool, and runs `restore` when uninstalled.
- **`src/GtavxrPtbr`**: the command-line tool. `SuitePatcher` does the rewrite with [Mono.Cecil](https://github.com/jbevain/cecil); `Program` handles the files, the backup, the state and the Suite settings.
- **`translations/`**: four JSON files of `{id, en, pt}` rows. Two are applied by the installer; two are for the in-game menus.

## What the Suite looks like inside

The Suite (`GTAVXR.exe`) is a .NET Framework WPF program. Its texts live in three places:

1. **`GtavxrSuite.L`**, a static table `key → {spanish, english}` built in the class constructor. The compiled code for each row is always the same: `ldstr key`, `ldc.i4.2`, `newarr string`, a local store and load, then `ldc.i4.0`, `ldstr spanish`, `stelem.ref`, and the same for index 1 with the English text. The Suite shows column 1 when `settings.json` says `"language": "en"` and column 0 for anything else.
2. **The language button**, a literal `"Español"` in the settings view.
3. **`GtavxrSuite.content-catalog.json`**, an embedded resource with the names and descriptions of the content mods, as `{"es": ..., "en": ...}` pairs.

## The rewrite

GTAVXR PT-BR replaces the **Spanish** column with Portuguese and leaves English untouched:

- In the constructor of `GtavxrSuite.L`, for each `ldstr key; ldc.i4.2; newarr` it looks a few instructions ahead for `ldc.i4.0` followed by `ldstr`, and replaces that operand with the Portuguese text of `key`. Keys without a translation keep their Spanish text and are counted as missing.
- The `"Español"` literal becomes `"Português"`.
- In the catalog resource, a regular expression replaces each `"es"` value whose `"en"` pair has a translation, keeping the rest of the file byte for byte.
- An embedded resource, `GtavxrPtbr.Marker` (it holds the tool name and version), marks the file as translated, so a second run stops with `ALREADY_PATCHED` and `restore` knows the file is the translated one.

Every other resource, including the embedded GTAVXR distribution bundle, is written back unchanged.

So the Suite keeps working as designed: `"en"` shows English, anything else shows the column that now holds Portuguese. That is why `--set-language` writes `"language": "es"`.

## Safety

`apply` works in this order. Up to step 5 a failure leaves the original untouched; if the swap itself fails, the kept original is copied back so `GTAVXR.exe` never goes missing:

1. refuse while the Suite runs from that file (exit 5);
2. read the file and count what is covered; stop below 80% of the table (exit 3) or if already translated (exit 4);
3. write the translated copy to `GTAVXR.exe.pt-BR.tmp` and check that it loads and carries the marker;
4. copy the original to `GTAVXR.exe.pt-BR.original` and compare its SHA-256 with the original;
5. swap the files with `File.Replace`;
6. write `state.json` with both hashes and the paths, right after the swap;
7. only then switch the Suite language (a failure there is a warning) and record the language it had before.

Only one translated copy is tracked: translating a second `GTAVXR.exe` while another is still translated stops with exit 8. A newer GTAVXR PT-BR run on a copy translated by an older one translates again from the kept original, so the texts are refreshed.

`restore` puts the original back only if the file is still the translated copy and the backup still has the recorded hash. After a GTAVXR update in place it leaves the new file as it is; if the file was moved or deleted it keeps the original where it is and says so. It switches the language back only if it is still `"es"`, so a change the player made later is kept.

## Why the in-game menus are not patched

The VR menu (`vr_format_text.h`, 308 texts) and the Trainer (`localization.h`, 776 texts) are compiled into GTAVXR's native runtime DLLs. Before every launch the Suite compares the SHA-256 of each of those files with its own manifest (`versions/gtavxr-1-0-2.json`) and refuses to start with `INSTALL_RUNTIME_CHANGED` when one differs. Patching them would break Play.

Their translation is in `translations/vr-menu.pt-BR.json` and `translations/trainer.pt-BR.json`, with the same ids as the GTAVXR source, and was offered to the GTAVXR project to be built in ([pull request](https://github.com/LVL-VR/GTAVXR/pull/33)), as a third language next to English and Spanish.

## Side effect: content mods rebuilt once

The hash of the catalog resource is part of the key the Suite uses to decide whether a content mod must be prepared again. After translating, the first Play rebuilds the content mods in use. This is listed in the known limitations.

## Tests and pipeline

- `tests/GtavxrPtbr.Tests/PatcherTests.cs` builds a small stand-in Suite with Mono.Cecil (same IL pattern, the `"Español"` literal and a catalog resource) and checks the rewrite, the marker, refusals and the catalog escaping.
- `tests/GtavxrPtbr.Tests/TranslationFileTests.cs` checks every translation file: same placeholders as English, no empty row, no long dash, unique ids.
- `.github/workflows/ci.yml` runs the tests on every push. `.github/workflows/release.yml` builds the installer, installs and uninstalls it on a clean Windows runner, and publishes a release from `main` when the version in `GtavxrPtbr.csproj` has no tag yet.

The real `GTAVXR.exe` is not in the repository (it belongs to LVL-VR). Each release is also checked by hand against the real GTAVXR 1.0.2: `check` reports 422/422 Suite texts and 129/129 catalog texts, `apply` and `restore` round-trip to the original SHA-256, and the translated Suite is opened and its pages read.
