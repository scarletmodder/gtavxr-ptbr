// Finds a GTAVXR text table in a loaded module and points its Spanish column at Portuguese texts.
//
// The tables are arrays of `const char*` rows in the module's read-only data:
//   VR menu:  { source, english, spanish }   (row width 3, key = source)
//   Trainer:  { english, spanish }           (row width 2, key = english)
// Only the pointers in memory change. The files on disk are never touched, so GTAVXR's integrity check is unaffected.
#pragma once
#include <windows.h>
#include <stdint.h>
#include <string.h>

namespace gtavxr_ptbr {

struct Text { const char* key; const char* pt; };

struct Region { uintptr_t begin; uintptr_t end; };

struct Module {
    uintptr_t base = 0;
    Region data[8] = {};   // initialized, non-executable sections (where tables and strings live)
    int dataCount = 0;
};

inline bool ReadModule(HMODULE handle, Module& out) {
    out = Module();
    if (!handle) return false;
    auto base = reinterpret_cast<uintptr_t>(handle);
    auto dos = reinterpret_cast<const IMAGE_DOS_HEADER*>(base);
    if (dos->e_magic != IMAGE_DOS_SIGNATURE) return false;
    auto nt = reinterpret_cast<const IMAGE_NT_HEADERS64*>(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE || nt->OptionalHeader.Magic != IMAGE_NT_OPTIONAL_HDR64_MAGIC) return false;
    out.base = base;
    auto section = IMAGE_FIRST_SECTION(nt);
    for (WORD i = 0; i < nt->FileHeader.NumberOfSections && out.dataCount < 8; ++i, ++section) {
        DWORD c = section->Characteristics;
        if ((c & IMAGE_SCN_CNT_INITIALIZED_DATA) && !(c & IMAGE_SCN_MEM_EXECUTE) && section->Misc.VirtualSize) {
            DWORD size = section->Misc.VirtualSize < section->SizeOfRawData || !section->SizeOfRawData
                             ? section->Misc.VirtualSize : section->SizeOfRawData;
            out.data[out.dataCount++] = { base + section->VirtualAddress, base + section->VirtualAddress + size };
        }
    }
    return out.dataCount > 0;
}

inline const Region* RegionOf(const Module& m, uintptr_t p) {
    for (int i = 0; i < m.dataCount; ++i)
        if (p >= m.data[i].begin && p < m.data[i].end) return &m.data[i];
    return nullptr;
}

// A pointer that lands on a NUL-terminated string (at most 4096 bytes) inside the module's data.
inline const char* StringAt(const Module& m, uintptr_t p) {
    const Region* r = RegionOf(m, p);
    if (!r) return nullptr;
    uintptr_t limit = p + 4096 < r->end ? p + 4096 : r->end;
    for (uintptr_t q = p; q < limit; ++q)
        if (*reinterpret_cast<const char*>(q) == 0) return reinterpret_cast<const char*>(p);
    return nullptr;
}

inline bool RowValid(const Module& m, uintptr_t row, int width) {
    for (int k = 0; k < width; ++k) {
        uintptr_t slot = row + 8 * k;
        const Region* r = RegionOf(m, slot);
        if (!r || slot + 8 > r->end) return false;
        if (!StringAt(m, *reinterpret_cast<const uintptr_t*>(slot))) return false;
    }
    return true;
}

inline uintptr_t FindString(const Module& m, const char* text) {
    size_t n = strlen(text) + 1;  // with the NUL, so a longer text that starts the same does not match
    for (int i = 0; i < m.dataCount; ++i) {
        const Region& r = m.data[i];
        if (r.end - r.begin < n) continue;
        for (uintptr_t p = r.begin; p + n <= r.end; ++p)
            if (*reinterpret_cast<const char*>(p) == text[0] && !memcmp(reinterpret_cast<const void*>(p), text, n)) return p;
    }
    return 0;
}

struct Table { uintptr_t first = 0; size_t rows = 0; int width = 0; };

// The table that holds a row whose key column points at `anchor`. Walks back to its first row and forward to its last.
inline Table FindTable(const Module& m, const char* anchor, int width, int keyColumn) {
    Table t;
    uintptr_t text = FindString(m, anchor);
    if (!text) return t;
    for (int i = 0; i < m.dataCount; ++i) {
        const Region& r = m.data[i];
        for (uintptr_t p = (r.begin + 7) & ~uintptr_t(7); p + 8 <= r.end; p += 8) {
            if (*reinterpret_cast<const uintptr_t*>(p) != text) continue;
            uintptr_t row = p - 8 * keyColumn;
            if (row < r.begin || !RowValid(m, row, width)) continue;
            uintptr_t stride = 8 * width;
            uintptr_t first = row;
            while (first >= r.begin + stride && RowValid(m, first - stride, width)) first -= stride;
            uintptr_t last = row;
            while (last + 2 * stride <= r.end && RowValid(m, last + stride, width)) last += stride;
            size_t rows = (last - first) / stride + 1;
            if (rows > t.rows) { t.first = first; t.rows = rows; t.width = width; }
        }
    }
    return t;
}

// How many rows have a key this translation knows.
inline size_t KnownKeys(const Table& t, int keyColumn, const Text* texts, size_t count) {
    size_t known = 0;
    for (size_t i = 0; i < t.rows; ++i) {
        const char* key = reinterpret_cast<const char* const*>(t.first + i * 8 * t.width)[keyColumn];
        for (size_t j = 0; j < count; ++j)
            if (!strcmp(key, texts[j].key)) { ++known; break; }
    }
    return known;
}

// FindTable, accepted only when at least 70% of its keys are known. A table whose real rows are wider than `width`
// (for example a future GTAVXR with a Portuguese column of its own) is still made of valid text pointers, so it would
// be found, but read out of step: its "keys" would be other columns and the writes would land in the wrong column.
// Out of step, at most about a third of the keys match, so the check refuses it and nothing is written.
inline Table LocateTable(const Module& m, const char* anchor, int width, int keyColumn, const Text* texts, size_t count,
                         bool* refused) {
    Table t = FindTable(m, anchor, width, keyColumn);
    *refused = false;
    if (t.rows && KnownKeys(t, keyColumn, texts, count) * 10 < t.rows * 7) {
        *refused = true;
        return Table();
    }
    return t;
}

struct PatchResult { size_t rows = 0; size_t translated = 0; size_t already = 0; };

// Points the Spanish column of every row whose key is in `texts` at its Portuguese text.
// `texts` must stay alive for as long as the module is loaded (they are static data of this plugin).
inline PatchResult PatchTable(const Table& t, int keyColumn, int spanishColumn, const Text* texts, size_t count) {
    PatchResult result;
    if (!t.rows) return result;
    result.rows = t.rows;
    size_t bytes = t.rows * 8 * t.width;
    DWORD old = 0;
    if (!VirtualProtect(reinterpret_cast<void*>(t.first), bytes, PAGE_READWRITE, &old)) return result;
    for (size_t i = 0; i < t.rows; ++i) {
        auto row = reinterpret_cast<const char**>(t.first + i * 8 * t.width);
        const char* key = row[keyColumn];
        for (size_t j = 0; j < count; ++j) {
            if (strcmp(key, texts[j].key) != 0) continue;
            if (row[spanishColumn] == texts[j].pt) ++result.already;
            else { row[spanishColumn] = texts[j].pt; ++result.translated; }
            break;
        }
    }
    DWORD ignored = 0;
    VirtualProtect(reinterpret_cast<void*>(t.first), bytes, old, &ignored);
    return result;
}

}  // namespace gtavxr_ptbr
