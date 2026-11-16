[English](translation-guide.md) | **Português**

# Guia de tradução

As regras e os termos fixos usados em todos os arquivos de [`translations/`](../translations). Se uma palavra soar errada no jogo, [abra uma issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) com o `id` da linha, o texto atual e a sua sugestão.

## Os arquivos

| Arquivo | O que é | Textos |
|---|---|---|
| `suite.pt-BR.json` | A janela da Suite (aplicada pelo instalador) | 422 |
| `content-catalog.pt-BR.json` | Nomes e descrições dos mods de conteúdo na Suite (aplicados pelo instalador) | 101 textos únicos, 129 lugares |
| `vr-menu.pt-BR.json` | O menu de VR dentro do jogo (oferecido ao GTAVXR, não aplicado) | 308 |
| `trainer.pt-BR.json` | O Trainer dentro do jogo (oferecido ao GTAVXR, não aplicado) | 776 |

Cada linha é `{"id", "en", "pt"}`: o identificador no código do GTAVXR, o texto em inglês e o texto em português. Os testes conferem que cada linha mantém os mesmos marcadores do inglês, não está vazia e não tem travessão.

## Regras

- Português do Brasil natural, como um jogo localizado no Brasil. A pessoa é tratada por "você".
- Traduzido a partir do inglês; o espanhol só tira dúvida de sentido.
- Sem travessão nem meia-risca: vírgula, dois-pontos ou parênteses.
- Os marcadores ficam exatamente como estão: `{0}`, `%s`, `%d`, `%.1f`, `%%`, `~r~`, `~s~`, `\n`, `>`, `<`, `·`, aspas e os espaços do início e do fim.
- A caixa segue a do inglês: TUDO MAIÚSCULO continua maiúsculo; Title Case vira caixa de frase, como é comum em português.
- Tamanho: os menus do jogo têm largura fixa. Ficar perto do tamanho do inglês; acima de 1,3 vez o maior entre inglês e espanhol, abreviar com naturalidade.
- Não se traduz: nomes próprios (Michael, Los Santos, nomes de mods, lojas, missões), marcas de veículos e armas, siglas (HUD, VR, OpenXR, DLSS, FSR, FPS, DLC), unidades.

## Termos fixos

| Inglês | Português |
|---|---|
| headset | óculos VR (rótulo curto: Óculos) |
| controllers | controles |
| grip | botão de agarrar (curto: Agarrar) |
| trigger | gatilho |
| thumbstick | analógico |
| recenter | recentralizar |
| snap turning / smooth turning | giro por passos / giro suave |
| haptics, vibration | vibração |
| profile / save / story | perfil / jogo salvo (curto: Save) / modo história |
| settings | configurações (curto, em menus: Ajustes) |
| wanted level | nível de procurado |
| god mode / never wanted | modo deus / nunca procurado |
| waypoint | ponto marcado no mapa (curto: Marcador) |
| teleport / spawn | teleportar / gerar |
| ped | pedestre, pessoa |
| loadout | equipamento |
| toggle / reset | opção liga/desliga / redefinir |
| frame generation / frame | geração de quadros / quadro |
| library / stable / test version | biblioteca / estável / versão de teste |
| Trainer, Suite, runtime | mantidos em inglês |
| ON / OFF | mantidos como ON e OFF |
