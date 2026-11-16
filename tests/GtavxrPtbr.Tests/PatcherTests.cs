using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace GtavxrPtbr.Tests
{
    public class PatcherTests : IDisposable
    {
        readonly string dir = Path.Combine(Path.GetTempPath(), "gtavxr-ptbr-tests-" + Guid.NewGuid().ToString("N"));

        public PatcherTests() { Directory.CreateDirectory(dir); }

        public void Dispose() { Directory.Delete(dir, true); }

        /// <summary>
        /// Builds a small stand-in for the Suite: a GtavxrSuite.L table compiled the way csc does it
        /// (stloc/ldloc between the stores), a Settings method with the "Español" button and a catalog resource.
        /// </summary>
        static string BuildFakeSuite(string folder, params string[] keys)
        {
            var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("FakeSuite", new Version(1, 0)), "FakeSuite", ModuleKind.Dll);
            var module = assembly.MainModule;
            var table = new TypeDefinition("GtavxrSuite", "L", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, module.TypeSystem.Object);
            var stringArray = new ArrayType(module.TypeSystem.String);
            var field = new FieldDefinition("Values", FieldAttributes.Public | FieldAttributes.Static, new ArrayType(stringArray));
            table.Fields.Add(field);
            var cctor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, module.TypeSystem.Void);
            cctor.Body.Variables.Add(new VariableDefinition(stringArray));
            var il = cctor.Body.GetILProcessor();
            foreach (string key in keys)
            {
                il.Emit(OpCodes.Ldstr, key);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Newarr, module.TypeSystem.String);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldstr, "es:" + key);
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldstr, "en:" + key);
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ret);
            table.Methods.Add(cctor);

            var settings = new MethodDefinition("Build", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.String);
            var sil = settings.Body.GetILProcessor();
            sil.Emit(OpCodes.Ldstr, SuitePatcher.SpanishButton);
            sil.Emit(OpCodes.Ret);
            table.Methods.Add(settings);
            module.Types.Add(table);

            string catalog = "{\"mods\":[{\"title\": {\"es\": \"Tráfico\", \"en\": \"Traffic\"}}, {\"title\": {\"es\":\"Otro\",\"en\":\"Other\"}}]}";
            module.Resources.Add(new EmbeddedResource(SuitePatcher.CatalogResource, ManifestResourceAttributes.Public, System.Text.Encoding.UTF8.GetBytes(catalog)));
            string path = Path.Combine(folder, "FakeSuite.dll");
            assembly.Write(path);
            assembly.Dispose();
            return path;
        }

        static Translations Data(Dictionary<string, string> suite, Dictionary<string, string> catalog)
        {
            return new Translations(suite, catalog);
        }

        static List<string> Strings(string path)
        {
            using (var module = ModuleDefinition.ReadModule(path, new ReaderParameters { InMemory = true }))
                return module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
                    .SelectMany(m => m.Body.Instructions).Where(i => i.OpCode == OpCodes.Ldstr).Select(i => (string)i.Operand).ToList();
        }

        [Fact]
        public void Patch_replaces_only_the_spanish_column_and_marks_the_file()
        {
            string suite = BuildFakeSuite(dir, "nav.play", "nav.mods");
            string output = Path.Combine(dir, "out.dll");
            var report = SuitePatcher.Patch(suite, output,
                Data(new Dictionary<string, string> { { "nav.play", "Jogar" } }, new Dictionary<string, string> { { "Traffic", "Trânsito" } }), "1.0.0");

            Assert.Equal(2, report.SuiteKeysFound);
            Assert.Equal(1, report.SuiteKeysTranslated);
            Assert.Equal(new[] { "nav.mods" }, report.MissingKeys);
            Assert.True(report.LanguageButtonTranslated);
            Assert.Equal(2, report.CatalogTextsFound);
            Assert.Equal(1, report.CatalogTextsTranslated);

            var strings = Strings(output);
            Assert.Contains("Jogar", strings);
            Assert.DoesNotContain("es:nav.play", strings);
            Assert.Contains("en:nav.play", strings);
            Assert.Contains("es:nav.mods", strings);
            Assert.Contains(SuitePatcher.PortugueseButton, strings);
            Assert.True(SuitePatcher.IsPatched(output));
            Assert.False(SuitePatcher.IsPatched(suite));
        }

        [Fact]
        public void Patch_refuses_a_file_that_is_already_translated()
        {
            string suite = BuildFakeSuite(dir, "nav.play");
            string once = Path.Combine(dir, "once.dll");
            var data = Data(new Dictionary<string, string> { { "nav.play", "Jogar" } }, new Dictionary<string, string>());
            SuitePatcher.Patch(suite, once, data, "1.0.0");
            var error = Assert.Throws<PatchException>(() => SuitePatcher.Patch(once, Path.Combine(dir, "twice.dll"), data, "1.0.0"));
            Assert.Equal("ALREADY_PATCHED", error.Code);
        }

        [Fact]
        public void Inspect_rejects_a_file_without_the_text_table()
        {
            var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Other", new Version(1, 0)), "Other", ModuleKind.Dll);
            string path = Path.Combine(dir, "Other.dll");
            assembly.Write(path);
            assembly.Dispose();
            var error = Assert.Throws<PatchException>(() => SuitePatcher.Inspect(path, Data(new Dictionary<string, string>(), new Dictionary<string, string>())));
            Assert.Equal("NOT_SUITE", error.Code);
        }

        [Fact]
        public void Inspect_rejects_a_file_that_is_not_a_program()
        {
            string path = Path.Combine(dir, "notes.txt");
            File.WriteAllText(path, "hello");
            var error = Assert.Throws<PatchException>(() => SuitePatcher.Inspect(path, Data(new Dictionary<string, string>(), new Dictionary<string, string>())));
            Assert.Equal("NOT_SUITE", error.Code);
        }

        [Fact]
        public void Catalog_keeps_its_layout_and_escapes_quotes()
        {
            var report = new PatchReport();
            string json = "{\"a\": {\"es\": \"Uno\",\n  \"en\": \"One \\\"big\\\"\"}}";
            string result = SuitePatcher.TranslateCatalog(json, new Dictionary<string, string> { { "One \"big\"", "Um \"grande\"" } }, report);
            Assert.Equal("{\"a\": {\"es\": \"Um \\\"grande\\\"\",\n  \"en\": \"One \\\"big\\\"\"}}", result);
            Assert.Equal(1, report.CatalogTextsTranslated);
        }

        [Theory]
        [InlineData("linha\nnova \"aspas\" \\ barra")]
        [InlineData("Ação, configuração e pão")]
        public void Json_text_round_trips(string text)
        {
            Assert.Equal(text, JsonText.Unescape(JsonText.Escape(text)));
        }
    }
}
