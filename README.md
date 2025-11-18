## 📄 Metadados do Projeto

| Campo | Detalhe |
| :--- | :--- |
| **Nome do Projeto** | Megabonk-like |
| **Autor(a)** | Paulo Ricardo (Miku-chan) |
| **Data de Criação** | 17 de Novembro de 2025 |
| **Foco Principal** | Teste técnico com ênfase de arquitetura e design |

# 📜 README

Este projeto é uma demonstração técnica de um jogo no estilo "horde survival" (semelhante a Vampire Survivors ou megabonk), focado na utilização de padrões de projeto, separação de responsabilidades e gerenciamento de performance em um ambiente com grande número de entidades.

Abaixo, seguem seções de apresentações das partes e aspectos técnicos.
Recomendo também a leitura do diario.txt, presente na mesma pasta.

Sinto falta de efeitos, feedbacks generalizados, um visual melhor (até selecionei um asset 3D de fantasia, mas não tive tempo para aplicação), e um tratamento melhor para as UI's.

# 🚀 Arquitetura e Padrões de Projeto

O código-base segue uma arquitetura focada na separação clara entre a lógica de negócios e a camada de visualização/interação da Unity, utilizando o padrão POCO/MonoBehaviour.
Classes Monobehaviour no geral atuam como pontes entre os dados em POCO, e a lógica da engine.

## ♻ POCO (Plain Old C# Object) / MonoBehaviour

A lógica central do jogo é encapsulada em classes POCO, que são classes C# puras, não derivam de MonoBehaviour e não dependem diretamente do ciclo de vida da Unity.

    Classes POCO de Dados e Lógica:

    Estado do Jogo: PlayerStats, WeaponData, EnemyData, SpawnGroupTracker.

    Comportamento: IWeaponEffect (e implementações como SlashAttack, NoneEffect), IPursuit (e implementações como SimplePursuit), IItemModifier (e implementações como HermesSpeedModifier).

    Curvas/Evolução: A lógica de evolução por nível do jogador e das armas é extraída de AnimationCurves configuradas em ScriptableObjects (CurveScriptable, WeaponBlueprint), mas a avaliação e aplicação dos valores são feitas nas classes POCO (PlayerStats, WeaponData).

## 🔷 Classes MonoBehaviour (Controladores/Orquestradores/Pontes):

    Controladores: PlayerController, EnemyController, PlayerWeapon, PlayerItems. Atuam como pontes entre os dados/lógica POCO e o mundo da Unity (componentes, transformações, entradas).

    Sistema de Eventos: O EventBus utiliza structs como payloads (OnEnemyDeathEvent, OnPlayerLevelUp, etc.) para desacoplar ainda mais as classes, permitindo que os controladores POCO publiquem eventos que são consumidos por outros controladores MonoBehaviour (como a UI ou o EnemyManager).

    Pooling: O MonoBehaviourPool<T> gerencia a reutilização de EnemyControllers para otimizar a performance.


---

# 🏗️ Estrutura e Padrões de Design

Algumas exemplificações:

| Padrão | Classes Principais | Aplicação |
| :--- | :--- | :--- |
| **Strategy** | `IPursuit`, `SimplePursuit`, `PursuitStrategy` | Permite trocar o algoritmo de movimento e perseguição dos inimigos dinamicamente. |
| **Strategy** | `IWeaponEffect`, `SlashAttack`, `BaseWeaponEffectScriptable` | Permite trocar o efeito e o comportamento de ataque das armas. |
| **Observer/Event Bus** | `EventBus`, `OnEnemyDeathEvent`, `PlayerController` | Desacopla a produção de eventos (ex: morte do inimigo) do seu consumo (ex: ganho de XP pelo jogador). |
| **Object Pool** | `MonoBehaviourPool<T>`, `IPoolable`, `EnemyController` | Reutiliza objetos (`EnemyController`) para otimizar a performance, especialmente em jogos *megabonk-like* com muitos inimigos. |
| **Data/Logic Separation (POCO/SO)**| `EnemyBlueprint` / `EnemyData` / `EnemyController` | Separa dados configuráveis (`Blueprint`) de dados de *runtime* (`Data`) e da Monobehaviour (`Controller`). |

---

## 👩‍💻 Sistemas

