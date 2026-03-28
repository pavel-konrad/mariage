# Mariáš – projektový kontext

## Co je to za projekt

Mobilní karetní hra Mariáš pro Unity (iOS/Android). Klasická česká varianta.
3 hráči: 1 člověk (vždy index 0) vs. 2 AI. Fokus na grafiku a čistou architekturu, ne na feature kompletnost.

## Herní pravidla (kanonická, nemění se)

- 32 karet: 7–A ve 4 barvách (listy, kule, srdce, žaludy)
- 10 karet na hráče + 2 talon
- Bodové hodnoty: A=11, 10=10, K=4, Q=3, J=2 | celkem 90 bodů
- Výhra normální hry: 46+ bodů
- Síla karet: 7 < 8 < 9 < J < Q < K < 10 < A
- Přiznání barvy povinné; musíš přebít pokud můžeš; musíš trumfnout pokud nemáš barvu
- Hlášky: K+Q stejné barvy = 20 bodů, trumfová hláška = 40 bodů
- Sedma: výhra posledního štychu trumfovou sedmou

## Architektura – co zachováváme

### Pure C# jádro (bez MonoBehaviour)

- `MariasGameRules` – statická třída s pravidly, NEMĚNÍ SE
- `MariasGameState` – stav jednoho kola
- `Card`, `CardSuit`, `CardRank`, `Deck` – modely karet
- `IAIStrategy` + implementace `Easy/Medium/HardAIStrategy` – zachovat

### Co bylo přepsáno (refaktoring 2026-03)

| Před | Po |
|---|---|
| 40+ public C# events na MariasGameController | `IObserver<T>` / `ISubject<T>` / `*EventManager` |
| `ServiceLocator` singleton | DI přes Factory + `Initialize()` |
| `MariasGameManager` 920 řádků (god class) | `CardViewFactory` + Presentery + `AITurnController` |
| `[SerializeField] public T field` na SO | `[field: SerializeField] public T Prop { get; private set; }` |
| `*SO` suffix na ScriptableObjects | `*Config` (konfigurace) / `*Data` (data entity) |
| Single-implementation interfaces (YAGNI) | Smazány, přímé konkrétní třídy |

## Modulární game mody

Každý level = jeden `GameModeConfig : ScriptableObject` asset.
`MariasGameController` dostane config přes `Initialize()` – žádný switch na enum.

| | Easy | Medium | Hard |
|---|---|---|---|
| HumanAlwaysDeclarer | true | false | false |
| HasBidding | false | true | true |
| HasBettelDurch | false | false | true |
| HasFlekRe | false | flek | flek + re |
| AIDifficulty | Easy | Medium | Hard |

## Session (více kol)

`GameSessionData` – pure C# třída mimo `MariasGameController`.
Drží kumulativní skóre, počet kol, historii výsledků.
`MariasGameController` řídí jedno kolo; session layer ho volá opakovaně.

## Jmenné konvence specifické pro projekt

- Namespace: `MariasGame.*` (zachovat)
- Hráč 0 = vždy lidský hráč
- Hráč 1, 2 = AI
- Event data structs: `GameEvent`, `CardEvent`, `ScoreEvent` s enum typem
- `*Presenter` = propojuje game events s UI (implementuje `IObserver<T>`)

## Co NEIMPLEMENTUJEME (KISS)

- Rotace role forhonta na Easy levelu
- Save/load hry mid-round
- Replay štychu
- Síťová hra
- Více než 3 game mody (Easy / Medium / Hard)
