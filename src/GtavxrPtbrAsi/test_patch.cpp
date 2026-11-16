// Test of the table patch against copies of the real GTAVXR files, outside the game.
// Usage: test_patch.exe <copy of gtavxr_trainer.dll> <copy of the GTAVXR game executable> [trainer rows] [VR rows]
//        test_patch.exe fake_tables.dll fake_tables.dll 5 4      (the stand-in used by the CI)
//        test_patch.exe --refuse fake_wider.dll                  (tables wider than expected: nothing may change)
// Each file is mapped as an image without running any of its code (DONT_RESOLVE_DLL_REFERENCES), the table is
// patched in memory, and every row is checked: Spanish column = Portuguese text, other columns unchanged.
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include "table_patch.h"
#include "texts.g.h"

using namespace gtavxr_ptbr;

static int failures = 0;
#define CHECK(c, ...) do { if (!(c)) { ++failures; printf("FAIL: " __VA_ARGS__); printf("\n"); } } while (0)

static const char* Lookup(const Text* texts, size_t count, const char* key) {
    for (size_t j = 0; j < count; ++j) if (!strcmp(texts[j].key, key)) return texts[j].pt;
    return nullptr;
}

static void Run(const wchar_t* path, const char* name, const char* anchor, int width, int spanish, const Text* texts,
                size_t count, size_t expectRows) {
    HMODULE h = LoadLibraryExW(path, nullptr, DONT_RESOLVE_DLL_REFERENCES);
    CHECK(h, "%s: could not map (%lu)", name, GetLastError());
    if (!h) return;
    Module m;
    CHECK(ReadModule(h, m), "%s: not a PE64 image", name);
    bool refused = false;
    Table t = LocateTable(m, anchor, width, 0, texts, count, &refused);
    CHECK(!refused, "%s: table refused", name);
    CHECK(t.rows == expectRows, "%s: %zu rows, expected %zu", name, t.rows, expectRows);

    // before: copy of every column
    size_t cells = t.rows * width;
    const char** before = new const char*[cells];
    memcpy(before, reinterpret_cast<void*>(t.first), cells * 8);

    PatchResult r = PatchTable(t, 0, spanish, texts, count);
    PatchResult again = PatchTable(t, 0, spanish, texts, count);
    CHECK(again.translated == 0 && again.already == r.translated, "%s: second patch changed rows again", name);

    size_t pt = 0, kept = 0;
    auto rows = reinterpret_cast<const char**>(t.first);
    for (size_t i = 0; i < t.rows; ++i) {
        const char** row = rows + i * width;
        const char** old = before + i * width;
        for (int k = 0; k < width; ++k)
            if (k != spanish) CHECK(row[k] == old[k], "%s row %zu: column %d changed", name, i, k);
        const char* want = Lookup(texts, count, row[0]);
        if (want) { CHECK(row[spanish] == want, "%s row %zu: not Portuguese", name, i); ++pt; }
        else { CHECK(row[spanish] == old[spanish], "%s row %zu: unknown row changed", name, i); ++kept; }
    }
    printf("%s: table of %zu rows, %zu in Portuguese, %zu left as they were\n", name, t.rows, pt, kept);
    CHECK(pt == r.translated, "%s: count mismatch", name);
    delete[] before;
    FreeLibrary(h);
}

// A table laid out wider than the plugin expects (a future GTAVXR with a Portuguese column of its own) must be refused,
// and no byte of the module may change.
static void Refuse(const wchar_t* path, const char* name, const char* anchor, int width, const Text* texts, size_t count) {
    HMODULE h = LoadLibraryExW(path, nullptr, DONT_RESOLVE_DLL_REFERENCES);
    CHECK(h, "%s: could not map (%lu)", name, GetLastError());
    if (!h) return;
    Module m;
    CHECK(ReadModule(h, m), "%s: not a PE64 image", name);
    Table seen = FindTable(m, anchor, width, 0);
    CHECK(seen.rows > 0, "%s: the wider table was not even seen, so the test proves nothing", name);
    bool refused = false;
    Table t = LocateTable(m, anchor, width, 0, texts, count, &refused);
    CHECK(refused && !t.rows, "%s: a wider table was accepted (%zu rows)", name, t.rows);
    printf("%s: wider table of %zu apparent rows refused\n", name, seen.rows);
    FreeLibrary(h);
}

int wmain(int argc, wchar_t** argv) {
    if (argc == 3 && !wcscmp(argv[1], L"--refuse")) {
        Refuse(argv[2], "Trainer", "Previous Trainer Options", 2, TrainerTexts, TrainerTextCount);
        Refuse(argv[2], "VR menu", "SETTINGS / AJUSTES", 3, VrTexts, VrTextCount);
        printf(failures ? "%d FAILURES\n" : "OK\n", failures);
        return failures ? 1 : 0;
    }
    if (argc != 3 && argc != 5) {
        printf("usage: test_patch <gtavxr_trainer.dll copy> <game exe copy> [trainer rows] [VR rows]\n");
        return 2;
    }
    // GTAVXR 1.0.2: 777 Trainer rows (776 different texts) and 308 VR menu rows
    size_t trainerRows = argc == 5 ? wcstoul(argv[3], nullptr, 10) : 777;
    size_t vrRows = argc == 5 ? wcstoul(argv[4], nullptr, 10) : 308;
    Run(argv[1], "Trainer", "Previous Trainer Options", 2, 1, TrainerTexts, TrainerTextCount, trainerRows);
    Run(argv[2], "VR menu", "SETTINGS / AJUSTES", 3, 2, VrTexts, VrTextCount, vrRows);
    printf(failures ? "%d FAILURES\n" : "OK\n", failures);
    return failures ? 1 : 0;
}