Abaixo, detalho alguns dos sistemas desenvolvidos.

### 👾 Sistema de Inimigos (Enemies)

O sistema de inimigos é um componente central e altamente estruturado, projetado para lidar com um grande volume de entidades ativas, utilizando padrões como Data-Driven, Pool de Objetos e Strategy Pattern para comportamento.

Um problema facilmente identificável é o uso do enum EnemyType para identificar os inimigos.
O enum torna difícil e lento adicionar novos tipos de inimigos, pois é tipado e compilável.

Uma proposta de solução seria utilizar um objeto com um mapeamento único.
Uma simples string única para garantir que a identificação seja consistente entre diversos componentes.

A ideia seria trocar o enum por um ScriptableObject e o uso de addressables que fará:
- Mapeamento: O ScriptableObject terá uma lista onde cada entrada define um inimigo.
- ID Único: Essa lista fornecerá o ID do inimigo e, ao mesmo tempo, será o ID de Addressable (o identificador para carregar o asset).
- Resultado: Todos os inimigos terão um único ID consistente em todo o projeto, tornando a adição de novos inimigos muito mais simples, rápida e coesa.

Isso permitira uma adição de inimigos apenas enquanto conteúdo de addressables, pois uma futura atualização do jogo
implicaria apenas em rebuildar os addressables e publica-los.

Em resumo, essa seria uma melhoria futura para substituirmos o enum rígido por um conjuntos de dados centralizada e flexível.

#### 🎲 Dados e Configuração (Data-Driven)

