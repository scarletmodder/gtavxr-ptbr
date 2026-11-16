// A stand-in for the GTAVXR files in the CI, where they are not available (and never shipped): a DLL with a VR menu
// table and a Trainer table laid out like the real ones, with the first rows of each and placeholder Spanish texts.
// test_patch.exe checks the patch against it the same way it checks copies of the real files.
// In the real files each table has non-text data around it; here each one gets a section of its own, so the two
// tables are not read as one longer table.
//
// Built with /DFAKE_WIDER it becomes fake_wider.dll: the same tables with one more column (Portuguese), as a future
// GTAVXR with Portuguese of its own could have them. The plugin must refuse those tables and change nothing.
#pragma section(".vrtab", read)
#pragma section(".trtab", read)

#ifndef FAKE_WIDER

struct VrRow { const char* source; const char* english; const char* spanish; };
struct TrainerRow { const char* english; const char* spanish; };

extern "C" __declspec(dllexport) __declspec(allocate(".vrtab")) const VrRow FakeVr[] = {
    {"SETTINGS / AJUSTES", "Settings", "es-1"},
    {"Calibration / Calibracion  >", "Calibration  >", "es-2"},
    {"Giro: %s", "Turning: %s", "es-3"},
    {"a row this translation does not know", "unknown", "es-4"},
};

extern "C" __declspec(dllexport) __declspec(allocate(".trtab")) const TrainerRow FakeTrainer[] = {
    {"Previous Trainer Options", "es-1"},
    {"Temporary access to the original VR Trainer while its replacements are completed.", "es-2"},
    {"Spanish", "es-3"},
    {"a row this translation does not know", "es-4"},
    {"Trainer", "es-5"},
};

#else

struct VrRow { const char* source; const char* english; const char* spanish; const char* portuguese; };
struct TrainerRow { const char* english; const char* spanish; const char* portuguese; };

extern "C" __declspec(dllexport) __declspec(allocate(".vrtab")) const VrRow FakeVr[] = {
    {"SETTINGS / AJUSTES", "Settings", "es-1", "pt-1"},
    {"Calibration / Calibracion  >", "Calibration  >", "es-2", "pt-2"},
    {"Giro: %s", "Turning: %s", "es-3", "pt-3"},
    {"a row this translation does not know", "unknown", "es-4", "pt-4"},
};

extern "C" __declspec(dllexport) __declspec(allocate(".trtab")) const TrainerRow FakeTrainer[] = {
    {"Previous Trainer Options", "es-1", "pt-1"},
    {"Temporary access to the original VR Trainer while its replacements are completed.", "es-2", "pt-2"},
    {"Spanish", "es-3", "pt-3"},
    {"a row this translation does not know", "es-4", "pt-4"},
    {"Trainer", "es-5", "pt-5"},
};

#endif
