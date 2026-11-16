[English](usage.md) | **Português**

# Uso

## Antes de começar

- O GTAVXR está baixado: o `GTAVXR.exe` da [página oficial](https://github.com/LVL-VR/GTAVXR/releases), aberto pelo menos uma vez.
- O GTAVXR está fechado.

## O instalador, página por página

1. **Boas-vindas.** O que o instalador faz: traduz a cópia do GTAVXR que você já tem e guarda a original ao lado.

   <img src="images/installer-pt-BR-welcome.png" alt="Página de boas-vindas do instalador" width="560">

2. **O que esta tradução precisa.** Mostra se a Suite do GTAVXR foi **encontrada** nos lugares de costume: a pasta Downloads, a Área de Trabalho, ou a pasta onde a Suite se instala quando você usa **Instalar neste PC** nos ajustes dela. **Abrir a página do GTAVXR** abre a página oficial de download. **Conferir de novo** procura outra vez, depois que você baixar.

   <img src="images/installer-pt-BR-needs.png" alt="Página que mostra se o GTAVXR foi encontrado" width="560">

3. **Qual GTAVXR traduzir.** O `GTAVXR.exe` que você abre para jogar. **Procurar** deixa escolher outro. Quando você clica em **Avançar**, os textos desse arquivo são conferidos; nada muda ainda.

   <img src="images/installer-pt-BR-pick.png" alt="Página para escolher o arquivo GTAVXR.exe" width="560">

4. **Tarefas adicionais.** **Mudar o idioma da Suite para português** vem marcado. Desmarcado, a tradução é aplicada mas a Suite abre no idioma que tinha; dá para trocar depois em **Ajustes**.

   <img src="images/installer-pt-BR-tasks.png" alt="Página com a opção de mudar o idioma da Suite para português" width="560">

5. **Pronto para instalar.** Um resumo do arquivo e da opção. **Instalar** copia a ferramenta pequena e traduz o arquivo.

   <img src="images/installer-pt-BR-ready.png" alt="Resumo antes de instalar" width="560">

6. **Concluir.** Diz que o GTAVXR está em português. **Abrir o GTAVXR agora** abre a Suite traduzida.

   <img src="images/installer-pt-BR-finish.png" alt="Última página do instalador" width="560">

## O que muda no seu computador

| Onde | O quê |
|---|---|
| O seu `GTAVXR.exe` | Os textos em espanhol viram português; o inglês fica como está |
| `GTAVXR.exe.pt-BR.original`, ao lado | O arquivo original, intacto |
| O `settings.json` da Suite (`%LOCALAPPDATA%\GTAVXR\Suite`, ou a pasta de dados escolhida na Suite) | `"language"` na coluna em português, só com a opção marcada |
| `%LOCALAPPDATA%\Programs\GTAVXR PT-BR` | A ferramenta, as traduções e o desinstalador |
| `%LOCALAPPDATA%\GTAVXR-PTBR` | `state.json` (o que desfazer) e `last-run.log` (o que a última execução fez) |

## Quando o GTAVXR atualizar

Um `GTAVXR.exe` novo vem só em inglês e espanhol. Rode de novo o instalador do GTAVXR PT-BR e escolha o arquivo novo.

- Se você instalou um GTAVXR PT-BR mais novo, a mesma execução já traz os textos novos dele.
- Se a versão nova tiver os mesmos textos, ela é traduzida como antes.
- Se pelo menos 80% dos textos forem conhecidos, ela é traduzida, e os textos novos ficam em espanhol até sair um GTAVXR PT-BR novo.
- Abaixo disso, o instalador avisa que a versão ainda não é coberta e não muda nada.

Se a Suite se atualizou no mesmo lugar, a cópia de segurança da versão antiga não corresponde mais a ela; a desinstalação então deixa o arquivo novo como está.

## Desinstalar

Configurações do Windows, **Aplicativos**, **Aplicativos instalados**, **GTAVXR PT-BR**, **Desinstalar**. O desinstalador:

1. devolve o `GTAVXR.exe` original, se a sua cópia ainda for a traduzida;
2. volta o idioma da Suite ao que era antes, se ele ainda estiver na coluna em português;
3. remove a ferramenta e o arquivo de estado (a pasta `%LOCALAPPDATA%\GTAVXR-PTBR` fica só com o `last-run.log`).

Se o GTAVXR estiver aberto, ele pede para fechar antes; **Cancelar** interrompe a desinstalação sem mudar nada. Se o seu `GTAVXR.exe` foi movido ou apagado, a original continua onde foi guardada, `GTAVXR.exe.pt-BR.original`.

Só uma cópia do `GTAVXR.exe` fica traduzida por vez. Para traduzir outra cópia, desinstale antes. Um GTAVXR PT-BR mais novo instalado por cima de um mais antigo atualiza os textos da cópia que ele traduziu.

## Linha de comando

A ferramenta instalada pode ser usada direto de `%LOCALAPPDATA%\Programs\GTAVXR PT-BR`:

```bash
GtavxrPtbr find
```

```bash
GtavxrPtbr check --exe "C:\caminho\para\GTAVXR.exe"
```

```bash
GtavxrPtbr apply --exe "C:\caminho\para\GTAVXR.exe" --set-language
```

```bash
GtavxrPtbr status
```

```bash
GtavxrPtbr restore
```

| Código de saída | Significado |
|---|---|
| 0 | Feito |
| 1 | Erro (a mensagem diz qual) |
| 2 | Comando ou opção errada |
| 3 | Esta versão do GTAVXR ainda não é coberta (menos de 80% dos textos conhecidos) |
| 4 | Já traduzido |
| 5 | O GTAVXR está aberto |
| 6 | Nada para devolver |
| 7 | O arquivo não é a Suite do GTAVXR |
| 8 | Outra cópia já está traduzida (desinstale antes) |

O instalador também aceita `/SUITE="C:\caminho\para\GTAVXR.exe"` para escolher o arquivo, o que permite instalar em silêncio:

```bash
GTAVXR-PTBR-Setup-v1.0.0.exe /VERYSILENT /SUITE="C:\Games\GTAVXR\GTAVXR.exe"
```
