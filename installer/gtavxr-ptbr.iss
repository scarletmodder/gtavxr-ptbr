; Inno Setup script of the Windows installer of GTAVXR PT-BR.
; It installs a small tool for the current user (no administrator prompt), in English or in Brazilian
; Portuguese, and uses it to translate the player's own copy of the GTAVXR Suite (GTAVXR.exe).
; The in-game menus are translated by a script mod of our own (GtavxrPtbr.asi), added to the player's GTAVXR
; library with the Suite's own import command: it changes the texts in memory while the game runs.
; No file of GTAVXR is shipped: one page says where to get the mod, another finds the copy to translate.
; The original GTAVXR.exe is kept next to it; uninstalling from Windows puts it back.
;
; Build with scripts\build.ps1, which compiles the tool first and passes the version.

#if VER < EncodeVer(6, 7, 0)
  #error Inno Setup 6.7 or newer is required (the version this script is built and tested with)
#endif

#define AppName "GTAVXR PT-BR"
#define AppPublisher "Scarlet Modder"
#define AppURL "https://github.com/scarletmodder/gtavxr-ptbr"
#define ToolExe "GtavxrPtbr.exe"
; Identifies the installation across upgrades: never change it.
#define AppGuid "6F1C2B9E-3A47-4D58-9E21-7B0C5D8A4F36"
#define ModURL "https://github.com/LVL-VR/GTAVXR/releases/latest"
; Where scripts\build.ps1 leaves the compiled tool.
#define ToolDir "..\work\publish"
; Where scripts\build.ps1 leaves the in-game mod package.
#define InGameZip "GTAVXR PT-BR menus do jogo.zip"
#define InGameDir "..\work\asi\package"

#ifndef AppVersion
  #error AppVersion is missing: run scripts\build.ps1
#endif

