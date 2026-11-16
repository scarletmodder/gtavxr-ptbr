// GTAVXR PT-BR in-game plugin (.asi).
//
// GTAVXR loads it into the game like any script mod. Once a second it looks for the VR menu text table (in the game
// executable) and the Trainer text table (in gtavxr_trainer.dll), and points their Spanish column at the Portuguese texts
// carried by this plugin. With GTAVXR's in-game language set to Spanish, the menus show Portuguese.
// Nothing on disk changes: GTAVXR's files, and its checks of them, stay exactly as they were.
#include <windows.h>
#include <shlobj.h>
#include <stdio.h>
#include "table_patch.h"
#include "texts.g.h"

__declspec(dllimport) void scriptRegister(HMODULE module, void (*main)());
__declspec(dllimport) void scriptUnregister(HMODULE module);
__declspec(dllimport) void scriptWait(DWORD milliseconds);

#ifndef GTAVXR_PTBR_VERSION
#define GTAVXR_PTBR_VERSION "dev"
#endif

namespace {

using namespace gtavxr_ptbr;

uintptr_t vrDone = 0;       // base of the module whose VR table was handled
uintptr_t trainerDone = 0;  // base of the gtavxr_trainer.dll that was handled (it can be reloaded)
bool vrMissingLogged = false;
bool faultLogged = false;

void Log(const char* format, ...) {
    wchar_t* local = nullptr;
    if (FAILED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &local))) return;
    wchar_t path[MAX_PATH];
    swprintf_s(path, L"%s\\GTAVXR-PTBR", local);
    CoTaskMemFree(local);
    CreateDirectoryW(path, nullptr);
    wcscat_s(path, L"\\in-game.log");
    FILE* f = nullptr;
    if (_wfopen_s(&f, path, L"a") || !f) return;
    SYSTEMTIME t;
    GetLocalTime(&t);
    fprintf(f, "%04u-%02u-%02u %02u:%02u:%02u ", t.wYear, t.wMonth, t.wDay, t.wHour, t.wMinute, t.wSecond);
    va_list args;
    va_start(args, format);
    vfprintf(f, format, args);
    va_end(args);
    fputc('\n', f);
    fclose(f);
}

// The scan reads memory described by the module's headers. A page that turns out not to be readable (or a module
// that goes away mid-scan) raises an access violation: it is caught here and the work is given up, not the game.
struct Job { const Module* m; const char* anchor; int width; int spanish; const Text* texts; size_t count;
             Table table; bool refused; PatchResult result; };

void RunJob(Job& job) {
    job.table = LocateTable(*job.m, job.anchor, job.width, 0, job.texts, job.count, &job.refused);
    if (job.table.rows) job.result = PatchTable(job.table, 0, job.spanish, job.texts, job.count);
}

bool SafeRun(Job& job) {
    __try {
        RunJob(job);
        return true;
    } __except (GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {
        return false;
    }
}

void LogFault(const char* what) {
    if (!faultLogged) Log("%s: memory could not be read, nothing more is changed there", what);
    faultLogged = true;
}

void PatchVr() {
    HMODULE game = GetModuleHandleW(nullptr);
    auto base = reinterpret_cast<uintptr_t>(game);
    if (vrDone == base) return;
    vrDone = base;
    Module m;
    if (!ReadModule(game, m)) return;
    Job job{&m, "SETTINGS / AJUSTES", 3, 2, VrTexts, VrTextCount, {}, false, {}};
    if (!SafeRun(job)) { LogFault("VR menu"); return; }
    if (job.refused) { Log("VR menu: text table does not match this translation (GTAVXR version not covered yet): left as it is"); return; }
    if (!job.table.rows) {
        if (!vrMissingLogged) Log("VR menu: text table not found (GTAVXR version not covered yet)");
        vrMissingLogged = true;
        return;
    }
    Log("VR menu: %zu of %zu texts in Portuguese", job.result.translated + job.result.already, job.result.rows);
}

Table trainerTable;

// True while the patched table is still the one in memory: a Trainer unloaded and loaded again at the same address
// comes back with its Spanish texts, and is patched again.
bool TrainerStillPatched() {
    if (!trainerTable.rows) return false;
    auto row = reinterpret_cast<const char* const*>(trainerTable.first);
    for (size_t i = 0; i < trainerTable.rows; ++i, row += 2)
        for (size_t j = 0; j < TrainerTextCount; ++j)
            if (row[1] == TrainerTexts[j].pt) return true;
    return false;
}

bool SafeStillPatched(bool* result) {
    __try {
        *result = TrainerStillPatched();
        return true;
    } __except (GetExceptionCode() == EXCEPTION_ACCESS_VIOLATION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH) {
        return false;
    }
}

void PatchTrainerLocked(HMODULE trainer) {
    auto base = reinterpret_cast<uintptr_t>(trainer);
    if (trainerDone == base) {
        bool still = false;
        if (!trainerTable.rows) return;
        if (!SafeStillPatched(&still)) { LogFault("Trainer"); return; }
        if (still) return;
    }
    trainerDone = base;
    trainerTable = Table();
    Module m;
    if (!ReadModule(trainer, m)) return;
    Job job{&m, "Previous Trainer Options", 2, 1, TrainerTexts, TrainerTextCount, {}, false, {}};
    if (!SafeRun(job)) { LogFault("Trainer"); return; }
    if (job.refused) { Log("Trainer: text table does not match this translation (GTAVXR version not covered yet): left as it is"); return; }
    if (!job.table.rows) { Log("Trainer: text table not found (GTAVXR version not covered yet)"); return; }
    Log("Trainer: %zu of %zu texts in Portuguese", job.result.translated + job.result.already, job.result.rows);
    trainerTable = job.table;
}

void PatchTrainer() {
    // holding a reference keeps the Trainer loaded while it is read and patched
    HMODULE trainer = nullptr;
    if (!GetModuleHandleExW(0, L"gtavxr_trainer.dll", &trainer) || !trainer) {
        trainerDone = 0;
        trainerTable = Table();
        return;
    }
    PatchTrainerLocked(trainer);
    FreeLibrary(trainer);
}

void ScriptMain() {
    Log("GTAVXR PT-BR " GTAVXR_PTBR_VERSION " loaded");
    for (;;) {
        PatchVr();
        PatchTrainer();
        scriptWait(1000);
    }
}

}  // namespace

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID) {
    if (reason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(module);
        // The game's tables point at texts inside this plugin, so it must never leave memory before the game does.
        HMODULE pinned = nullptr;
        GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_PIN | GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
                           reinterpret_cast<LPCWSTR>(&DllMain), &pinned);
        scriptRegister(module, ScriptMain);
    } else if (reason == DLL_PROCESS_DETACH) {
        scriptUnregister(module);
    }
    return TRUE;
}
