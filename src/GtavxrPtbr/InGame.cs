using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace GtavxrPtbr
{
    /// <summary>
    /// The in-game menus (VR menu and Trainer) are translated by a GTAVXR script mod (GtavxrPtbr.asi) that changes the
    /// texts in memory while the game runs. This class adds it to the player's GTAVXR library through the Suite's own
    /// import command, turns it on in the profiles, and takes it out again.
    /// </summary>
    internal static class InGame
    {
        public const string AsiName = "GtavxrPtbr.asi";

        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        static readonly Regex ModsArray = new Regex("(\"mods\"\\s*:\\s*\\[)([^\\]]*)(\\])", RegexOptions.CultureInvariant);
        static readonly Regex TestProfile = new Regex("\"test\"\\s*:\\s*true", RegexOptions.CultureInvariant);
        static readonly Regex CardId = new Regex("^[a-z0-9-]{3,64}$", RegexOptions.CultureInvariant);

        internal sealed class Imported
        {
            public string Id;
            /// <summary>The card was already in the library (the player had added the same mod): it is theirs to keep.</summary>
            public bool AlreadyThere;
        }

        /// <summary>
        /// Imports the mod with the Suite (GTAVXR.exe --import-mods) and returns its card. The card must be a script mod
        /// that GTAVXR can load with our .asi in it; anything else is taken out again and reported as an error.
        /// </summary>
        public static Imported Import(string suiteExe, string modZip)
        {
            if (!File.Exists(modZip)) throw new PatchException("INGAME_MISSING", "Mod package not found: " + modZip);
            var before = new HashSet<string>(CardIds());
            string result = Path.Combine(Path.GetTempPath(), "gtavxr-ptbr-import-" + Guid.NewGuid().ToString("N") + ".json");
            string id;
            try
            {
                var start = new ProcessStartInfo(suiteExe, "--import-mods \"" + modZip + "\" \"" + result + "\"")
                {
                    UseShellExecute = false, CreateNoWindow = true,
                };
                using (var p = Process.Start(start))
                {
                    if (!p.WaitForExit(180000))
                    {
                        try { p.Kill(); } catch (InvalidOperationException) { }
                        throw new PatchException("INGAME_IMPORT", "GTAVXR did not finish importing the mod.");
                    }
                }
                if (!File.Exists(result)) throw new PatchException("INGAME_IMPORT", "GTAVXR did not report the import.");
                var report = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(result, Encoding.UTF8));
                object status;
                report.TryGetValue("status", out status);
                if (!"IMPORTED".Equals(status))
                {
                    object message;
                    report.TryGetValue("message", out message);
                    throw new PatchException("INGAME_IMPORT", "GTAVXR refused the mod: " + (message ?? status));
                }
                object cardsValue;
                report.TryGetValue("cards", out cardsValue);
                var cards = cardsValue as System.Collections.ArrayList;
                string first = cards != null && cards.Count == 1 ? cards[0] as string : null;
                id = first?.Split(' ')[0];
                if (id == null || !CardId.IsMatch(id)) throw new PatchException("INGAME_IMPORT", "Unexpected import result.");
            }
            finally
            {
                if (File.Exists(result)) File.Delete(result);
            }
            var imported = new Imported { Id = id, AlreadyThere = before.Contains(id) };
            if (!Loadable(id))
            {
                // an older Suite keeps an unknown .asi as a plain archive that never runs: do not leave it switched on
                if (!imported.AlreadyThere) Remove(id, null, true);
                throw new PatchException("INGAME_NOT_LOADABLE",
                    "GTAVXR added the mod but cannot load it as a script mod. This GTAVXR version may not support .asi mods.");
            }
            return imported;
        }

        /// <summary>The card is a script mod GTAVXR can load, carrying our .asi.</summary>
        static bool Loadable(string id)
        {
            string card = Path.Combine(SuiteSettings.DataRoot(), "mods", id + ".json");
            if (!File.Exists(card)) return false;
            string text = File.ReadAllText(card, Encoding.UTF8);
            return Regex.IsMatch(text, "\"kind\"\\s*:\\s*\"script\"") && Regex.IsMatch(text, "\"loadable\"\\s*:\\s*true") &&
                   text.IndexOf(AsiName, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Turns the mod on in the given profiles (folder names), or, with null, in every profile that is not a test profile.
        /// Returns the profiles where it was switched on (not those that had it on already).
        /// </summary>
        public static List<string> Enable(string id, ICollection<string> only)
        {
            var changed = new List<string>();
            foreach (string profile in Profiles())
            {
                string name = Path.GetFileName(Path.GetDirectoryName(profile));
                string text = File.ReadAllText(profile, Encoding.UTF8);
                if (only != null ? !only.Contains(name) : TestProfile.IsMatch(text)) continue;
                var match = ModsArray.Match(text);
                if (!match.Success || Ids(match.Groups[2].Value).Contains(id)) continue;
                string inside = match.Groups[2].Value.Trim();
                string updated = text.Substring(0, match.Groups[2].Index) + (inside.Length == 0 ? "\"" + id + "\"" : inside + ",\"" + id + "\"") +
                                 text.Substring(match.Groups[2].Index + match.Groups[2].Length);
                Write(profile, updated);
                changed.Add(name);
            }
            return changed;
        }

        /// <summary>
        /// Takes the mod out of the given profiles (null: every profile) and, with deleteCard, removes its card and the
        /// files no other card uses.
        /// </summary>
        public static void Remove(string id, ICollection<string> only, bool deleteCard)
        {
            if (id == null || !CardId.IsMatch(id)) return;
            foreach (string profile in Profiles())
            {
                if (only != null && !only.Contains(Path.GetFileName(Path.GetDirectoryName(profile)))) continue;
                string text = File.ReadAllText(profile, Encoding.UTF8);
                var match = ModsArray.Match(text);
                if (!match.Success) continue;
                var ids = Ids(match.Groups[2].Value);
                if (!ids.Remove(id)) continue;
                string inside = string.Join(",", ids.Select(x => "\"" + x + "\""));
                Write(profile, text.Substring(0, match.Groups[2].Index) + inside + text.Substring(match.Groups[2].Index + match.Groups[2].Length));
            }
            if (!deleteCard) return;
            string root = SuiteSettings.DataRoot();
            string card = Path.Combine(root, "mods", id + ".json");
            if (!File.Exists(card)) return;
            var hashes = BlobHashes(File.ReadAllText(card, Encoding.UTF8));
            File.Delete(card);
            var stillUsed = new HashSet<string>(Directory.EnumerateFiles(Path.Combine(root, "mods"), "*.json")
                .SelectMany(f => BlobHashes(File.ReadAllText(f, Encoding.UTF8))), StringComparer.OrdinalIgnoreCase);
            foreach (string hash in hashes.Where(h => !stillUsed.Contains(h)))
            {
                // the Suite keeps its stored files read-only; an orphan left behind is harmless, so failing here is not an error
                string blob = Path.Combine(root, "blobs", hash.Substring(0, 2), hash);
                try
                {
                    if (!File.Exists(blob)) continue;
                    File.SetAttributes(blob, FileAttributes.Normal);
                    File.Delete(blob);
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("WARNING BLOB: could not remove " + blob + " (" + e.Message + ")");
                }
            }
        }

        /// <summary>True when the card is still in the library (the player did not remove the mod in the Suite).</summary>
        public static bool Present(string id) =>
            id != null && CardId.IsMatch(id) && File.Exists(Path.Combine(SuiteSettings.DataRoot(), "mods", id + ".json"));

        static IEnumerable<string> CardIds()
        {
            string dir = Path.Combine(SuiteSettings.DataRoot(), "mods");
            if (!Directory.Exists(dir)) return Enumerable.Empty<string>();
            return Directory.EnumerateFiles(dir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
        }

        static IEnumerable<string> Profiles()
        {
            string dir = Path.Combine(SuiteSettings.DataRoot(), "profiles");
            if (!Directory.Exists(dir)) return Enumerable.Empty<string>();
            return Directory.EnumerateDirectories(dir).Select(d => Path.Combine(d, "profile.json")).Where(File.Exists).ToList();
        }

        static List<string> Ids(string inside) =>
            Regex.Matches(inside, "\"([^\"]*)\"").Cast<Match>().Select(m => m.Groups[1].Value).ToList();

        static IEnumerable<string> BlobHashes(string card) =>
            Regex.Matches(card, "\"sha256\"\\s*:\\s*\"([0-9A-Fa-f]{64})\"").Cast<Match>().Select(m => m.Groups[1].Value);

        static void Write(string path, string text)
        {
            string temp = path + ".pt-BR.tmp";
            File.WriteAllText(temp, text, Utf8);
            File.Replace(temp, path, null);
        }
    }
}