[Setup]
AppId={{{#AppGuid}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
VersionInfoVersion={#AppVersion}.0
VersionInfoCompany={#AppPublisher}
VersionInfoDescription={#AppName}
; Per user: GTAVXR lives in the user's own folders, so the installer asks for no administrator rights.
PrivilegesRequired=lowest
DefaultDirName={autopf}\{#AppName}
DisableDirPage=yes
DisableWelcomePage=no
DisableProgramGroupPage=yes
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#ToolExe}
SetupIconFile=..\assets\icon.ico
WizardStyle=modern dynamic windows11
WizardSizePercent=115
ShowLanguageDialog=auto
OutputDir=..\dist
OutputBaseFilename=GTAVXR-PTBR-Setup-v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
; Fixed date on the installed files: the build time is not recorded.
TimeStampsInUTC=yes
TouchDate=2026-01-01
TouchTime=00:00

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "pt"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Messages]
en.WelcomeLabel2=This installs [name/ver] on your computer.%n%nIt translates the GTAVXR Suite (the program with the Play button of the GTAVXR virtual reality mod for GTA V Legacy) into Brazilian Portuguese.%n%nNo file of GTAVXR comes with this installer: it changes only the texts of the copy you already have, and keeps the original next to it. Uninstalling puts the original back.
pt.WelcomeLabel2=Este assistente instala o [name/ver] no seu computador.%n%nEle traduz para o português do Brasil a Suite do GTAVXR (o programa com o botão Jogar do mod de realidade virtual GTAVXR para o GTA V Legacy).%n%nNenhum arquivo do GTAVXR vem neste instalador: ele muda só os textos da cópia que você já tem e guarda a original ao lado. Desinstalar devolve a original.

[CustomMessages]
en.NeedsTitle=What this translation needs
en.NeedsSubtitle=The GTAVXR mod, made by other people. This installer does not install it.
en.NeedsIntro=GTAVXR PT-BR translates a GTAVXR you already downloaded. If you do not have it yet, the button below opens the official page of the mod: download GTAVXR.exe there, open it once and come back.
en.NeedsMod=GTAVXR Suite (GTAVXR.exe)
en.NeedsModWhy=Version 1.0.2 was checked: 422 of 422 texts translated. Newer versions are checked when you continue.
en.NeedsFound=found
en.NeedsMissing=not found in the usual places (you can choose it on the next page)
en.NeedsOpenMod=Open the GTAVXR page
en.NeedsAgain=Check again
en.PickTitle=Which GTAVXR to translate
en.PickSubtitle=Choose the GTAVXR.exe you open to play.
en.PickIntro=This is usually the one in your Downloads folder, or the installed one if you used "Install on this PC" in the Suite. The texts are checked before anything changes.
en.PickLabel=GTAVXR.exe:
en.PickFilter=GTAVXR Suite (GTAVXR.exe)|GTAVXR.exe|Programs (*.exe)|*.exe
en.PickMissing=Choose the GTAVXR.exe file.
en.CheckNotSuite=This file is not the GTAVXR Suite. Choose the GTAVXR.exe of the mod.
en.CheckLow=This GTAVXR version has texts this translation does not know yet, so nothing was changed.%n%nCheck for a newer GTAVXR PT-BR at:%n{#AppURL}/releases
en.CheckAlready=This GTAVXR.exe is already translated. Continuing reinstalls the tool and, when this is a newer GTAVXR PT-BR, updates the texts from the kept original.
en.CheckFailed=The file could not be checked.%n%nDetails are in:%n%1
en.SetLanguage=Switch the Suite language to Portuguese (recommended)
en.Running=Close GTAVXR (the Suite window) and click OK.%n%nCancel leaves GTAVXR as it is.
en.ApplyFailed=GTAVXR was not translated, and the original was left as it was.%n%nDetails are in:%n%1
en.Done=Done: GTAVXR is in Portuguese. In the Suite, Settings > Language shows Português and English.
en.OtherCopy=Another GTAVXR.exe is already translated on this computer. Uninstall GTAVXR PT-BR first, which puts that one back, and then translate this one.
en.OpenSuite=Open GTAVXR now
en.InGame=Also translate the menus inside the game: VR menu and Trainer (recommended)
en.InGameFailed=The Suite was translated, but the in-game translation could not be added to GTAVXR.%n%nDetails are in:%n%1
en.InGameDone=%n%nThe menus inside the game: in the VR menu, open Settings and switch Language until the texts are in Portuguese (it takes the place of Spanish).
en.InGameRemoveFailed=The in-game translation could not be taken out of GTAVXR. In the Suite, remove the mod "GTAVXR PT-BR menus do jogo".%n%nDetails are in:%n%1
en.RestoreFailed=The original GTAVXR.exe could not be put back.%n%nDetails are in:%n%1%n%nThe original is kept next to it as GTAVXR.exe.pt-BR.original.
pt.NeedsTitle=O que esta tradução precisa
pt.NeedsSubtitle=O mod GTAVXR, feito por outras pessoas. Este instalador não instala o mod.
pt.NeedsIntro=O GTAVXR PT-BR traduz um GTAVXR que você já baixou. Se ainda não tem, o botão abaixo abre a página oficial do mod: baixe o GTAVXR.exe lá, abra uma vez e volte aqui.
pt.NeedsMod=Suite do GTAVXR (GTAVXR.exe)
pt.NeedsModWhy=A versão 1.0.2 foi conferida: 422 de 422 textos traduzidos. Versões mais novas são conferidas quando você continuar.
pt.NeedsFound=encontrada
pt.NeedsMissing=não encontrada nos lugares de costume (dá para escolher na próxima página)
pt.NeedsOpenMod=Abrir a página do GTAVXR
pt.NeedsAgain=Conferir de novo
pt.PickTitle=Qual GTAVXR traduzir
pt.PickSubtitle=Escolha o GTAVXR.exe que você abre para jogar.
pt.PickIntro=Em geral é o da pasta Downloads, ou o instalado, se você usou "Instalar neste PC" na Suite. Os textos são conferidos antes de qualquer mudança.
pt.PickLabel=GTAVXR.exe:
pt.PickFilter=Suite do GTAVXR (GTAVXR.exe)|GTAVXR.exe|Programas (*.exe)|*.exe
pt.PickMissing=Escolha o arquivo GTAVXR.exe.
pt.CheckNotSuite=Este arquivo não é a Suite do GTAVXR. Escolha o GTAVXR.exe do mod.
pt.CheckLow=Esta versão do GTAVXR tem textos que esta tradução ainda não conhece, então nada foi mudado.%n%nVeja se há um GTAVXR PT-BR mais novo em:%n{#AppURL}/releases
pt.CheckAlready=Este GTAVXR.exe já está traduzido. Continuar reinstala a ferramenta e, quando este GTAVXR PT-BR for mais novo, atualiza os textos a partir da original guardada.
pt.CheckFailed=Não deu para conferir o arquivo.%n%nOs detalhes estão em:%n%1
pt.SetLanguage=Mudar o idioma da Suite para português (recomendado)
pt.Running=Feche o GTAVXR (a janela da Suite) e clique em OK.%n%nCancelar deixa o GTAVXR como está.
pt.ApplyFailed=O GTAVXR não foi traduzido, e o original ficou como estava.%n%nOs detalhes estão em:%n%1
pt.Done=Pronto: o GTAVXR está em português. Na Suite, Ajustes > Idioma mostra Português e English.
pt.OtherCopy=Outro GTAVXR.exe já está traduzido neste computador. Desinstale o GTAVXR PT-BR primeiro, o que devolve aquele, e depois traduza este.
pt.OpenSuite=Abrir o GTAVXR agora
pt.InGame=Traduzir também os menus de dentro do jogo: menu de VR e Trainer (recomendado)
pt.InGameFailed=A Suite foi traduzida, mas a tradução dos menus do jogo não pôde ser adicionada ao GTAVXR.%n%nOs detalhes estão em:%n%1
pt.InGameDone=%n%nOs menus de dentro do jogo: no menu de VR, abra Ajustes e troque o Idioma até os textos ficarem em português (ele ocupa o lugar do espanhol).
pt.InGameRemoveFailed=Não deu para tirar a tradução dos menus do jogo do GTAVXR. Na Suite, remova o mod "GTAVXR PT-BR menus do jogo".%n%nOs detalhes estão em:%n%1
pt.RestoreFailed=Não deu para devolver o GTAVXR.exe original.%n%nOs detalhes estão em:%n%1%n%nO original continua guardado ao lado, como GTAVXR.exe.pt-BR.original.

[Tasks]
Name: "setlanguage"; Description: "{cm:SetLanguage}"
Name: "ingame"; Description: "{cm:InGame}"

[Files]
Source: "{#ToolDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion
Source: "{#InGameDir}\{#InGameZip}"; DestDir: "{app}\in-game"; Flags: ignoreversion

[Run]
Filename: "{code:SuiteExe}"; Description: "{cm:OpenSuite}"; Flags: nowait postinstall skipifsilent; Check: Translated

[Code]
const
  ExitOk = 0;
  ExitCoverage = 3;
  ExitAlready = 4;
  ExitRunning = 5;
  ExitNothing = 6;
  ExitNotSuite = 7;
  ExitOtherCopy = 8;

var
  NeedsPage: TWizardPage;
  PickPage: TInputFileWizardPage;
  ModStatus: TNewStaticText;
  WasTranslated: Boolean;
  InGameAdded: Boolean;

function LogPath(): String;
begin
  Result := ExpandConstant('{localappdata}\GTAVXR-PTBR\last-run.log');
end;

{ The same places the tool looks at (Program.Candidates). }
function FindSuite(): String;
var
  Places: array[0..2] of String;
  I: Integer;
begin
  Places[0] := ExpandConstant('{localappdata}\Programs\GTAVXR\GTAVXR.exe');
  Places[1] := ExpandConstant('{%USERPROFILE}\Downloads\GTAVXR.exe');
  Places[2] := ExpandConstant('{userdesktop}\GTAVXR.exe');
  Result := '';
  for I := 0 to 2 do
    if FileExists(Places[I]) then
    begin
      Result := Places[I];
      exit;
    end;
end;

function SuiteExe(Param: String): String;
begin
  Result := PickPage.Values[0];
end;

function Translated(): Boolean;
begin
  Result := WasTranslated;
end;

procedure RefreshNeeds();
begin
  if FindSuite() <> '' then
    ModStatus.Caption := CustomMessage('NeedsMod') + ': ' + CustomMessage('NeedsFound')
  else
    ModStatus.Caption := CustomMessage('NeedsMod') + ': ' + CustomMessage('NeedsMissing');
end;

procedure ModClick(Sender: TObject);
var
  Code: Integer;
begin
  ShellExec('open', '{#ModURL}', '', '', SW_SHOWNORMAL, ewNoWait, Code);
end;

procedure AgainClick(Sender: TObject);
begin
  RefreshNeeds();
  if (PickPage.Values[0] = '') then
    PickPage.Values[0] := FindSuite();
end;

function AddText(Top: Integer; const Caption: String; Bold: Boolean): TNewStaticText;
begin
  Result := TNewStaticText.Create(NeedsPage);
  Result.Parent := NeedsPage.Surface;
  Result.AutoSize := False;
  Result.WordWrap := True;
  Result.Top := Top;
  Result.Width := NeedsPage.SurfaceWidth;
  if Bold then
    Result.Font.Style := [fsBold];
  Result.Caption := Caption;
  Result.AdjustHeight();
end;

function AddButton(Top, Left: Integer; const Caption: String; OnClick: TNotifyEvent): TNewButton;
begin
  Result := TNewButton.Create(NeedsPage);
  Result.Parent := NeedsPage.Surface;
  Result.Caption := Caption;
  Result.Top := Top;
  Result.Left := Left;
  Result.Width := ScaleX(230);
  Result.Height := WizardForm.NextButton.Height;
  Result.OnClick := OnClick;
end;

procedure InitializeWizard();
var
  Last: TControl;
begin
  NeedsPage := CreateCustomPage(wpWelcome, CustomMessage('NeedsTitle'), CustomMessage('NeedsSubtitle'));
  Last := AddText(0, CustomMessage('NeedsIntro'), False);
  ModStatus := AddText(Last.Top + Last.Height + ScaleY(14), CustomMessage('NeedsMod'), True);
  Last := AddText(ModStatus.Top + ModStatus.Height + ScaleY(2), CustomMessage('NeedsModWhy'), False);
  Last := AddButton(Last.Top + Last.Height + ScaleY(6), 0, CustomMessage('NeedsOpenMod'), @ModClick);
  AddButton(Last.Top, Last.Left + Last.Width + ScaleX(8), CustomMessage('NeedsAgain'), @AgainClick).Width := ScaleX(150);

  PickPage := CreateInputFilePage(NeedsPage.ID, CustomMessage('PickTitle'), CustomMessage('PickSubtitle'), CustomMessage('PickIntro'));
  PickPage.Add(CustomMessage('PickLabel'), CustomMessage('PickFilter'), '.exe');
  { /SUITE="path" chooses the copy to translate (also for silent installs). }
  PickPage.Values[0] := ExpandConstant('{param:SUITE|}');
  if PickPage.Values[0] = '' then
    PickPage.Values[0] := FindSuite();
  RefreshNeeds();
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = NeedsPage.ID then
    RefreshNeeds()
  else if (CurPageID = wpFinished) and WasTranslated then
  begin
    if InGameAdded then
      WizardForm.FinishedLabel.Caption := CustomMessage('Done') + CustomMessage('InGameDone')
    else
      WizardForm.FinishedLabel.Caption := CustomMessage('Done');
    WizardForm.FinishedLabel.AdjustHeight();
  end;
end;

{ Runs the tool that was extracted to the temporary folder (wizard time) or installed (after copying files). }
function RunTool(const Folder, Params: String): Integer;
begin
  if not Exec(AddBackslash(Folder) + '{#ToolExe}', Params, Folder, SW_HIDE, ewWaitUntilTerminated, Result) then
    Result := -1;
end;

{ Where ExtractTemporaryFiles left the tool: the temporary folder, keeping the folders after the constant. }
function ExtractedTool(): String;
begin
  Result := ExpandConstant('{tmp}') + '\{app}';
  if not FileExists(Result + '\{#ToolExe}') then
    Result := ExpandConstant('{tmp}');
  Log('Tool extracted to ' + Result);
end;

{ Before anything is copied: is the chosen file a GTAVXR Suite this translation covers? }
function CheckChoice(): Boolean;
var
  Code: Integer;
begin
  Result := False;
  if (PickPage.Values[0] = '') or not FileExists(PickPage.Values[0]) then
  begin
    { A silent install without GTAVXR only installs the tool (also the clean machine of the CI). }
    if WizardSilent() then
    begin
      Result := True;
      exit;
    end;
    MsgBox(CustomMessage('PickMissing'), mbError, MB_OK);
    exit;
  end;
  ExtractTemporaryFiles('{app}\*');
  Code := RunTool(ExtractedTool(), 'check --exe "' + PickPage.Values[0] + '"');
  case Code of
    ExitOk: Result := True;
    ExitAlready:
      begin
        SuppressibleMsgBox(CustomMessage('CheckAlready'), mbInformation, MB_OK, IDOK);
        Result := True;
      end;
    ExitCoverage: SuppressibleMsgBox(CustomMessage('CheckLow'), mbError, MB_OK, IDOK);
    ExitNotSuite: SuppressibleMsgBox(CustomMessage('CheckNotSuite'), mbError, MB_OK, IDOK);
  else
    SuppressibleMsgBox(FmtMessage(CustomMessage('CheckFailed'), [LogPath()]), mbError, MB_OK, IDOK);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = PickPage.ID then
    Result := CheckChoice();
end;

{ The in-game menus: the Suite imports our script mod and it is turned on in the profiles. }
procedure AddInGame();
var
  Code: Integer;
begin
  repeat
    Code := RunTool(ExpandConstant('{app}'), 'ingame-add --exe "' + PickPage.Values[0] + '" --mod "' +
      ExpandConstant('{app}\in-game\{#InGameZip}') + '"');
    if Code <> ExitRunning then
      break;
  until SuppressibleMsgBox(CustomMessage('Running'), mbInformation, MB_OKCANCEL, IDCANCEL) = IDCANCEL;
  InGameAdded := Code = ExitOk;
  if InGameAdded then
    Log('In-game translation added')
  else if Code <> ExitRunning then
    SuppressibleMsgBox(FmtMessage(CustomMessage('InGameFailed'), [LogPath()]), mbError, MB_OK, IDOK);
end;

{ After the tool is copied: translate, asking to close the Suite while it is open. }
procedure CurStepChanged(CurStep: TSetupStep);
var
  Code: Integer;
  Params: String;
begin
  if CurStep <> ssPostInstall then
    exit;
  { A silent install without /SUITE on a computer without GTAVXR only installs the tool. }
  if (PickPage.Values[0] = '') or not FileExists(PickPage.Values[0]) then
  begin
    Log('No GTAVXR.exe chosen: nothing translated');
    exit;
  end;
  Params := 'apply --exe "' + PickPage.Values[0] + '"';
  if WizardIsTaskSelected('setlanguage') then
    Params := Params + ' --set-language';
  repeat
    Code := RunTool(ExpandConstant('{app}'), Params);
    if Code <> ExitRunning then
      break;
  until SuppressibleMsgBox(CustomMessage('Running'), mbInformation, MB_OKCANCEL, IDCANCEL) = IDCANCEL;
  WasTranslated := (Code = ExitOk) or (Code = ExitAlready);
  if WasTranslated then
    Log('GTAVXR translated: ' + PickPage.Values[0])
  else if Code = ExitOtherCopy then
    SuppressibleMsgBox(CustomMessage('OtherCopy'), mbError, MB_OK, IDOK)
  else if Code <> ExitRunning then
    SuppressibleMsgBox(FmtMessage(CustomMessage('ApplyFailed'), [LogPath()]), mbError, MB_OK, IDOK);
  if WasTranslated and WizardIsTaskSelected('ingame') then
    AddInGame();
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := CustomMessage('PickLabel') + NewLine + Space + PickPage.Values[0];
  if MemoTasksInfo <> '' then
    Result := Result + NewLine + NewLine + MemoTasksInfo;
end;

{ Before anything is removed: GTAVXR must be closed. Cancel stops the uninstall, so the tool that
  can put the original back stays installed. }
function InitializeUninstall(): Boolean;
begin
  Result := True;
  while RunTool(ExpandConstant('{app}'), 'busy') = ExitRunning do
    if SuppressibleMsgBox(CustomMessage('Running'), mbInformation, MB_OKCANCEL, IDCANCEL) = IDCANCEL then
    begin
      Result := False;
      exit;
    end;
end;

{ Uninstalling takes the in-game translation out of GTAVXR and puts the original GTAVXR.exe back,
  before the tool itself goes away. }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Code: Integer;
begin
  if CurUninstallStep <> usUninstall then
    exit;
  { GTAVXR may have been opened again since the uninstall started: ask once more to close it. }
  repeat
    Code := RunTool(ExpandConstant('{app}'), 'ingame-remove');
    if Code <> ExitRunning then
      break;
  until SuppressibleMsgBox(CustomMessage('Running'), mbInformation, MB_OKCANCEL, IDCANCEL) = IDCANCEL;
  if (Code <> ExitOk) and (Code <> ExitNothing) then
    SuppressibleMsgBox(FmtMessage(CustomMessage('InGameRemoveFailed'), [LogPath()]), mbError, MB_OK, IDOK);
  Code := RunTool(ExpandConstant('{app}'), 'restore');
  if (Code <> ExitOk) and (Code <> ExitNothing) then
    SuppressibleMsgBox(FmtMessage(CustomMessage('RestoreFailed'), [LogPath()]), mbError, MB_OK, IDOK);
end;
