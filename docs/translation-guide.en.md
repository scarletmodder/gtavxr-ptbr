[Português](translation-guide.md) | **English**

# Translation guide

The rules and fixed terms used in every file of [`translations/`](../translations). If a word sounds wrong in the game, [open an issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) with the `id` of the row, the current text and your suggestion.

## The files

| File | What it is | Texts |
|---|---|---|
| `suite.pt-BR.json` | The Suite window (applied by the installer) | 422 |
| `content-catalog.pt-BR.json` | Names and descriptions of the content mods in the Suite (applied by the installer) | 101 unique texts, 129 places |
| `vr-menu.pt-BR.json` | The VR menu inside the game (applied by the in-game mod and offered to GTAVXR) | 308 |
| `trainer.pt-BR.json` | The Trainer inside the game (applied by the in-game mod and offered to GTAVXR) | 776 |

Each row is `{"id", "en", "pt"}`: the identifier in the GTAVXR source, the English text and the Portuguese text. The tests check that every row keeps the same placeholders as the English, is not empty and has no long dash.

## Rules

- Natural Brazilian Portuguese, like a game localized in Brazil. The player is addressed as "você".
- Translated from the English; the Spanish only settles doubts about meaning.
- No em dash or en dash: comma, colon or parentheses instead.
- Placeholders stay exactly as they are: `{0}`, `%s`, `%d`, `%.1f`, `%%`, `~r~`, `~s~`, `\n`, `>`, `<`, `·`, quotes and spaces at the start and the end.
- Capitalization follows the English: ALL CAPS stays all caps; Title Case becomes sentence case, as usual in Portuguese.
- Length: in-game menus have a fixed width. Stay close to the English length; above 1.3 times the longer of English and Spanish, abbreviate naturally.
- Not translated: proper names (Michael, Los Santos, names of mods, shops, missions), brand names of vehicles and weapons, acronyms (HUD, VR, OpenXR, DLSS, FSR, FPS, DLC), units.

## Fixed terms

| English | Portuguese |
|---|---|
| headset | óculos VR (short label: Óculos) |
| controllers | controles |
| grip | botão de agarrar (short: Agarrar) |
| trigger | gatilho |
| thumbstick | analógico |
| recenter | recentralizar |
| snap turning / smooth turning | giro por passos / giro suave |
| haptics, vibration | vibração |
| profile / save / story | perfil / jogo salvo (short: Save) / modo história |
| settings | configurações (short, in menus: Ajustes) |
| wanted level | nível de procurado |
| god mode / never wanted | modo deus / nunca procurado |
| waypoint | ponto marcado no mapa (short: Marcador) |
| teleport / spawn | teleportar / gerar |
| ped | pedestre, pessoa |
| loadout | equipamento |
| toggle / reset | opção liga/desliga / redefinir |
| frame generation / frame | geração de quadros / quadro |
| library / stable / test version | biblioteca / estável / versão de teste |
| Trainer, Suite, runtime | kept in English |
| ON / OFF | kept as ON and OFF |
