# Plán refaktoringu – Mariáš

Cíl: srovnat projekt s unity-core konvencemi před implementací nových feature.
Pořadí kroků je závazné – každý krok staví na předchozím.

---

## Krok 1 – Observer pattern: interfaces + event structs

**Soubory k vytvoření:**
- `Core/Interfaces/IObserver.cs`
- `Core/Interfaces/ISubject.cs`
- `Core/Events/GameEvent.cs` (struct + GameEventType enum)
- `Core/Events/CardEvent.cs` (struct + CardEventType enum)
- `Core/Events/ScoreEvent.cs` (struct + ScoreEventType enum)

**Pravidla:**
- Event data jako `struct` (nula GC alokací)
- Každý struct má `Type` (enum), relevantní payload, `int PlayerIndex`
- `ISubject<T>.NotifyObservers()` iteruje přes `.ToArray()` snapshot

**Příklad GameEvent:**
```csharp
public struct GameEvent
{
    public GameEventType Type;
    public int PlayerIndex;
    public GamePhase Phase;
    public MariasGameRules.GameResult Result;
}

public enum GameEventType
{
    GameStarted, PhaseChanged, PlayerTurnStarted,
    TrickWon, MarriageDeclared, TrumpDeclared,
    BetDoubled, GameEnded
}
```

---

## Krok 2 – EventManagery

**Soubory k vytvoření:**
- `Managers/GameEventManager.cs` – `MonoBehaviour, ISubject<GameEvent>`
- `Managers/CardEventManager.cs` – `MonoBehaviour, ISubject<CardEvent>`
- `Managers/ScoreEventManager.cs` – `MonoBehaviour, ISubject<ScoreEvent>`

**Pravidla:**
- Jeden EventManager = jedna zodpovědnost
- Žádná herní logika uvnitř EventManagerů – jen distribuce
- `OnDestroy` neřeší nic (managery žijí celou scénu)

---

## Krok 3 – Refactor MariasGameController

**Soubory k úpravě:**
- `Core/MariasGameController.cs`

**Co se mění:**
1. Smazat všechny `public event Action<T>` (40+ řádků)
2. Přidat do konstruktoru: `GameEventManager`, `CardEventManager`, `ScoreEventManager`
3. Každé `OnXxx?.Invoke(...)` nahradit `_gameEventManager.NotifyObservers(new GameEvent { ... })`
4. **Opravit bidding flow:**
   - Přidat `_passedPlayers` tracking
   - `MakeBid(Pass)` → posun na dalšího, dokud nezůstane jeden nebo nevyhraje nabídka
   - Pokud všichni pasují → re-deal (nebo dle GameModeConfig)

**Co zůstává stejné:**
- `MariasGameRules` volání beze změny
- `MariasGameState` beze změny
- `PlayCard()`, `ResolveTrick()`, `EndGame()` logika beze změny

---

## Krok 4 – GameModeConfig ScriptableObject

**Soubory k vytvoření:**
- `ScriptableObjects/GameModeConfig.cs`
- Assets: `Easy.asset`, `Medium.asset`, `Hard.asset` (vytvoří designer v editoru)

```csharp
[CreateAssetMenu(fileName = "GameModeConfig", menuName = "Marias/GameModeConfig")]
public class GameModeConfig : ScriptableObject
{
    [field: SerializeField] public string ModeName { get; private set; }
    [field: SerializeField] public bool HumanAlwaysDeclarer { get; private set; }
    [field: SerializeField] public bool HasBidding { get; private set; }
    [field: SerializeField] public bool HasBettelDurch { get; private set; }
    [field: SerializeField] public bool HasFlekRe { get; private set; }
    [field: SerializeField] public AIDifficulty AIDifficulty { get; private set; }
}
```

`MariasGameController.Initialize()` dostane `GameModeConfig` jako parametr.

---

## Krok 5 – Oprava ScriptableObjects

**Soubory k přejmenování (třída + soubor):**

| Starý název | Nový název |
|---|---|
| `GameSettingsSO` | `GameSettingsConfig` |
| `CardDataSO` | `CardData` |
| `CardThemeSO` | `CardThemeConfig` |
| `CardDatabaseSO` | `CardDatabase` |
| `SoundDataSO` | `SoundData` |
| `AvatarDataSO` | `AvatarData` |
| `AvatarDatabaseSO` | `AvatarDatabase` |
| `EnemyDataSO` | `EnemyData` |
| `EnemyDatabaseSO` | `EnemyDatabase` |

**Field pattern – projít každý SO a opravit:**
```csharp
// PŘED
[SerializeField] public string Name;

// PO
[field: SerializeField] public string Name { get; private set; }
```

⚠️ Po přejmenování tříd je nutné v Unity reassignovat SO assety – meta soubory se nezmění automaticky.

---

## Krok 6 – Smazat ServiceLocator + přidat GameBootstrapper

**Soubory ke smazání:**
- `Services/ServiceLocator.cs`

