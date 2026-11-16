using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace GtavxrPtbr
{
    /// <summary>
    /// Command line used by the installer and uninstaller:
    ///   find                       prints the GTAVXR.exe copies it can see, one per line
    ///   check   --exe PATH         reports how much of that Suite this translation covers
    ///   apply   --exe PATH [--set-language]   translates that copy (keeps the original next to it)
    ///   restore                    puts the original GTAVXR.exe back and undoes the language change
    ///   status                     prints the current state
    ///   busy                       exit 5 while GTAVXR is running and something must be undone (used before uninstalling)
    ///   ingame-add --exe PATH --mod ZIP   adds the in-game mod to the GTAVXR library and turns it on in the profiles
    ///   ingame-remove              takes the in-game mod out of the profiles and the library
    /// Exit codes: 0 ok, 1 error, 2 usage, 3 coverage too low, 4 already translated, 5 Suite running,
    /// 6 nothing to restore, 7 not the GTAVXR Suite, 8 another copy is already translated.
    /// </summary>
    internal static class Program
    {
        internal const double MinimumCoverage = 0.80;
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        static int Main(string[] args)
        {
            Console.OutputEncoding = Utf8;
            if (args.Length == 0) return Usage();
            using (var log = OpenLog(args))
            {
                if (log != null)
                {
                    Console.SetOut(new TeeWriter(Console.Out, log));
                    Console.SetError(new TeeWriter(Console.Error, log));
                }
                return Run(args);
            }
        }

        /// <summary>Every run also goes to %LOCALAPPDATA%\GTAVXR-PTBR\last-run.log, which the installer shows on errors.</summary>
        static StreamWriter OpenLog(string[] args)
        {
            try
            {
                Directory.CreateDirectory(StateDir);
                var writer = new StreamWriter(Path.Combine(StateDir, "last-run.log"), false, Utf8) { AutoFlush = true };
                writer.WriteLine("GtavxrPtbr " + Version + ": " + string.Join(" ", args));
                return writer;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                return null;
            }
        }

        static int Run(string[] args)
        {
            try
            {
                switch (args[0])
                {
                    case "find": return Find();
                    case "check": return Check(RequireExe(args));
                    case "apply": return Apply(RequireExe(args), args.Contains("--set-language"));
                    case "restore": return Restore();
                    case "status": return Status();
                    case "busy": return Busy();
                    case "ingame-add": return InGameAdd(RequireExe(args), RequireArg(args, "--mod"));
                    case "ingame-remove": return InGameRemove();
                    case "--version": Console.WriteLine(Version); return 0;
                    default: return Usage();
                }
            }
            catch (PatchException e)
            {
                Console.Error.WriteLine("ERROR " + e.Code + ": " + e.Message);
                return e.Code == "NOT_SUITE" ? 7 : 1;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Console.Error.WriteLine("ERROR IO: " + e.Message);
                return 1;
            }
            catch (Exception e)
            {
                // Last resort: the installer reads the exit code and the log, never a crash dialog.
                Console.Error.WriteLine("ERROR UNEXPECTED: " + e);
                return 1;
            }
        }

        static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        static string ToolDir => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        static string StateDir => Path.Combine(LocalAppData, "GTAVXR-PTBR");
        static string StatePath => Path.Combine(StateDir, "state.json");

        static int Usage()
        {
            Console.Error.WriteLine("Usage: GtavxrPtbr find | check --exe PATH | apply --exe PATH [--set-language] | restore | status | ingame-add --exe PATH --mod ZIP | ingame-remove");
            return 2;
        }

        static string RequireExe(string[] args)
        {
            int i = Array.IndexOf(args, "--exe");
            if (i < 0 || i + 1 >= args.Length) throw new PatchException("USAGE", "Missing --exe PATH.");
            string path = Path.GetFullPath(args[i + 1]);
            if (!File.Exists(path)) throw new PatchException("EXE_MISSING", "File not found: " + path);
            return path;
        }

        static string RequireArg(string[] args, string name)
        {
            int i = Array.IndexOf(args, name);
            if (i < 0 || i + 1 >= args.Length) throw new PatchException("USAGE", "Missing " + name + " PATH.");
            return Path.GetFullPath(args[i + 1]);
        }

        static string InGamePath => Path.Combine(StateDir, "in-game.json");

        internal sealed class InGameState
        {
            public string card { get; set; }
            public List<string> profiles { get; set; }
            public string version { get; set; }
            /// <summary>The card was in the library before the install: the uninstall leaves it there.</summary>
            public bool kept { get; set; }
        }

        /// <summary>The in-game state, or null when there is none or it cannot be read (it is then treated as absent).</summary>
        static InGameState ReadInGameState()
        {
            if (!File.Exists(InGamePath)) return null;
            try { return new JavaScriptSerializer().Deserialize<InGameState>(File.ReadAllText(InGamePath, Utf8)); }
            catch (Exception e) when (e is ArgumentException || e is InvalidOperationException || e is IOException)
            {
                Console.Error.WriteLine("WARNING INGAME_STATE: " + InGamePath + " could not be read (" + e.Message + ")");
                return null;
            }
        }

        static void WriteInGameState(InGameState state)
        {
            Directory.CreateDirectory(StateDir);
            File.WriteAllText(InGamePath, new JavaScriptSerializer().Serialize(state), Utf8);
        }

        /// <summary>GTAVXR itself (any copy of the Suite) or the game: the library must not change under them.</summary>
        static bool GtavxrRunning()
        {
            bool running = false;
            foreach (string name in new[] { "GTAVXR", "GTA5", "GTAVXR_SourceBaseline", "PlayGTAV" })
                foreach (var p in Process.GetProcessesByName(name)) { p.Dispose(); running = true; }
            return running;
        }

        static int InGameAdd(string exe, string modZip)
        {
            if (GtavxrRunning())
            {
                Console.Error.WriteLine("ERROR SUITE_RUNNING: Close GTAVXR and the game before adding the in-game translation.");
                return 5;
            }
            var earlier = ReadInGameState();
            // the new package goes in first: if GTAVXR refuses it, the earlier one stays as it was
            var imported = InGame.Import(exe, modZip);
            bool sameCard = earlier != null && earlier.card == imported.Id;
            var state = new InGameState
            {
                card = imported.Id,
                profiles = new List<string>(),
                version = Version,
                kept = sameCard ? earlier.kept : imported.AlreadyThere,
            };
            WriteInGameState(state);
            // an update keeps the profiles the earlier one was on in (a profile where the player switched it off stays off);
            // a first install turns it on in every profile that is not a test profile
            var only = earlier?.profiles;
            var changed = InGame.Enable(imported.Id, only);
            state.profiles = (sameCard && earlier.profiles != null ? earlier.profiles : new List<string>()).Union(changed).ToList();
            WriteInGameState(state);
            if (earlier != null && !sameCard)
            {
                InGame.Remove(earlier.card, earlier.kept ? earlier.profiles ?? new List<string>() : null, !earlier.kept);
                Console.WriteLine("Earlier in-game translation replaced: " + earlier.card);
            }
            Console.WriteLine("In-game translation added to the GTAVXR library: " + imported.Id + (imported.AlreadyThere ? " (it was already there)" : ""));
            Console.WriteLine("Turned on in " + changed.Count + " profile(s)");
            return 0;
        }

        static int InGameRemove()
        {
            if (!File.Exists(InGamePath)) { Console.WriteLine("No in-game translation to remove."); return 6; }
            if (GtavxrRunning())
            {
                Console.Error.WriteLine("ERROR SUITE_RUNNING: Close GTAVXR and the game before removing the in-game translation.");
                return 5;
            }
            var state = ReadInGameState();
            // a card the player had added before stays in the library, switched off only where the installer switched it on
            if (state != null) InGame.Remove(state.card, state.kept ? state.profiles ?? new List<string>() : null, !state.kept);
            File.Delete(InGamePath);
            Console.WriteLine("In-game translation removed from the GTAVXR library.");
            return 0;
        }

        /// <summary>Where the Suite usually lives: its own install folder, then the usual download places.</summary>
        internal static IEnumerable<string> Candidates()
        {
            string profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var places = new[]
            {
                Path.Combine(LocalAppData, "Programs", "GTAVXR", "GTAVXR.exe"),
                Path.Combine(profile, "Downloads", "GTAVXR.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "GTAVXR.exe"),
            };
            return places.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        static int Find()
        {
            foreach (string path in Candidates()) Console.WriteLine(path);
            return 0;
        }

        static Translations LoadTranslations() => Translations.Load(Path.Combine(ToolDir, "translations"));

        static int Check(string exe)
        {
            var report = SuitePatcher.Inspect(exe, LoadTranslations());
            Print(report);
            if (report.AlreadyPatched) return 4;
            return report.Coverage < MinimumCoverage ? 3 : 0;
        }

        static void Print(PatchReport r)
        {
            Console.WriteLine("suite_texts={0} translated={1} coverage={2:0.0}%", r.SuiteKeysFound, r.SuiteKeysTranslated, r.Coverage * 100);
            Console.WriteLine("catalog_texts={0} translated={1}", r.CatalogTextsFound, r.CatalogTextsTranslated);
            Console.WriteLine("language_button={0}", r.LanguageButtonTranslated ? "yes" : "no");
            Console.WriteLine("already_translated={0}", r.AlreadyPatched ? "yes" : "no");
            if (r.MissingKeys.Count > 0) Console.WriteLine("untranslated_keys=" + string.Join(",", r.MissingKeys.Take(20)));
        }

        static int Apply(string exe, bool setLanguage)
        {
            if (SuiteRunning(exe))
            {
                Console.Error.WriteLine("ERROR SUITE_RUNNING: Close GTAVXR before translating it.");
                return 5;
            }
            var translations = LoadTranslations();
            string temp = exe + ".pt-BR.tmp";
            string backup = exe + ".pt-BR.original";
            var earlier = ReadState();
            bool sameExe = earlier != null && string.Equals(earlier.exe, exe, StringComparison.OrdinalIgnoreCase);

            // One translated copy at a time, so that uninstalling can always undo it.
            if (earlier != null && !sameExe && File.Exists(earlier.exe) && SuitePatcher.IsPatched(earlier.exe))
            {
                Console.Error.WriteLine("ERROR OTHER_COPY: Another GTAVXR.exe is already translated: " + earlier.exe +
                    ". Uninstall GTAVXR PT-BR first to put it back, then translate this one.");
                return 8;
            }

            // The source of the translation: the file itself, or the kept original when this copy was
            // translated by an older version of this tool (so a new version refreshes the texts).
            string source = exe;
            var report = SuitePatcher.Inspect(exe, translations);
            if (report.AlreadyPatched)
            {
                bool refreshable = sameExe && earlier.version != Version && File.Exists(backup) && Sha256(backup) == earlier.original_sha256;
                if (!refreshable) { Print(report); Console.WriteLine("Already translated: nothing to do."); return 4; }
                source = backup;
                report = SuitePatcher.Inspect(source, translations);
                Console.WriteLine("Refreshing the texts of version " + earlier.version + " from the kept original.");
            }
            if (report.Coverage < MinimumCoverage)
            {
                Print(report);
                Console.Error.WriteLine("ERROR COVERAGE: This GTAVXR version is not supported by this translation yet.");
                return 3;
            }

            if (File.Exists(temp)) File.Delete(temp);
            try
            {
                SuitePatcher.Patch(source, temp, translations, Version);
                if (!SuitePatcher.IsPatched(temp)) throw new PatchException("VERIFY", "The translated copy failed its own check.");

                string originalHash = Sha256(source);
                if (source == exe)
                {
                    if (File.Exists(backup) && Sha256(backup) != originalHash) File.Delete(backup);
                    if (!File.Exists(backup)) File.Copy(exe, backup);
                    if (Sha256(backup) != originalHash) throw new PatchException("BACKUP", "Could not keep a verified copy of the original.");
                }
                try
                {
                    File.Replace(temp, exe, null);
                }
                catch
                {
                    // File.Replace can fail after moving the old file away: never leave the player without GTAVXR.exe.
                    if (!File.Exists(exe)) File.Copy(backup, exe);
                    throw;
                }

                // The state goes to disk right after the swap, so the change can always be undone.
                var state = new State
                {
                    exe = exe, backup = backup, original_sha256 = originalHash, patched_sha256 = Sha256(exe), version = Version,
                    previous_language = sameExe ? earlier.previous_language : null,
                };
                WriteState(state);
                Print(report);
                Console.WriteLine("Translated: " + exe);

                if (setLanguage) SwitchLanguage(state);
                return 0;
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
        }

        /// <summary>The language is a convenience: a failure here is a warning, the translation stays.</summary>
        static void SwitchLanguage(State state)
        {
            try
            {
                string before = SuiteSettings.SetLanguage("es");
                if (state.previous_language == null && before != null)
                {
                    state.previous_language = before;
                    WriteState(state);
                }
                Console.WriteLine("Suite language: Portuguese");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("WARNING LANGUAGE: The Suite language was not changed (" + e.Message + "). Choose Português in its settings.");
            }
        }

        static int Busy()
        {
            var state = ReadState();
            if (state != null && SuiteRunning(state.exe))
            {
                Console.WriteLine("GTAVXR is running: " + state.exe);
                return 5;
            }
            // the in-game translation is taken out of the GTAVXR library, which must not change while it runs
            if (File.Exists(InGamePath) && GtavxrRunning())
            {
                Console.WriteLine("GTAVXR or the game is running");
                return 5;
            }
            return 0;
        }

        static int Restore()
        {
            var state = ReadState();
            if (state == null) { Console.WriteLine("Nothing to restore."); return 6; }
            if (SuiteRunning(state.exe))
            {
                Console.Error.WriteLine("ERROR SUITE_RUNNING: Close GTAVXR before restoring it.");
                return 5;
            }
            if (!File.Exists(state.exe))
            {
                // Moved or deleted: the kept original is the only copy left, so it stays.
                Console.WriteLine("GTAVXR.exe is not at " + state.exe + " anymore. The original is kept at " + state.backup + ".");
            }
            else if (SuitePatcher.IsPatched(state.exe))
            {
                if (!File.Exists(state.backup) || Sha256(state.backup) != state.original_sha256)
                    throw new PatchException("BACKUP", "The kept original is missing or does not match; nothing was changed.");
                File.Copy(state.backup, state.exe, true);
                File.Delete(state.backup);
                Console.WriteLine("Original restored: " + state.exe);
            }
            else
            {
                // GTAVXR was updated in place: the new file is the player's, the old original is not needed.
                if (File.Exists(state.backup)) File.Delete(state.backup);
                Console.WriteLine("GTAVXR.exe is not the translated copy anymore (updated); left as it is.");
            }
            try
            {
                if (state.previous_language != null) SuiteSettings.RestoreLanguage("es", state.previous_language);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("WARNING LANGUAGE: The Suite language was not changed back (" + e.Message + ").");
            }
            File.Delete(StatePath);
            return 0;
        }

        static int Status()
        {
            var state = ReadState();
            var ig = ReadInGameState();
            if (ig != null)
            {
                Console.WriteLine("in_game={0} card={1}", InGame.Present(ig.card) ? "added" : "removed_in_suite", ig.card);
            }
            if (state == null) { Console.WriteLine("state=not_installed"); return 0; }
            bool patched = File.Exists(state.exe) && SuitePatcher.IsPatched(state.exe);
            Console.WriteLine("state={0}", patched ? "translated" : "original_or_updated");
            Console.WriteLine("exe=" + state.exe);
            return 0;
        }

        /// <summary>True when a process is running from that exact file.</summary>
        static bool SuiteRunning(string exe)
        {
            foreach (var p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe)))
            {
                try
                {
                    if (string.Equals(p.MainModule.FileName, exe, StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch (Exception e) when (e is System.ComponentModel.Win32Exception || e is InvalidOperationException)
                {
                    return true; // cannot tell: be safe
                }
                finally { p.Dispose(); }
            }
            return false;
        }

        internal static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        internal sealed class State
        {
            public string exe { get; set; }
            public string backup { get; set; }
            public string original_sha256 { get; set; }
            public string patched_sha256 { get; set; }
            public string previous_language { get; set; }
            public string version { get; set; }
        }

        static void WriteState(State state)
        {
            Directory.CreateDirectory(StateDir);
            File.WriteAllText(StatePath, new JavaScriptSerializer().Serialize(state), Utf8);
        }

        static State ReadState()
        {
            if (!File.Exists(StatePath)) return null;
            try
            {
                return new JavaScriptSerializer().Deserialize<State>(File.ReadAllText(StatePath, Utf8));
            }
            catch (Exception e) when (e is ArgumentException || e is InvalidOperationException)
            {
                throw new PatchException("STATE", "The state file is damaged: " + StatePath + ". The original, if any, is next to GTAVXR.exe as GTAVXR.exe.pt-BR.original.");
            }
        }
    }

    /// <summary>The Suite's own settings.json: only its "language" value is touched.</summary>
    internal static class SuiteSettings
    {
        static readonly Regex LanguageValue = new Regex("(\"language\"\\s*:\\s*\")([a-z]{2})(\")", RegexOptions.CultureInvariant);

        internal static string DataRoot()
        {
            string fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GTAVXR", "Suite");
            string pointer = Path.Combine(Path.GetDirectoryName(fallback), "suite-location.json");
            if (!File.Exists(pointer)) return fallback;
            Dictionary<string, object> record;
            try
            {
                record = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(pointer, Encoding.UTF8));
            }
            catch (Exception e) when (e is ArgumentException || e is InvalidOperationException)
            {
                return fallback;
            }
            object path;
            return record != null && record.TryGetValue("path", out path) && path is string s && Directory.Exists(s) ? s : fallback;
        }

        static string SettingsPath => Path.Combine(DataRoot(), "settings.json");

        /// <summary>Sets the language and returns the previous one, or null when there is no settings file yet.</summary>
        public static string SetLanguage(string language)
        {
            string path = SettingsPath;
            if (!File.Exists(path)) return null;
            string text = File.ReadAllText(path, Encoding.UTF8);
            var match = LanguageValue.Match(text);
            if (!match.Success) return null;
            string previous = match.Groups[2].Value;
            if (previous == language) return null;
            WriteKeepingCopy(path, LanguageValue.Replace(text, "${1}" + language + "${3}", 1));
            return previous;
        }

        /// <summary>Puts the previous language back, only if the player did not change it since.</summary>
        public static void RestoreLanguage(string expectedNow, string previous)
        {
            string path = SettingsPath;
            if (!File.Exists(path)) return;
            string text = File.ReadAllText(path, Encoding.UTF8);
            var match = LanguageValue.Match(text);
            if (!match.Success || match.Groups[2].Value != expectedNow) return;
            WriteKeepingCopy(path, LanguageValue.Replace(text, "${1}" + previous + "${3}", 1));
        }

        static void WriteKeepingCopy(string path, string text)
        {
            string temp = path + ".pt-BR.tmp";
            File.WriteAllText(temp, text, new UTF8Encoding(false));
            File.Replace(temp, path, null);
        }
    }
}
