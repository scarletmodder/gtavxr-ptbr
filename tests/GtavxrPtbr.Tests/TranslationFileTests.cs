using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Xunit;

namespace GtavxrPtbr.Tests
{
    /// <summary>Checks every shipped translation file: same placeholders as English, no empty text, no dashes.</summary>
    public class TranslationFileTests
    {
        static readonly Regex Markers = new Regex(@"\{\d+(:[^}]*)?\}|%[-0-9.]*[sdfxu%]|~[a-z]~|\n", RegexOptions.CultureInvariant);

        public static IEnumerable<object[]> Files()
        {
            yield return new object[] { "suite.pt-BR.json" };
            yield return new object[] { "content-catalog.pt-BR.json" };
            yield return new object[] { "vr-menu.pt-BR.json" };
            yield return new object[] { "trainer.pt-BR.json" };
        }

        static string Folder()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "gtavxr-ptbr.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return Path.Combine(dir.FullName, "translations");
        }

        static List<Dictionary<string, string>> Rows(string name)
        {
            string text = File.ReadAllText(Path.Combine(Folder(), name), Encoding.UTF8);
            return new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Deserialize<List<Dictionary<string, string>>>(text);
        }

        static string Signature(string text) =>
            string.Join("|", Markers.Matches(text).Cast<Match>().Select(m => m.Value).OrderBy(v => v, System.StringComparer.Ordinal));

        [Theory]
        [MemberData(nameof(Files))]
        public void Every_row_keeps_the_english_placeholders(string name)
        {
            var bad = Rows(name).Where(r => Signature(r["en"]) != Signature(r["pt"])).Select(r => r["id"]).ToList();
            Assert.Empty(bad);
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void No_row_is_empty_or_uses_long_dashes(string name)
        {
            var rows = Rows(name);
            Assert.NotEmpty(rows);
            Assert.All(rows, r =>
            {
                Assert.False(string.IsNullOrWhiteSpace(r["pt"]), r["id"]);
                Assert.DoesNotContain("—", r["pt"]);
                Assert.DoesNotContain("–", r["pt"]);
            });
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void Ids_are_unique(string name)
        {
            var ids = Rows(name).Select(r => r["id"]).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }
}