O sistema separa a definição do inimigo de sua instância em tempo de execução:

    EnemyType.cs: Um enum que define os tipos de inimigos (ex: Skeleton, Minotaur, Harpy).

    EnemyBlueprint.cs: Um ScriptableObject (configuração editável no Unity Editor) que atua como o molde do inimigo. Ele define:

        EnemyType, EnemyName.

        BaseHealth, BaseDamage, e ExpOnDeath.

        Referências para a movimentação: MovementSettings e PursuitStrategy.

    EnemyData.cs: Uma classe POCO (Plain Old C# Object) que armazena o estado mutável (variável) de um inimigo individual em tempo de execução. É inicializada com um EnemyBlueprint e armazena:

        CurrentHealth, CurrentSpeed, Position.

        Flags de estado, como IsAvoiding, IsDying e IsKnockingBack.

#### 🛀 Gerenciamento e Ciclo de Vida (Pooling)

O gerenciamento de inimigos é feito para otimizar a performance, o que é crucial em jogos tipo Megabonk-like com centenas de entidades:

    EnemyManager.cs: O orquestrador que gerencia todos os inimigos ativos.

        Pooling: Utiliza o pool genérico MonoBehaviourPool<EnemyController> para reciclar instâncias de inimigos, em vez de destruí-los e instanciá-los a cada vez, minimizando o Garbage Collection (GC).

        Atualização Distribuída: Para evitar picos de desempenho no FixedUpdate do Unity, o Manager atualiza o movimento/IA de um número limitado de inimigos por frame (enemiesPerFrame), espalhando a carga de trabalho.

        Morte do Inimigo: Quando um inimigo morre, o EnemyController é retornado ao pool, e o EnemyManager publica o evento OnEnemyDeathEvent através do EventBus.cs, que é consumido pelo PlayerController para conceder a EXP (TotalExperience).

    EnemyController.cs: O MonoBehaviour (componente Unity) do inimigo.

        É a ponte entre os dados (EnemyData) e a engine (transform, HealthComponent).

        Implementa IPoolable para ser gerenciado pelo EnemyManager.

        Lida com o efeito de Knockback usando uma coroutine (PerformKnockback) com interpolação (ease-out cubic) para um movimento suave.

#### 🧠 Comportamento e Movimentação (Strategy Pattern)

O movimento do inimigo é isolado em classes de comportamento usando o Strategy Pattern:

    PursuitStrategy.cs / SimplePursuitStrategy.cs: ScriptableObjects que fornecem a implementação do comportamento de perseguição/movimento (implementa IPursuit).

    SimplePursuit.cs: A implementação principal da lógica de perseguição, que calcula o vetor de movimento:

        Perseguição ao Alvo: Calcula a direção em relação ao alvo (targetPosition).

        Avoidance/Separação: Usa Physics.OverlapSphereNonAlloc para detectar e calcular um vetor de afastamento de inimigos próximos dentro de um separationRadius, evitando o empilhamento.

        Parada Mínima: Verifica a minStopDistance para interromper a perseguição quando o inimigo está muito próximo do alvo.

    MovementSettings.cs: Define os parâmetros de física/movimento utilizados pelo EnemyController para aplicar força ao seu Rigidbody.

#### 🌊 Sistema de Ondas e Spawning

O aparecimento de inimigos é controlado pelo sistema de ondas:

    WavesScriptable.cs / WaveSetup.cs: ScriptableObjects que contêm a lista de ondas, cada uma com:

        Uma lista de SpawnGroup (que inimigo, quantidade máxima, e tempo de spawn).

        Limite de inimigos ativos simultâneos (MaxEnemies).

        Duração total da onda (TotalTime).

    WaveManager.cs: Gerencia o progresso das ondas.

        Utiliza SpawnGroupTracker para rastrear o progresso de spawn de cada grupo de inimigos na onda atual.

        O spawn é configurado por uma AnimationCurve (SpawnOverTime dentro de SpawnGroup), o que permite um controle preciso sobre a frequência de spawn ao longo do tempo da onda (ex: mais intenso no meio, ou no final).

    CircleSpawn.cs: Implementa a interface ISpawner, calculando posições de spawn aleatórias em um raio (minSpawnRadius, maxSpawnRadius) ao redor do jogador, com uma área de exclusão (exclusionAngle) na frente do jogador para evitar spawns injustos no campo de visão.

### ⚔️ Sistema de Armas (Weaponry)

O sistema de armas é estruturado em torno de três conceitos principais: Blueprints, Dados (Data) e Efeitos (Effects), seguindo um padrão semelhante ao sistema de inimigos.

#### 🧱 Blueprints e Dados

    WeaponBlueprint.cs (ScriptableObject):

        Define as características base da arma: nome, ícone, dano base (BaseDamage), cooldown base (BaseCooldown), quantidade base (BaseAmount), nível máximo (MaxLevel) e se é de Auto Ataque (AutoAttack).

        Contém **AnimationCurve**s (DamageCurve, CooldownCurve, AmountCurve) para definir a progressão dos valores em cada nível, permitindo o balanceamento via editor da Unity.

        Possui métodos para obter valores em um nível específico (GetValueAtLevel).

        Faz referência a um BaseWeaponEffectScriptable (que se resolverá no IWeaponEffect).

    WeaponData.cs (POCO):

        Representa o estado atual da arma. É uma classe POCO (Plain Old C# Object), serializável para debugging.

        Armazena o CurrentLevel, CurrentDamage, CurrentCooldown, CurrentAmount e uma referência ao WeaponBlueprint.

        Controla o tempo do próximo ataque (nextAttackTime).

        O método TryLevelUp() incrementa o nível e chama UpdateStatsFromCurves() para recalcular e aplicar os novos CurrentDamage, CurrentCooldown e CurrentAmount usando as Curves do Blueprint.

        O método TryAttack(...) verifica o cooldown e, se puder atacar, chama o Execute do IWeaponEffect.

#### 💥 Efeitos de Arma (IWeaponEffect)

O efeito real do ataque é desacoplado do WeaponData e do WeaponBlueprint usando o padrão Strategy.

    IWeaponEffect.cs (Interface):

        Define a única função necessária: void Execute(WeaponData data, Vector3 origin, Quaternion direction).

        Esta interface é implementada pelas classes POCO que contêm a lógica do ataque (por exemplo, SlashAttack).

    BaseWeaponEffectScriptable.cs (Base ScriptableObject):

        Classe base abstrata para todos os ScriptableObjects que definem um efeito de arma.

        Possui um método abstrato public abstract IWeaponEffect GetWeaponEffect(); para resolver a instância do POCO que implementa o efeito.

    Implementações de Exemplo:

        SlashAttack.cs (POCO): Implementa IWeaponEffect. Usa Physics.OverlapSphereNonAlloc para detectar inimigos em um alcance (Range) e dentro de um ângulo (AttackAngle), aplicando dano apenas aos inimigos na frente do jogador.

        SlashAttackScriptable.cs (ScriptableObject): Deriva de BaseWeaponEffectScriptable e contém a instância serializada de SlashAttack para configurar seus parâmetros no editor da Unity.

        NoneWeaponEffect.cs / NoneWeaponEffectScriptable.cs: Efeitos nulos para armas que podem não ter um efeito secundário.

#### 🕹️ Orquestração do Jogador

    PlayerWeapon.cs (MonoBehaviour):

        Gerencia as armas equipadas, separando-as em listas para ataques automáticos (automaticWeapons) e manuais (manualWeapons).

        Em Update(), itera sobre as armas automáticas e chama TryAttack().

        Em OnPressed() (ligado ao input de "Attack"), itera sobre as armas manuais e chama TryAttack().

        EquipWeapon() adiciona uma nova WeaponData à lista apropriada.

        LevelUpWeapon() localiza o WeaponData correspondente ao Blueprint e chama TryLevelUp().

O design permite:

    Fácil Balanceamento: Alterar os valores de nível e a progressão das curvas diretamente no WeaponBlueprint.

    Extensão Simples: Criar novos efeitos de arma (ex: projéteis, área de efeito) apenas criando uma nova classe POCO que implementa IWeaponEffect e um novo ScriptableObject derivado de BaseWeaponEffectScriptable para configurá-lo.

### 📜 Configuração das Waves (WavesScriptable e WaveSetup)

A estrutura de dados das ondas é definida fora das cenas, em Scriptable Objects, permitindo um fácil balanceamento e iteração:

    WavesScriptable.cs: Contém a lista principal de todas as ondas do jogo (List<WaveSetup> Waves).

    WaveSetup.cs: Define os parâmetros de uma única onda:

        TotalTime: A duração da fase de spawn da onda.

        MaxEnemies: O limite máximo de inimigos ativos simultaneamente na cena (independente do tipo).

        InitalTimeBeforeSpawn: Um tempo de espera antes de a onda começar a gerar inimigos.

        CooldownTime: O tempo de espera após a onda terminar e todos os inimigos serem derrotados.

        Groups: Uma lista de SpawnGroup que detalha quais inimigos e como eles devem ser gerados.

    SpawnGroup (em WaveSetup.cs): Define um grupo de inimigos específicos dentro da onda:

        WhichEnemy: O tipo de inimigo a ser gerado (EnemyType).

        MaxEnemyCount: O número máximo desse inimigo que pode ser gerado durante a onda.

        SpawnDelay: O intervalo mínimo entre os spawns desse grupo específico.

        SpawnOverTime: Uma AnimationCurve crucial que mapeia o tempo normalizado da onda (Eixo X, de 0 a 1) para a chance de spawn (peso) (Eixo Y, de 0 a 1). Uma curva mais alta em um determinado ponto de 0 a 1 significa uma maior probabilidade de spawnar.


#### 🕹️ Orquestração e Lógica Principal (WaveManager)

A classe WaveManager.cs é o MonoBehaviour que executa a progressão das ondas em uma coroutine (WaveLoop).

    Início e Fim: O sistema é ativado pelo evento OnGameStart e pausado pelo OnGameEnd (gerados via EventBus.cs).

    Loop da Onda:

        Espera o InitalTimeBeforeSpawn.

        Inicializa os rastreadores (SpawnGroupTracker) para cada SpawnGroup na onda atual.

        Entra no loop principal, que dura por TotalTime.

        Após o tempo acabar, espera até que todos os inimigos ativos sejam eliminados (yield return new WaitUntil(AllEnemiesEliminated);).

        Espera o CooldownTime e avança para a próxima onda.

    Rastreamento (SpawnGroupTracker.cs): Esta classe POCO (Plain Old C# Object) é usada para monitorar o estado de cada grupo de spawn, rastreando quantos inimigos foram gerados (SpawnedCount) e quando o próximo inimigo desse grupo pode ser gerado (NextSpawnTime).

#### 🎯 Lógica de Spawn e Limites

O método AttemptSpawn dentro do WaveManager executa a lógica de spawn:

    Limite de Inimigos: Primeiro, verifica se o número total de inimigos ativos está abaixo do limite MaxEnemies definido no WaveSetup. Se estiver no limite, o spawn é adiado.

    Contagem Máxima por Grupo: Verifica se o SpawnGroupTracker atingiu o seu MaxEnemyCount. Se sim, desativa o tracker.

    Cooldown por Grupo: Verifica se o SpawnDelay do grupo já passou (Time.time < tracker.NextSpawnTime).

    Peso de Spawn no Tempo: Calcula o spawnWeight (chance) avaliando a curva SpawnOverTime no tempo normalizado da onda.

    Sorteio e Spawn: Um valor aleatório é sorteado. Se o valor for menor ou igual ao spawnWeight, um inimigo é gerado:

        A posição de spawn é determinada pelo CircleSpawn.cs, que escolhe um ponto aleatório em um raio de 15m a 25m do jogador, excluindo um cone de 45° na frente da direção de movimento do jogador.

        O EnemyManager é chamado para instanciar o inimigo na posição calculada.

    Atualização: O SpawnGroupTracker é atualizado (incrementa SpawnedCount e define o próximo NextSpawnTime usando o SpawnDelay).

Em resumo, a taxa de spawn é controlada de forma flexível pela curva SpawnOverTime, respeitando os limites de contagem por grupo (MaxEnemyCount) e o limite global de inimigos ativos (MaxEnemies).

# ⚙️ Gerenciamento de Performance

A performance foi uma preocupação central, especialmente no que diz respeito ao grande número de inimigos em tela.

- Object Pooling (MonoBehaviourPool<T>):

    Utilizado extensivamente para EnemyControllers (e por extensão, inimigos). Isso evita a alocação e desalocação constante de memória (GC Alloc) em tempo de execução, que é um grande problema em jogos com muitas entidades sendo criadas e destruídas.

    Inimigos são inicializados no EnemyManager e retornados ao pool ao morrerem.

- Atualização Distribuída com Time-slicing (FixedUpdate em EnemyManager.cs):

    Em EnemyManager.cs, a atualização do movimento dos inimigos ativos é distribuída em vários frames (enemiesPerFrame).

    Objetivo: Evitar picos de desempenho (hiccups) no FixedUpdate (ciclo da física), mantendo o tempo de frame dentro do orçamento padrão da Unity (20ms) para um fixed update suave.

    Resultados (pelo profiling no editor): Testes com 500 inimigos ativos, atualizados em 2 frames fixos, mostraram picos de 13.54ms no FixedUpdate, mantendo o PlayerLoop em aproximadamente 16.81ms (cerca de 59 FPS), o que é um resultado positivo para uma situação extrema.

- Algoritmos de Combate e Movimento Otimizados:

    Combate (SlashAttack.cs): Utiliza Physics.OverlapSphereNonAlloc e um array estático (hitResults) para evitar alocações de memória ao realizar checagens de colisão para ataques de área.

    Perseguição (SimplePursuit.cs): Adicionado um short-cut para interromper o movimento quando o inimigo está muito próximo do jogador, para otimizar o cálculo. Também utiliza OverlapSphereNonAlloc e um array estático (separationResults) para o cálculo de separação entre inimigos, evitando o "empilhamento".

## 💯 Otimizações

| Categoria de Configuração | Configuração Específica         | Valor / Ação                     | Objetivo de Otimização                                                                       |
|---------------------------|---------------------------------|----------------------------------|----------------------------------------------------------------------------------------------|
| Build Settings            | Compression Method              | LZ4HC                            | Comprometimento entre alta compressão e velocidade de descompressão (tempo de carregamento). |
| Build Settings            | Scripting Backend               | IL2CPP                           | Melhorar o desempenho da CPU compilando C# para C++ nativo.                                  |
| Build Settings            | C++ Compiler Configuration      | Master                           | Otimização máxima (velocidade e tamanho) para a release build.                               |
| Build Settings            | Stacktrace                      | Desativada p/ warnings e asserts | Reduz o overhead de debug em builds finais.                                                  |
| Build Settings            | Diagnostic Data                 | Desativado                       | Evitar o envio de dados de diagnóstico do engine.                                            |
| Build Settings            | Lightmap e HDR Encoding         | Normal Quality                   | Balancear qualidade e uso de memória.                                                        |
| Build Settings            | Preload Assets                  | EnemyController (prefab)         | Evitar picos de latência carregando assets críticos na inicialização.                        |
| Physics Settings          | Otimização da Matrix de Colisão | Otimizada                        | Reduzir o número de cálculos de colisão desnecessários.                                      |
| Quality Settings          | VSync Count                     | Don't Sync                       | Permitir o FPS máximo, não limitado pela taxa de atualização do monitor.                     |
| Quality Settings          | Global MipMap                   | Half Resolution                  | Reduzir o uso de VRAM e o tempo de sampling de textura.                                      |
| Quality Settings          | Anisotropic Textures            | Desabilitado                     | Diminuir o custo de renderização de texturas em ângulos rasos.                               |
| URP                       | Depth-texture                   | Desativado                       | Reduzir o overhead de renderização se não for usado para efeitos visuais.                    |
| URP                       | Anti-aliasing                   | Desativado                       | Reduzir o custo de renderização de bordas.                                                   |
| URP                       | HDR                             | Desativado                       | Evitar o uso de High Dynamic Range, economizando banda de memória.                           |
| URP                       | Shadow Resolution               | 1024                             | Resolução de sombra de médio/baixo custo.                                                    |
| URP                       | Additional Lights               | Per Vertex / Limit = 2           | Iluminação mais rápida (Per Vertex) e limitação de luzes processadas.                        |
| URP                       | Shadows -> Cascade count        | 2                                | Reduzir a complexidade de cálculo de sombras de luzes direcionais.                           |
| URP                       | Soft shadows -> Quality         | Medium                           | Compromisso entre suavidade das sombras e desempenho.                                        |
| Time Settings             | Fixed Update Interval           | Aumento do Intervalo             | Diminuir a frequência de chamadas do FixedUpdate, reduzindo o overhead de física/lógica.     |

## 🎮 Análise de Desempenho

Avaliando o desempenho da atualização de 500 entidades inimigas, com foco na manutenção de uma taxa de frames (FPS) de 60.
Para atingir essa meta, o tempo máximo de processamento por frame deve ser de, no máximo, 16.67ms (1/60s).

### Metodologia e Estimativas Teóricas

A estratégia adotada consiste em instanciar 500 inimigos, dos quais 250 são atualizados por chamada do FixedUpdate. Considerando a taxa de atualização padrão do FixedUpdate na Unity de 20ms (o que corresponde a 50 execuções por segundo, com a ressalva de que a taxa foi aumentada no projeto), o cálculo teórico do tempo necessário para processar todos os inimigos é o seguinte:

    1. Número de FixedUpdate para todos os inimigos:
    Frames do FixedUpdate = Total de Inimigos / Inimigos por FixedUpdate = 500/250 = 2 frames.

    2. Tempo Total de Processamento Estimado (com base em 20ms):
    Tempo Total = Frames do FixedUpdate × Tempo Padrão do FixedUpdate = 2×20ms = 40ms

    3. Taxa de Atualização por Inimigo:
    Atualizações por Segundo = 1s / Tempo Total (s) ​= 1 / 0.040  ​= 25 Atualizações por inimigo

A taxa padrão de 20ms do FixedUpdate é tratada como um "orçamento" de tempo.
Ultrapassar consistentemente esse limite pode levar à degradação do desempenho geral do jogo.

### Resultados Práticos e Conclusão

A observação no profiler (utilizando um sistema com Ryzen 7 2700X e RX 6700 XT) em um cenário de estresse revelou os seguintes dados de desempenho:

    Pico do FixedUpdate: O pico de execução registrou 13.54ms. Este valor está abaixo do orçamento teórico de 20ms, indicando uma boa eficiência.

    Tempo da Main Thread (PlayerLoop): O tempo total de processamento da main thread por frame foi de 16.81ms.

O tempo total de 16.81ms implica uma taxa de frames de aproximadamente 59.49FPS (1/0.01681s).

Conclusão: Mesmo em uma situação de alta demanda, utilizando 500 inimigos no total e atualizando-os de forma escalonada em 2 frames, a otimização implementada demonstrou ser eficaz. Os resultados práticos confirmam que o sistema consegue operar dentro do limite de tempo proposto, mantendo uma média de 60FPS e operando abaixo do orçamento de 20ms para o FixedUpdate, atestando a otimização para a atualização de entidades.

Mais testes revelaram alguns gargalos no algortimo de perseguição (IPursuit), que foram prontamente corrigidos e otimizados.

Ao final, foi estabelecido um valor de 200 atualizações de inimigos por frame no FixedUpdate, o que aumenta a folga do game loop.

### Visão do profiler com PlayerLoop

![Visão do profiler com PlayerLoop](.Assets/Documentation/Images/Profiler_1.PNG "Profiler - 1")

### Visão do profiler com EnemyManager.FixedUpdate em destaque

![Visão do profiler com EnemyManager.FixedUpdate em destaque](.Assets/Documentation/Images/Profiler_2.PNG "Profiler - 2")

### Visão do profiler com hierarquia das funções

![Visão do profiler com hierarquia das funções](.Assets/Documentation/Images/Profiler_3.PNG "Profiler - 3")

### Visão da Unity com 500 unidades inimigas

![Visão da Unity com 500 unidades inimigas](.Assets/Documentation/Images/Unity_1_500.PNG "Unity - 1")

### Visão da Unity com scriptable de Waves

![Visão da Unity com scriptable de Waves](.Assets/Documentation/Images/Unity_2_500.PNG "Unity - 2")


# ✍️ Análise e Sugestões para o Código em C# Unity

Segue a análise e sugestões de código feitas por IA.

A arquitetura do seu código demonstra um bom entendimento dos princípios de **POCO (Plain Old C# Object)** e do uso de **ScriptableObjects** em Unity para separar dados configuráveis da lógica de *runtime*. O uso de interfaces e classes genéricas para o *pooling* (`MonoBehaviourPool<T>`), eventos (`EventBus`), e a estratégia de perseguição (`IPursuit`, `SimplePursuitStrategy`) é um ponto forte, promovendo **extensibilidade e baixo acoplamento**.

As preocupações que você mencionou no `Diario.txt` e nos comentários do código sobre a simplicidade e a falta de tempo são compreensíveis, e a base de código está sólida para futuras melhorias.

## 🔍 Pontos Específicos e Sugestões de Melhoria

### 1. Sistema de Itens (Item Modifier)
Você observou que o sistema de itens está incompleto (`PlayerItem.cs`).

* **Problema:** O método `ApplyItem` em `PlayerItems.cs` aplica o modificador, mas não gerencia o **estado** do item (nível, empilhamento, remoção).
* **Sugestão (Padrão Decorator/Composite):**
    * Crie uma classe **POCO** para representar a instância de um item adquirido, por exemplo, `PlayerItemInstance`, que armazene o `ItemBlueprint`, o nível atual e a referência ao `IItemModifier` instanciado.
    * Mantenha uma lista de `PlayerItemInstance` em `PlayerItems`.
    * Ao subir de nível, você pode chamar `instance.LevelUp()`, que por sua vez chama `modifier.UpdateValue()`, seguido de `modifier.Apply()` e `modifier.Remove()` para reaplicar o efeito com o novo valor.

### 2. Gerenciamento de Armas
O uso de *Blueprints* e *Data* para armas está correto, mas o *PlayerMovement* acopla estaticamente as armas:

* **Problema:** Em `PlayerMovement.cs`, o método `EquipeWeapons()` cria as armas de forma estática com *ScriptableObjects* serializados, o que viola o princípio de responsabilidade única.
* **Sugestão:** Mova a lógica de equipar armas para a classe **PlayerWeapon**. A classe `PlayerController` ou uma nova classe `GameManager` pode ser responsável por injetar a lista inicial de *WeaponBlueprints* no `PlayerWeapon` via uma lista serializada no Inspector ou um evento de início.

### 3. Evitando *Garbage Collection* no *SlashAttack*
O uso do array `hitResults` estático em `SlashAttack.cs` é uma ótima micro-otimização para evitar alocação de memória no *Update*.

* **Nota:** A limpeza explícita do array (`hitResults[i] = null;`) em `SlashAttack.cs` não é estritamente necessária para o Garbage Collector (GC) em C# quando se trata de *structs* (como *Collider*), nem é garantido que ela influencie o GC para tipos de referência no cenário do `Physics.OverlapSphereNonAlloc`. O mais importante é que a alocação do array em si (`new Collider[50]`) acontece apenas uma vez, reduzindo o *GC Pressure*.

### 4. *Knockback* e Movimento dos Inimigos
Em `EnemyController.cs`, o *knockback* usa uma corrotina e é tratado no `PerformKnockback`.

* **Sugestão:** Considere mover o estado `IsKnockingBack` e a lógica de `PerformKnockback` para o `EnemyData` ou um novo componente de **MovementState**. A `EnemyController` poderia então delegar o movimento (no `FixedUpdate` ou `Update`) para o estado atual (`Pursuit` ou `Knockback`). Isso tornaria o movimento mais extensível e o `EnemyController` mais limpo.

### 5. Configuração da Curva de Nível
O `CurvePopulatorEditor.cs` é uma excelente ferramenta de editor para simplificar o balanceamento, confirmando que você pensou na **experiência de design** também.

* **Fórmula Acumulativa em `CurvePopulatorEditor.cs`:**
    ```csharp
    if (cumulativeMode)
    {
        accumulatedValue += xpIncreasePerLevel;
        currentValue = accumulatedValue;
    }
    ```
    Isso modela corretamente um sistema de XP total por nível (por exemplo, Nível 2 = Nível 1 XP + Aumento Fixo).

* **Fórmula Não-Acumulativa:**
    ```csharp
    else
    {
        currentValue = initialValue + xpIncreasePerLevel * (level - 1);
    }
    ```
    Isso modela corretamente uma progressão linear para um único valor (por exemplo, *Dano Base*, onde cada nível adiciona um valor fixo).

Ambas as implementações estão corretas para as suas respectivas finalidades descritas.

---

# ⚠️ Pontos de Atenção (Gaps e Oportunidades de Melhoria)

O projeto foi construído em um curto espaço de tempo, e alguns pontos foram simplificados em detrimento de uma solução mais completa e escalável. O código fornece uma base adequada para futuras melhorias.

    Sistema de Itens (PlayerItems.cs):

        "Essa classe merecia tratamento mais adequado. Está crua devido a falta de tempo hábil."

        Atualmente, o método ApplyItem aplica o modificador, mas não gerencia o estado do item (nível atual, múltiplas cópias do mesmo item, remoção de modificadores após venda/descarte). O sistema real de itens é mais complexo e exigiria uma classe POCO para rastrear o estado de cada item.

    UI de Level-Up (PlayerLevelUpController.cs):

        "Essa classe deveria ser mais elástica. Não possuindo referências diretas ao itens mas obtendo por meio de uma lista dinamica para sugerir o level up."

        As opções de level-up (armas e itens) são referências estáticas no editor. O ideal seria um sistema dinâmico que obtivesse as opções disponíveis (armas não maximizadas, itens não possuídos) de um manager e as populasse na UI.

    Equipamento de Armas (PlayerMovement.cs e PlayerWeapon.cs):

        "Infelizmente, as armas estão de maneira estática. Gostaria que fosse de outra maneira, mas estou sem tempo ;3;"

        As armas são equipadas de forma hardcoded no PlayerMovement.cs (WeaponData sword = new WeaponData(swordBlueprint);). Um sistema ideal teria um WeaponManager que carregasse as armas (e seus respectivos WeaponBlueprints) de forma dinâmica.

    Restart do Jogo (UIGameOverController.cs):

        "Não queria construir o restart dessa maneira. Dando um loading na scene. Temos infraestrutura suficiente para dar um soft-reboot em todos os mecanismos facilmente."

        O restart atualmente recarrega a cena (SceneManager.LoadScene(0)). O ideal, dada a arquitetura desacoplada, seria implementar um soft-reboot disparando um evento para que todos os managers (EnemyManager, WaveManager, PlayerController, etc.) reinicializassem seus estados (limpar pools, resetar stats, resetar tempo, etc.).


---

## 🚀 Conclusão

O projeto demonstra uma arquitetura robusta, limpa e modular. Os padrões de design foram bem aplicados para garantir que as partes mais complexas do jogo (eventos, pooling, perseguição) sejam **flexíveis e de alto desempenho**. A separação de dados com ScriptableObjects e a otimização de GC no *SlashAttack* mostram um cuidado notável com a performance, o que é vital para o gênero *horde survival*.
