# Trabalho Prático 01 - Técnicas de Desenvolvimento de Videojogos
## Análise de Implementação: Platformer 2D (MonoGame)

**Grupo:**
* Diogo Fernandes - 34988
* Tiago Martins - 34986
* Vitor Ferreira - 31488

---

## 1. Descrição do Jogo
O projeto escolhido é o **Platformer 2D**, um "Starter Kit" oficial da equipa do MonoGame. Trata-se de um jogo de plataformas clássico onde o jogador controla uma personagem que deve navegar por níveis, recolher gemas, evitar inimigos e alcançar a saída dentro de um tempo limite. O jogo serve como demonstração de físicas básicas (gravidade e saltos), animação de sprites e gestão de estados de jogo.

## 2. Instruções de Instalação e Execução
Para compilar e correr este projeto, é necessário ter instalado o ambiente de desenvolvimento **Visual Studio** com a extensão do MonoGame ou o **.NET SDK** instalado.

1. Clone o repositório para a sua máquina local.
2. Abra o ficheiro de solução (`.sln`) no Visual Studio.
3. Certifique-se de que o **MonoGame Content Builder (MGCB) Editor** está instalado para processar os assets.
4. Compile (Build) e execute (F5) o projeto.

**Controlos:**
* **Mover:** Teclas `A` e `D` ou Setas.
* **Saltar:** Tecla `Espaço`, `W` ou Seta Cima.

## 3. Organização do Projeto e Pastas
O projeto apresenta uma estrutura organizada e modular, seguindo as convenções padrão do ecossistema MonoGame:

* **Pasta Raiz:** Contém os ficheiros de código fonte (`.cs`) e a definição do projeto.
* **Pasta `Content/`:** É o coração dos assets do jogo. Utiliza o Content Pipeline do MonoGame para converter imagens (PNG), sons (WAV) e fontes em ficheiros `.xnb` otimizados.
    * `Backgrounds/`: Camadas de fundo para efeito de parallax.
    * `Sprites/`: Folhas de sprites (Player, Enemy, Gem).
    * `Sounds/`: Efeitos sonoros e música.
    * `Fonts/`: Ficheiros de definição de texto para o HUD.
* **Organização do Código:** Os ficheiros estão nomeados de acordo com a classe que representam (ex: `Player.cs`, `Level.cs`), o que facilita a navegação e manutenção do código.

## 4. Análise da Arquitetura e Implementação
A lógica do jogo assenta no ciclo de vida fundamental do MonoGame: **Initialize -> LoadContent -> Update -> Draw**.

### Ciclo Principal (PlatformerGame.cs)
A classe principal gere o estado global do jogo. É responsável por carregar os níveis e alternar entre o estado de jogo ativo, vitória ou derrota. O método `Update` coordena a sua lógica temporal, enquanto o `Draw` renderiza o nível e a interface de utilizador (HUD).

### Gestão de Níveis (Level.cs)
A classe `Level` é central na arquitetura. Ela lê ficheiros de texto para carregar o mapa. Cada caractere no ficheiro `.txt` é mapeado para um tipo de `Tile` (Passável, Impassável ou Plataforma). Esta classe também gere a lista de entidades ativas (inimigos e gemas).

### Física e Colisões (Player.cs)
A movimentação do jogador utiliza vetores de velocidade e aceleração.
* **Gravidade:** Aplicada constantemente no eixo Y quando o jogador não está sobre uma superfície sólida.
* **Colisões:** O jogo utiliza *AABB (Axis-Aligned Bounding Boxes)*. O código verifica os tiles adjacentes à posição do jogador para impedir a passagem por paredes ou para permitir que o jogador "pouse" em plataformas.

### Animações (AnimationPlayer.cs)
O sistema de animação é separado. A estrutura `Animation` guarda os dados da textura, enquanto a `AnimationPlayer` gere o tempo de cada frame e a origem da origem da renderização (Flip horizontal para mudar de direção).