**Soubory k vytvoření:**
- `Game/GameBootstrapper.cs` – `MonoBehaviour`, dráty všechny závislosti v `Awake()`

**GameBootstrapper zodpovědnosti:**
1. Vytvoří EventManagery (nebo reference přes Inspector)
2. Vytvoří services (CardDataService, DeckFactoryService, ...)
3. Vytvoří `MariasGameController` přes konstruktor s DI
4. Předá controlleru `GameModeConfig`
5. Zaregistruje Presentery jako observery

**Třídy co volaly `ServiceLocator.Get<T>()` → dostanou závislost přes `Initialize(T dep)`**

---

## Krok 7 – Rozdělit MariasGameManager

Současný `MariasGameManager.cs` (920 řádků) → smazat, nahradit:

| Nová třída | Zodpovědnost | Typ |
|---|---|---|
| `CardViewFactory` | Instantiate CardView prefabů, injektovat závislosti | MonoBehaviour |
| `PlayerInfoPresenter` | Zobrazuje jméno, avatar, score hráče | MonoBehaviour, IObserver<ScoreEvent> |
| `HandPresenter` | Layout karet v ruce, highlight legálních tahů | MonoBehaviour, IObserver<CardEvent> |
| `BiddingPresenter` | Zobrazuje / skrývá BiddingModal | MonoBehaviour, IObserver<GameEvent> |
| `TrumpPresenter` | Zobrazuje / skrývá TrumpSelectionModal | MonoBehaviour, IObserver<GameEvent> |
| `AITurnController` | Spouští AI coroutines po `PlayerTurnStarted` eventu | MonoBehaviour, IObserver<GameEvent> |
| `GameFlowController` | Orchestrace scény (start hry, konec kola, nové kolo) | MonoBehaviour, IObserver<GameEvent> |

---

## Krok 8 – Session layer

**Soubory k vytvoření:**
- `Core/GameSessionData.cs` – pure C# třída
- `Game/GameSessionController.cs` – MonoBehaviour

```csharp
public class GameSessionData
{
    public int[] CumulativeScore { get; private set; } = new int[3];
    public int RoundsPlayed { get; private set; }
    public List<MariasGameRules.GameResult> RoundHistory { get; } = new();

    public void RecordRoundResult(MariasGameRules.GameResult result) { ... }
    public void ResetSession() { ... }
}
```

`GameSessionController` drží `GameSessionData` a říká `GameBootstrapper` kdy spustit nové kolo.

---

## Krok 9 – Smazat YAGNI interfaces

**Soubory ke smazání** (single-implementation, používány jen ServiceLocatorem):
- `Core/Interfaces/ICardDataProvider.cs`
- `Core/Interfaces/IAvatarProvider.cs`
- `Core/Interfaces/IEnemyProvider.cs`
- `Core/Interfaces/ISettingsProvider.cs`
- `Core/Interfaces/ICardThemeProvider.cs`
- `Core/Interfaces/IAssetLoader.cs`
- `Core/Interfaces/ISettingsRepository.cs`
- `Core/Interfaces/IHumanPlayer.cs` (jediná implementace)
- `Core/Interfaces/IAIPlayer.cs` (jediná implementace)

**Zachovat** (2+ implementace nebo reálná inverze závislosti):
- `IAIStrategy` (Easy/Medium/Hard)
- `IPlayer` (HumanPlayer/AIPlayer)
- `ICardStateMachine`
- `IDeck` / `ICardCollection`
- `IDeckFactory`

---

## Krok 10 – Debug.Log audit

Projít všechny soubory a obalit `Debug.Log` do `#if UNITY_EDITOR`:
```csharp
#if UNITY_EDITOR
Debug.Log($"[GameController] Phase: {phase}");
#endif
```

`Debug.LogWarning` a `Debug.LogError` nechat bez podmínky.

---

## Pořadí v čase

```
Krok 1  →  Krok 2  →  Krok 3   (observer základ)
                    ↓
              Krok 4 + 5         (SO vrstva)
                    ↓
              Krok 6             (odstraní ServiceLocator)
                    ↓
              Krok 7             (závisí na event systému a bootstrapperu)
                    ↓
              Krok 8             (závisí na fungující hře)
                    ↓
              Krok 9 + 10        (cleanup, může jít kdykoli po kroku 6)
```

---

## Co se NETÝKÁ refaktoringu (nemění se)

- `MariasGameRules.cs` – pravidla jsou správně
- `MariasGameState.cs` – stav je čistý
- `Card.cs`, `Deck.cs`, `CardState.cs`, `CardStateMachine.cs`
- `EasyAIStrategy.cs`, `MediumAIStrategy.cs`, `HardAIStrategy.cs`
- UI komponenty: `CardView`, `CardDragHandler`, `CardHandLayout`, `ModalBase`
- Editor tools: `CardDatabaseCreator`, `GameSetupTester`
