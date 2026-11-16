using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace GtavxrPtbr
{
    /// <summary>The Portuguese texts shipped with this tool (translations\*.pt-BR.json).</summary>
    internal sealed class Translations
    {
        /// <summary>Suite text table: key of GtavxrSuite.L to Portuguese.</summary>
        public IDictionary<string, string> Suite { get; private set; }

        /// <summary>Content-mod catalog: English text to Portuguese.</summary>
        public IDictionary<string, string> Catalog { get; private set; }

        internal Translations(IDictionary<string, string> suite, IDictionary<string, string> catalog)
        {
            Suite = suite;
            Catalog = catalog;
        }

        public static Translations Load(string folder)
        {
            return new Translations(ReadMap(Path.Combine(folder, "suite.pt-BR.json")),
                                    ReadMap(Path.Combine(folder, "content-catalog.pt-BR.json")));
        }

        /// <summary>Reads a list of {"id": ..., "pt": ...} objects into a dictionary.</summary>
        internal static IDictionary<string, string> ReadMap(string path)
        {
            if (!File.Exists(path))
                throw new PatchException("DATA_MISSING", "Translation file not found: " + path);
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var rows = serializer.Deserialize<List<Dictionary<string, string>>>(File.ReadAllText(path, Encoding.UTF8));
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                string id, pt;
                if (!row.TryGetValue("id", out id) || !row.TryGetValue("pt", out pt) || id == null || string.IsNullOrEmpty(pt))
                    throw new PatchException("DATA_INVALID", "Invalid translation row in " + Path.GetFileName(path));
                map[id] = pt;
            }
            return map;
        }
    }

    /// <summary>Minimal JSON string escaping that keeps accents as they are.</summary>
    internal static class JsonText
    {
        public static string Escape(string text)
        {
            var sb = new StringBuilder(text.Length + 8);
            foreach (char c in text)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string Unescape(string escaped)
        {
            return new JavaScriptSerializer().Deserialize<string>("\"" + escaped + "\"");
        }
    }
}
