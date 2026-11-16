**English** | [Português](troubleshooting.pt-BR.md)

# Troubleshooting

Every message of the installer, why it appears and what to do. What the last run did is written to `%LOCALAPPDATA%\GTAVXR-PTBR\last-run.log`; include that file when you [ask for help](https://github.com/scarletmodder/gtavxr-ptbr/issues). It has no personal data other than the path of your `GTAVXR.exe`.

## "Suite do GTAVXR (GTAVXR.exe): not found in the usual places"

The installer looked in your Downloads folder, your Desktop and the folder where the Suite installs itself. Your copy is somewhere else: choose it on the next page with **Browse**. If you do not have GTAVXR yet, **Open the GTAVXR page** opens its official download page.

## "This file is not the GTAVXR Suite"

The chosen file is not the program with GTAVXR's Play button. Choose the `GTAVXR.exe` you open to play. A copy that is not the Suite (another mod, an installer, a shortcut) is rejected before anything changes.

## "This GTAVXR version has texts this translation does not know yet"

GTAVXR was updated and less than 80% of its texts are known. Nothing was changed. Check the [releases](https://github.com/scarletmodder/gtavxr-ptbr/releases) for a newer GTAVXR PT-BR, or [open an issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) with the GTAVXR version.

## "This GTAVXR.exe is already translated"

It was translated before. Continuing reinstalls the tool; if this GTAVXR PT-BR is newer than the one that translated it, the texts are updated from the kept original.

## "Another GTAVXR.exe is already translated on this computer"

Only one copy is translated at a time, so that uninstalling can always put it back. Uninstall GTAVXR PT-BR (the other copy gets its original back), then run the installer again for this copy.

## "Close GTAVXR (the Suite window) and click OK"

The file cannot be changed while it is running. Close the GTAVXR window and click **OK**. **Cancel** leaves GTAVXR as it is: when installing, the tool is installed but nothing is translated (run the installer again later); when uninstalling, nothing is removed.

## "GTAVXR was not translated, and the original was left as it was"

Something failed while writing the translated copy: usually a file that is locked, or a folder without permission to write (for example a copy inside `C:\Program Files`). The original is unchanged. The log says which step failed. Moving `GTAVXR.exe` to a folder of your own, such as Downloads, solves the permission case.

## "The original GTAVXR.exe could not be put back"

Shown by the uninstaller. The original is still kept next to your copy as `GTAVXR.exe.pt-BR.original`: close GTAVXR, delete `GTAVXR.exe` and rename the kept file to `GTAVXR.exe`. Or download GTAVXR again from its official page.

## The Suite is still in English

The option to switch the language was unchecked, or the Suite was set to English later. In the Suite, open **Settings** and press **Português**.

## Some texts are in Spanish

They are texts this version of the translation does not know yet, usually because GTAVXR was updated. [Open an issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) with a screenshot.

## The menus inside the game are in English or Spanish

Expected for now: the in-game VR menu and the Trainer cannot be translated from outside without stopping GTAVXR from starting, so their translation was offered to the GTAVXR authors ([pull request](https://github.com/LVL-VR/GTAVXR/pull/33)). See [Known limitations](../README.md#%EF%B8%8F-known-limitations).

## After translating, Play takes longer once

GTAVXR rebuilt the content mods you use (World of Variety and others), because their descriptions changed language. It happens once.

## Windows says the installer may be unsafe

The installer is not digitally signed. Choose **More info** and **Run anyway**. You can compare the file with the `SHA256SUMS.txt` of the release:

```bash
certutil -hashfile GTAVXR-PTBR-Setup-v1.0.0.exe SHA256
```
