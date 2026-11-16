using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace GtavxrPtbr
{
    /// <summary>Result of reading or patching a GTAVXR Suite executable.</summary>
    internal sealed class PatchReport
    {
        public int SuiteKeysFound;
        public int SuiteKeysTranslated;
        public int CatalogTextsFound;
        public int CatalogTextsTranslated;
        public bool LanguageButtonTranslated;
        public bool AlreadyPatched;
        public readonly List<string> MissingKeys = new List<string>();

        /// <summary>Share of the Suite texts that this translation covers (0..1).</summary>
        public double Coverage => SuiteKeysFound == 0 ? 0 : (double)SuiteKeysTranslated / SuiteKeysFound;
    }

    /// <summary>
    /// Replaces the Spanish column of the GTAVXR Suite with Brazilian Portuguese, inside the player's own copy.
    /// The Suite keeps every visible word in one table (class GtavxrSuite.L) as {key, {Spanish, English}}, plus a
    /// JSON catalog of content mods with {"es", "en"} pairs. Only those Spanish texts change; nothing else does.
    /// </summary>
    internal static class SuitePatcher
    {
        public const string StringsType = "GtavxrSuite.L";
        public const string CatalogResource = "GtavxrSuite.content-catalog.json";
        public const string MarkerResource = "GtavxrPtbr.Marker";
        public const string SpanishButton = "Español";
        public const string PortugueseButton = "Português";

        static readonly Regex CatalogPair = new Regex(
            "\"es\"(\\s*):(\\s*)\"((?:[^\"\\\\]|\\\\.)*)\"(\\s*),(\\s*)\"en\"(\\s*):(\\s*)\"((?:[^\"\\\\]|\\\\.)*)\"",
            RegexOptions.CultureInvariant);

        /// <summary>Reads the executable and reports what would change, without writing anything.</summary>
        public static PatchReport Inspect(string exePath, Translations translations)
        {
            using (var module = Load(exePath))
                return Apply(module, translations);
        }

        /// <summary>Writes a translated copy of <paramref name="exePath"/> to <paramref name="outputPath"/>.</summary>
        public static PatchReport Patch(string exePath, string outputPath, Translations translations, string version)
        {
            using (var module = Load(exePath))
            {
                var report = Apply(module, translations);
                if (report.AlreadyPatched)
                    throw new PatchException("ALREADY_PATCHED", "This copy of GTAVXR.exe is already translated.");
                module.Resources.Add(new EmbeddedResource(MarkerResource, ManifestResourceAttributes.Private,
                    Encoding.UTF8.GetBytes("gtavxr-ptbr " + version)));
                module.Write(outputPath);
                return report;
            }
        }

        /// <summary>True when the executable carries the marker this tool adds.</summary>
        public static bool IsPatched(string exePath)
        {
            using (var module = Load(exePath))
                return module.Resources.Any(r => r.Name == MarkerResource);
        }

        static ModuleDefinition Load(string path)
        {
            try
            {
                return ModuleDefinition.ReadModule(path, new ReaderParameters { InMemory = true, ReadingMode = ReadingMode.Immediate });
            }
            catch (BadImageFormatException)
            {
                throw new PatchException("NOT_SUITE", "This file is not the GTAVXR Suite.");
            }
        }

        static PatchReport Apply(ModuleDefinition module, Translations translations)
        {
            var report = new PatchReport { AlreadyPatched = module.Resources.Any(r => r.Name == MarkerResource) };
            var strings = module.GetType(StringsType);
            var cctor = strings?.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            if (cctor == null || !cctor.HasBody)
                throw new PatchException("NOT_SUITE", "The GTAVXR text table was not found in this file.");

            PatchStringTable(cctor, translations.Suite, report);
            report.LanguageButtonTranslated = PatchLanguageButton(module);
            PatchCatalog(module, translations.Catalog, report);
            return report;
        }

        /// <summary>
        /// Walks the table initializer. Each entry compiles to
        /// ldstr key; ldc.i4.2; newarr string; (dup | stloc; ldloc); ldc.i4.0; ldstr spanish; stelem.ref; ... ldc.i4.1; ldstr english
        /// The Spanish text is the first "index 0" store after the array is created.
        /// </summary>
        internal static void PatchStringTable(MethodDefinition cctor, IDictionary<string, string> suite, PatchReport report)
        {
            var code = cctor.Body.Instructions;
            for (int i = 0; i + 3 < code.Count; i++)
            {
                if (code[i].OpCode != OpCodes.Ldstr || !IsInt(code[i + 1], 2) || code[i + 2].OpCode != OpCodes.Newarr)
                    continue;
                Instruction spanish = null;
                for (int j = i + 3; j + 1 < code.Count && j <= i + 8; j++)
                    if (IsInt(code[j], 0) && code[j + 1].OpCode == OpCodes.Ldstr) { spanish = code[j + 1]; break; }
                if (spanish == null) continue;
                string key = (string)code[i].Operand;
                report.SuiteKeysFound++;
                string pt;
                if (suite.TryGetValue(key, out pt))
                {
                    spanish.Operand = pt;
                    report.SuiteKeysTranslated++;
                }
                else report.MissingKeys.Add(key);
            }
        }

        static bool IsInt(Instruction ins, int value)
        {
            if (value == 0 && ins.OpCode == OpCodes.Ldc_I4_0) return true;
            if (value == 1 && ins.OpCode == OpCodes.Ldc_I4_1) return true;
            if (value == 2 && ins.OpCode == OpCodes.Ldc_I4_2) return true;
            if (ins.OpCode == OpCodes.Ldc_I4_S) return Convert.ToInt32(ins.Operand) == value;
            if (ins.OpCode == OpCodes.Ldc_I4) return (int)ins.Operand == value;
            return false;
        }

        /// <summary>The Settings page writes the Spanish button label in code; it becomes "Português".</summary>
        static bool PatchLanguageButton(ModuleDefinition module)
        {
            bool found = false;
            foreach (var type in module.GetTypes())
                foreach (var method in type.Methods.Where(m => m.HasBody))
                    foreach (var ins in method.Body.Instructions)
                        if (ins.OpCode == OpCodes.Ldstr && (string)ins.Operand == SpanishButton)
                        {
                            ins.Operand = PortugueseButton;
                            found = true;
                        }
            return found;
        }

        static void PatchCatalog(ModuleDefinition module, IDictionary<string, string> catalog, PatchReport report)
        {
            var resource = module.Resources.OfType<EmbeddedResource>().FirstOrDefault(r => r.Name == CatalogResource);
            if (resource == null) return;
            string json = new UTF8Encoding(false).GetString(resource.GetResourceData());
            string translated = TranslateCatalog(json, catalog, report);
            int index = module.Resources.IndexOf(resource);
            module.Resources[index] = new EmbeddedResource(resource.Name, resource.Attributes, new UTF8Encoding(false).GetBytes(translated));
        }

        /// <summary>Replaces each "es" value with the Portuguese text of its "en" partner; the layout stays the same.</summary>
        internal static string TranslateCatalog(string json, IDictionary<string, string> catalog, PatchReport report)
        {
            return CatalogPair.Replace(json, m =>
            {
                report.CatalogTextsFound++;
                string english = JsonText.Unescape(m.Groups[8].Value);
                string pt;
                if (!catalog.TryGetValue(english, out pt)) return m.Value;
                report.CatalogTextsTranslated++;
                return "\"es\"" + m.Groups[1].Value + ":" + m.Groups[2].Value + "\"" + JsonText.Escape(pt) + "\"" +
                       m.Groups[4].Value + "," + m.Groups[5].Value + "\"en\"" + m.Groups[6].Value + ":" + m.Groups[7].Value +
                       "\"" + m.Groups[8].Value + "\"";
            });
        }
    }

    internal sealed class PatchException : Exception
    {
        public string Code { get; }
        public PatchException(string code, string message) : base(message) { Code = code; }
    }
}
