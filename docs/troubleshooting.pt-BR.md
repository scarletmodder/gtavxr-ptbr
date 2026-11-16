[English](troubleshooting.md) | **Português**

# Problemas comuns

Cada mensagem do instalador, por que aparece e o que fazer. O que a última execução fez fica em `%LOCALAPPDATA%\GTAVXR-PTBR\last-run.log`; mande esse arquivo junto quando [pedir ajuda](https://github.com/scarletmodder/gtavxr-ptbr/issues). Ele não tem dado pessoal além do caminho do seu `GTAVXR.exe`.

## "Suite do GTAVXR (GTAVXR.exe): não encontrada nos lugares de costume"

O instalador procurou na pasta Downloads, na Área de Trabalho e na pasta onde a Suite se instala. A sua cópia está em outro lugar: escolha ela na página seguinte com **Procurar**. Se você ainda não tem o GTAVXR, **Abrir a página do GTAVXR** abre a página oficial de download.

## "Este arquivo não é a Suite do GTAVXR"

O arquivo escolhido não é o programa com o botão Jogar do GTAVXR. Escolha o `GTAVXR.exe` que você abre para jogar. Um arquivo que não é a Suite (outro mod, um instalador, um atalho) é recusado antes de qualquer mudança.

## "Esta versão do GTAVXR tem textos que esta tradução ainda não conhece"

O GTAVXR foi atualizado e menos de 80% dos textos são conhecidos. Nada foi mudado. Veja nas [versões](https://github.com/scarletmodder/gtavxr-ptbr/releases) se há um GTAVXR PT-BR mais novo, ou [abra uma issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) com a versão do GTAVXR.

## "Este GTAVXR.exe já está traduzido"

Ele já foi traduzido antes. Continuar reinstala a ferramenta; se este GTAVXR PT-BR for mais novo que o que traduziu, os textos são atualizados a partir da original guardada.

## "Outro GTAVXR.exe já está traduzido neste computador"

Só uma cópia fica traduzida por vez, para a desinstalação sempre conseguir devolver. Desinstale o GTAVXR PT-BR (a outra cópia recebe a original de volta) e rode o instalador de novo para esta cópia.

## "Feche o GTAVXR (a janela da Suite) e clique em OK"

O arquivo não pode ser mudado enquanto está aberto. Feche a janela do GTAVXR e clique em **OK**. **Cancelar** deixa o GTAVXR como está: na instalação, a ferramenta fica instalada mas nada é traduzido (rode o instalador de novo depois); na desinstalação, nada é removido.

## "O GTAVXR não foi traduzido, e o original ficou como estava"

Algo falhou ao gravar a cópia traduzida: em geral um arquivo travado, ou uma pasta sem permissão de escrita (por exemplo uma cópia dentro de `C:\Program Files`). O original não mudou. O log diz qual passo falhou. Mover o `GTAVXR.exe` para uma pasta sua, como Downloads, resolve o caso de permissão.

## "Não deu para devolver o GTAVXR.exe original"

Aparece no desinstalador. O original continua guardado ao lado da sua cópia, como `GTAVXR.exe.pt-BR.original`: feche o GTAVXR, apague o `GTAVXR.exe` e renomeie o arquivo guardado para `GTAVXR.exe`. Ou baixe o GTAVXR de novo pela página oficial.

## A Suite continua em inglês

A opção de mudar o idioma estava desmarcada, ou a Suite foi posta em inglês depois. Na Suite, abra **Ajustes** e clique em **Português**.

## Alguns textos estão em espanhol

São textos que esta versão da tradução ainda não conhece, em geral porque o GTAVXR foi atualizado. [Abra uma issue](https://github.com/scarletmodder/gtavxr-ptbr/issues) com um print.

## Os menus de dentro do jogo estão em inglês ou espanhol

Esperado por enquanto: o menu de VR e o Trainer do jogo não podem ser traduzidos por fora sem impedir o GTAVXR de abrir, então a tradução deles foi oferecida aos autores do GTAVXR. Veja [Limitações conhecidas](../README.pt-BR.md#%EF%B8%8F-limitações-conhecidas).

## Depois de traduzir, o Jogar demora mais uma vez

O GTAVXR reconstruiu os mods de conteúdo que você usa (World of Variety e outros), porque as descrições deles mudaram de idioma. Acontece uma vez.

## O Windows diz que o instalador pode ser inseguro

O instalador não tem assinatura digital. Escolha **Mais informações** e **Executar assim mesmo**. Dá para comparar o arquivo com o `SHA256SUMS.txt` da versão:

```bash
certutil -hashfile GTAVXR-PTBR-Setup-v1.0.0.exe SHA256
```
