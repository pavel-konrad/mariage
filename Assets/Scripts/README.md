# Mariashkey -- Cesky Marias (Unity)

Implementace ceskeho mariase v Unity s SOLID architekturou, event-driven designem a plnou podporou AI protivniku.

**Balik:** 32 karet (7-A x 4 barvy) | **Hraci:** 3 | **Karty:** 10 na hrace + 2 talon

---

## Obsah

- [Cast 1: Implementacni workflow](#cast-1-implementacni-workflow)
  - [Faze 1: ScriptableObjects -- datova vrstva](#faze-1-scriptableobjects----datova-vrstva)
  - [Faze 2: Scene hierarchy -- GameObjecty](#faze-2-scene-hierarchy----gameobjekty)
  - [Faze 3: Managers -- pripojeni komponent](#faze-3-managers----pripojeni-komponent)
  - [Faze 4: UI -- Canvas a prefaby](#faze-4-ui----canvas-a-prefaby)
  - [Faze 5: Propojeni UI komponent](#faze-5-propojeni-ui-komponent)
  - [Faze 6: GameController -- herni smycka](#faze-6-gamecontroller----herni-smycka)
  - [Faze 7: Audio a VFX napojeni](#faze-7-audio-a-vfx-napojeni)
  - [Faze 8: Overeni a testovani](#faze-8-overeni-a-testovani)
- [Cast 2: Architektura projektu](#cast-2-architektura-projektu)
  - [Adresarova struktura](#adresarova-struktura)
  - [Architekturni vzory](#architekturni-vzory)
  - [Datovy model](#datovy-model)
  - [Event system](#event-system)
  - [AI system](#ai-system)
  - [Herni pravidla](#herni-pravidla)
- [Troubleshooting](#troubleshooting)

---

# Cast 1: Implementacni workflow

Kompletni postup, jak ve scene od nuly napojit a nastavit vsechny systemy. Kazda faze na sebe navazuje -- postupuj striktne v poradi.

---

## Faze 1: ScriptableObjects -- datova vrstva

Vsechna konfigurovatelna data existuji jako ScriptableObjecty. Pred cimkoliv jinym je nutne je vytvorit.

### 1.1 CardDatabaseSO (32 karet)

**Automaticka migrace** (pokud existuje `CardDatabase.json`):

1. Unity Editor menu: `PrsiGame > Tools > Migrate Card Database`
2. Vyber `Assets/Scripts/Services/CardDatabase.json`
3. Output path: `Assets/ScriptableObjects/Cards`
4. Klikni **Migrate to ScriptableObjects**
5. Vysledek: `CardDatabase.asset` + 32x `CardDataSO` + `GameSounds.asset`

**Rucni vytvoreni:**

1. `Create > PrsiGame > Card Database` -- pojmenuj `CardDatabase`
2. Pro kazdou ze 32 karet (`7,8,9,10,J,Q,K,A` x `Hearts,Diamonds,Clubs,Spades`):
   - `Create > PrsiGame > Card Data`
   - Nastav `Suit`, `Rank`
   - Prirad `Sprite` (obrazek karty)
   - Volitelne prirad `AudioClip[]` do `cardSounds`
   - Konvence pojmenovani: `Hearts_Seven`, `Spades_Ace`, ...
3. Vsech 32 `CardDataSO` pretahni do pole `Cards` v `CardDatabase.asset`
4. Nastav `cardBackSprite` (sprite rubu karty)

### 1.2 CardThemeSO (volitelne, pro vice temat)

Pokud chces podporu vice grafickych temat:

1. `Create > PrsiGame > Card Theme`
2. Vyplni:
   - `themeName`: "Classic" / "Modern" / ...
   - `themeDescription`: popis tematu
   - `themePreview`: preview sprite pro UI vyber
   - `isUnlocked`: `true` (nebo `false` pro DLC)
   - `cardDatabase`: reference na prislusne `CardDatabaseSO`
3. Opakuj pro kazde tema -- kazde tema ma vlastni `CardDatabaseSO` s vlastnimi sprity

Organizace souboru:
```
Assets/ScriptableObjects/Themes/
  Classic/
    CardTheme_Classic.asset
    CardDatabase_Classic.asset
    Classic_Hearts_Seven.asset ... (32 karet)
  Modern/
    CardTheme_Modern.asset
    CardDatabase_Modern.asset
    Modern_Hearts_Seven.asset ... (32 karet)
```

### 1.3 GameSettingsSO

1. `Create > PrsiGame > Game Settings` -- pojmenuj `GameSettings`
2. Nastav v inspektoru:

| Pole | Typ | Default | Popis |
|------|-----|---------|-------|
| `startingCash` | int | 1000 | Pocatecni penize hrace |
| `minBet` | int | 1 | Minimalni sazka |
| `maxBet` | int | 100 | Maximalni sazka |
| `cardsPerPlayer` | int | 10 | Pocet karet (marias = 10, nemenit) |
| `maxPlayers` | int | 4 | Max hracu |
| `aiThinkTime` | float | 1.5 | Prodleva AI tahu (sekundy) |
| `aiDifficultyLevel` | int | 2 | 1=Easy, 2=Medium, 3=Hard |
| `cardAnimationDuration` | float | 0.5 | Delka animace karty (s) |
| `dealCardDelay` | float | 0.2 | Prodleva mezi rozdanymi kartami (s) |
| `masterVolume` | float | 1.0 | Hlavni hlasitost 0-1 |
| `musicVolume` | float | 0.7 | Hlasitost hudby 0-1 |
| `sfxVolume` | float | 1.0 | Hlasitost efektu 0-1 |

### 1.4 AvatarDatabaseSO

1. `Create > PrsiGame > Avatar Database` -- pojmenuj `AvatarDatabase`
2. Pro kazdy avatar:
   - `Create > PrsiGame > Avatar Data`
   - Nastav `avatarName`, `avatarSprite`, `avatarId`
   - Pojmenovani: `Avatar_0`, `Avatar_1`, ...
3. Vsechny `AvatarDataSO` pretahni do `AvatarDatabase.asset`

### 1.5 EnemyDatabaseSO

1. `Create > PrsiGame > Enemy Database` -- pojmenuj `EnemyDatabase`
2. Pro kazdeho AI protivnika:
   - `Create > PrsiGame > Enemy Data`
   - Nastav `enemyName`, `enemySprite`, `enemyId`, `difficulty` (1-3)
3. Vsechny `EnemyDataSO` pretahni do `EnemyDatabase.asset`

### Kontrolni seznam faze 1

- [ ] `CardDatabase.asset` se 32 kartami (kazda ma sprite)
- [ ] `cardBackSprite` nastaven v CardDatabaseSO
- [ ] `GameSettings.asset` s vychozimi hodnotami
- [ ] `AvatarDatabase.asset` s alespon 4 avatary
- [ ] `EnemyDatabase.asset` s alespon 2 AI neprateli
- [ ] Volitelne: `CardTheme_*.asset` pro kazde tema

---

## Faze 2: Scene hierarchy -- GameObjecty

Vytvor hierarchii GameObjectu v Hierarchy panelu. Vsechno se stavi odzdola -- zacni prazdnou scenou.

### Cilova hierarchie

```
GameRoot
├── [ServiceLocator]              ← ServiceLocator.cs (singleton)
├── Managers
│   ├── CardDataManager           ← CardDataManager.cs
│   ├── GameSettingsManager       ← GameSettingsManager.cs
│   ├── AvatarManager             ← AvatarManager.cs
│   ├── EnemyManager              ← EnemyManager.cs
│   ├── AudioManager              ← AudioManager.cs + 3x AudioSource
│   └── VFXManager                ← VFXManager.cs
├── Canvas                        ← Canvas + CanvasScaler + GraphicRaycaster
│   ├── Header
│   │   ├── MenuButton
│   │   ├── TrumpIndicator        ← Image (ikona trumfove barvy)
│   │   └── GamePhaseText         ← TextMeshProUGUI
│   ├── EnemyBar 1
│   │   ├── Avatar                ← Image (circle mask)
│   │   ├── EnemyName             ← TextMeshProUGUI
│   │   ├── ScorePanel
│   │   │   ├── GoldText          ← TextMeshProUGUI
│   │   │   └── PointsText        ← TextMeshProUGUI
│   │   └── EnemyHand             ← HorizontalLayoutGroup (karty řízeny přes CardView)
│   ├── EnemyBar 2
│   │   └── (stejna struktura)
│   ├── EnemyBar 3                ← volitelne (4. hrac)
│   │   └── (stejna struktura)
│   ├── TableArea
│   │   ├── DeckPanel
│   │   │   └── DeckView          ← DeckView.cs + Image + Text
│   │   ├── TrickPanel
│   │   │   ├── Card1Slot         ← Image (1. karta stychu)
│   │   │   ├── Card2Slot         ← Image (2. karta stychu)
│   │   │   └── Card3Slot         ← Image (3. karta stychu)
│   │   └── TalonPanel            ← pro vyber talonu
│   ├── PlayerInfo
│   │   ├── Avatar                ← Image
│   │   ├── NameText              ← TextMeshProUGUI
│   │   └── ScorePanel
│   │       ├── GoldText          ← TextMeshProUGUI
│   │       └── PointsText        ← TextMeshProUGUI
│   ├── PlayerHand                ← PlayerHandView.cs
│   ├── MarriageButton            ← Button (jen kdyz hrac vynasi a ma hlasku)
│   └── Modals
│       ├── BiddingModal          ← modal vyjizdi pri fazi Bidding
│       │   ├── ModalBackground   ← Image (polopruhledne pozadi)
│       │   └── ModalPanel
│       │       ├── GameButton    ← Button
│       │       ├── SevenButton   ← Button
│       │       ├── HundredButton ← Button
│       │       ├── BettelButton  ← Button
│       │       ├── DurchButton   ← Button
│       │       └── PassButton    ← Button
│       └── TrumpSelectionModal   ← modal vyjizdi pri fazi Declaring
│           ├── ModalBackground   ← Image (polopruhledne pozadi)
│           └── ModalPanel
│               ├── HeartsButton  ← Button
│               ├── DiamondsButton ← Button
│               ├── ClubsButton   ← Button
│               └── SpadesButton  ← Button
├── Main Camera
└── EventSystem                   ← POVINNE pro UI interakce
```

### Postup vytvoreni

1. **Prazdna scena** -- smaz vsechno krome Main Camera a EventSystem
2. **Vytvor `GameRoot`** -- prazdny GameObject, root vsecho
3. **Vytvor `[ServiceLocator]`** -- prazdny GameObject pod GameRoot
4. **Vytvor `Managers`** -- prazdny parent, pod nej 6 prazdnych GameObjectu (jeden pro kazdy manager)
5. **Vytvor `Canvas`** pod GameRoot:
   - Render Mode: `Screen Space - Camera`
   - Pixel Perfect: zapnout
   - Referenci na Main Camera
6. **Na Canvasu nastav CanvasScaler:**
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1080 x 1920` (portrait)
   - Screen Match Mode: `Match Width Or Height`
   - Match: `0.5`
7. **Vytvor vsechny child GameObjecty** podle hierarchie vyse
8. Over ze **EventSystem** existuje ve scene (jinak UI nebude reagovat na kliknuti)

### Doporucene barvy UI

```
Header:     #47597A  rgb(71, 89, 122)
Table:      #6B8C78  rgb(107, 140, 120)
PlayArea:   #8C6B6B  rgb(140, 107, 107)
Modals:     #383838  rgb(56, 56, 56)
Gold:       #D9BF40  rgb(217, 191, 64)
```

---

## Faze 3: Managers -- pripojeni komponent

Na kazdy Manager GameObject pripoj odpovidajici skript a prirad reference v inspektoru.

### 3.1 ServiceLocator

**GameObject:** `[ServiceLocator]`
**Skript:** `ServiceLocator.cs`

| Inspector pole | Prirazeni | Poznamka |
|----------------|-----------|----------|
| `autoDiscovery` | `true` | Automaticky najde vsechny managery ve scene |
| `cardDataManager` | -- | Nutne jen kdyz autoDiscovery = false |
| `settingsManager` | -- | Nutne jen kdyz autoDiscovery = false |
| `avatarManager` | -- | Nutne jen kdyz autoDiscovery = false |
| `enemyManager` | -- | Nutne jen kdyz autoDiscovery = false |
| `audioManager` | -- | Nutne jen kdyz autoDiscovery = false |
| `vfxManager` | -- | Nutne jen kdyz autoDiscovery = false |

ServiceLocator je **singleton** -- v cele scene smi byt jen jeden. Pri `autoDiscovery = true` pouzije `FindAnyObjectByType<T>()` pro kazdy manager.

### 3.2 GameSettingsManager

**GameObject:** `Managers/GameSettingsManager`
**Skript:** `GameSettingsManager.cs`

| Inspector pole | Prirazeni | Povinne |
|----------------|-----------|---------|
| `settingsSO` | `GameSettings.asset` (z faze 1.3) | **ANO** |
| `autoSave` | `true` | ne |
| `saveKey` | `"GameSettings"` | ne |

Implementuje `ISettingsProvider`. Automaticky uklada nastaveni pri pauze / ztrate fokusu.

### 3.3 CardDataManager

**GameObject:** `Managers/CardDataManager`
**Skript:** `CardDataManager.cs`

| Inspector pole | Prirazeni | Povinne |
|----------------|-----------|---------|
| `availableThemes` | Seznam `CardThemeSO` assetu | Doporuceno |
| `defaultTheme` | Vychozi `CardThemeSO` | Volitelne |
| `cardDatabaseSO` | `CardDatabase.asset` (z faze 1.1) | **ANO** (fallback bez temat) |

Poskytuje sluzby: `ICardDataProvider`, `IDeckFactory`, `ICardThemeProvider`, `IAssetLoader`.

Interni servisni vrstva:
- `CardDataService` -- pristup k datum jednotlivych karet
- `DeckFactoryService` -- vytvari instance balicku
- `CardThemeService` -- sprava temat, ukladani vyberu do PlayerPrefs
- `AssetLoaderService` -- cachovane nacitani assetu

### 3.4 AvatarManager

**GameObject:** `Managers/AvatarManager`
**Skript:** `AvatarManager.cs`

| Inspector pole | Prirazeni | Povinne |
|----------------|-----------|---------|
| `avatarDatabaseSO` | `AvatarDatabase.asset` (z faze 1.4) | **ANO** |

Implementuje `IAvatarProvider`. Poskytuje `GetAvatar(int index)`, `AvatarCount`.

### 3.5 EnemyManager

**GameObject:** `Managers/EnemyManager`
**Skript:** `EnemyManager.cs`

| Inspector pole | Prirazeni | Povinne |
|----------------|-----------|---------|
| `enemyDatabaseSO` | `EnemyDatabase.asset` (z faze 1.5) | **ANO** |

Implementuje `IEnemyProvider`. Poskytuje `GetEnemy(int index)`, `GetAllEnemies()`, `EnemyCount`.

### 3.6 AudioManager

**GameObject:** `Managers/AudioManager`
**Skript:** `AudioManager.cs`

Na tentyz GameObject **pridej 3x AudioSource** komponentu (rucne pres Add Component):

| AudioSource | Ucel | Play On Awake | Loop |
|-------------|-------|---------------|------|
| 1. `musicSource` | Hudba na pozadi | Off | On |
| 2. `sfxSource` | Zvukove efekty | Off | Off |
| 3. `ambientSource` | Ambientni zvuky | Off | On |

Inspector pole AudioManageru:

| Pole | Prirazeni | Popis |
|------|-----------|-------|
| `musicSource` | AudioSource 1 | Drag z Inspector |
| `sfxSource` | AudioSource 2 | Drag z Inspector |
| `ambientSource` | AudioSource 3 | Drag z Inspector |
| `cardSounds` | SoundLibrary | 5 slotu -- viz nize |
| `uiSounds` | SoundLibrary | 3 sloty -- viz nize |
| `gameSounds` | SoundLibrary | 7 slotu -- viz nize |
| `masterVolume` | 1.0 | Hlavni hlasitost |
| `musicVolume` | 0.7 | Hlasitost hudby |
| `sfxVolume` | 1.0 | Hlasitost efektu |
| `ambientVolume` | 0.5 | Hlasitost ambient |

**Card Sounds sloty** (SoundLibrary):
1. `cardPlace` -- polozeni karty na stul
2. `cardDraw` -- lizeni karty z balicku
3. `cardShuffle` -- michani karet
4. `cardDeal` -- rozdavani karet
5. `cardFlip` -- otoceni karty

**Game Sounds sloty** (SoundLibrary):
1. `trickWin` -- vyhra stychu
2. `gameWin` -- vyhra cele hry
3. `gameLose` -- prohra hry
4. `marriageCall` -- hlaseni (20/40 bodu)
5. `sevenCall` -- hlaseni sedmy
6. `bettelCall` -- vyhlaseni betlu
7. `durchCall` -- vyhlaseni durchu

**UI Sounds sloty** (SoundLibrary):
1. `buttonClick` -- kliknuti na tlacitko
2. `buttonHover` -- najeti na tlacitko
3. `notification` -- notifikace/upozorneni

### 3.7 VFXManager

**GameObject:** `Managers/VFXManager`
**Skript:** `VFXManager.cs`

| Inspector pole | Prirazeni | Povinne |
|----------------|-----------|---------|
| `vfxLibrary` | VFXLibrary (15 prefab slotu) | **ANO** |
| `vfxContainer` | Transform (prazdny child GO) | Doporuceno |
| `enableVFX` | `true` | ne |
| `vfxIntensity` | 1.0 | ne |

**VFXLibrary sloty** (15 prefabu):

Card efekty:
1. `cardPlaceEffect` -- efekt pri polozeni karty
2. `cardDrawEffect` -- efekt pri lizeni karty
3. `cardFlipEffect` -- efekt pri otoceni
4. `cardSelectEffect` -- highlight vybrane karty (persistent)
5. `playableCardEffect` -- glow hratelne karty (persistent)

Game efekty:
6. `trickWinEffect` -- efekt vyhry stychu
7. `gameWinEffect` -- velky efekt vyhry hry
8. `gameLoseEffect` -- efekt prohry
9. `marriageEffect` -- efekt hlaseni
10. `sevenEffect` -- efekt sedmy
11. `bettelEffect` -- efekt betlu
12. `durchEffect` -- efekt durchu
13. `confettiEffect` -- konfety pro velke vyhry

UI efekty:
14. `buttonHighlightEffect` -- highlight tlacitka
15. `notificationEffect` -- efekt notifikace

Doporuceni pro VFX prefaby:
- Pouzivejte `ParticleSystem` s `Play On Awake = true`
- One-shot efekty se automaticky znici po dokonceni
- VFXManager skaluje `startSize` podle `vfxIntensity`

### Kontrolni seznam faze 3

- [ ] ServiceLocator ma `autoDiscovery = true`
- [ ] GameSettingsManager ma prirazene `GameSettings.asset`
- [ ] CardDataManager ma prirazene `CardDatabase.asset` (a/nebo themes)
- [ ] AvatarManager ma prirazene `AvatarDatabase.asset`
- [ ] EnemyManager ma prirazene `EnemyDatabase.asset`
- [ ] AudioManager ma 3x AudioSource + SoundLibrary sloty vyplnene
- [ ] VFXManager ma VFXLibrary sloty vyplnene

---

## Faze 4: UI -- Canvas a prefaby

### 4.1 Canvas nastaveni

Canvas uz je vytvoren v fazi 2. Over:

- **Render Mode:** Screen Space - Camera
- **Camera reference:** Main Camera
- **CanvasScaler:** Scale With Screen Size, 1080x1920, Match=0.5
- **GraphicRaycaster:** pridan (jinak tlacitka nereaguji)

### 4.2 CardView Prefab (KRITICKY)

CardView je zakladni stavebni blok -- pouziva se vsude (ruka hrace, ruka nepritele, balicek, stych).

1. Vytvor prazdny GameObject `CardView`
2. Pridej komponenty:
   - `RectTransform` -- Width: 120, Height: 180, Pivot: (0.5, 0.5)
   - `Image` -- zobrazeni karty (hlavni sprite)
   - `Button` -- pro interakci (kliknuti)
   - `CardView.cs` -- hlavni logika
   - `CardAnimationController.cs` -- volitelne (animace)
   - `Animator` -- volitelne (Animator Controller)
3. V inspektoru CardView nastav:
   - `cardImage` -- reference na Image komponentu
   - `cardBackImage` -- volitelne (druhy Image pro rub)
   - `isFaceUp` -- `true` (vychozi)
   - `isInteractive` -- `true`
4. **Uloz jako Prefab:** pretahni do `Assets/Prefabs/UI/CardView.prefab`
5. **Smaz z Hierarchy** -- prefab se bude instantiovat dynamicky

### 4.3 Volitelne: Animator Controller pro karty

Pokud chces animace karet:

1. `Create > Animator Controller` -- pojmenuj `CardAnimator`
2. Pridej states:
   - `Idle` (default)
   - `Dealing` -- animace prijezdu karty do ruky
   - `Selected` -- karta je vybrana (zvyraznena)
   - `Playing` -- karta leti na stul
   - `Discarding` -- karta mizi do odhozovaciho balicku
   - `Flipping` -- otaceni karty
3. Pridej parametry (triggery):
   - `Deal` (Trigger)
   - `Discard` (Trigger)
   - `Play` (Trigger)
   - `Flip` (Trigger)
   - `Select` (Trigger)
   - `FaceUp` (Bool)
   - `Speed` (Float)
4. Nastav transitions mezi states podle triggeru
5. Prirad Animator Controller do CardView prefabu

Fallback bez Animatoru -- CardAnimationController podporuje prime AnimationClip reference:
```
Inspector: CardAnimationController
├── dealAnimation      [AnimationClip]
├── discardAnimation   [AnimationClip]
├── playAnimation      [AnimationClip]
├── flipAnimation      [AnimationClip]
└── selectAnimation    [AnimationClip]
```

### Kontrolni seznam faze 4

- [ ] Canvas ma CanvasScaler 1080x1920
- [ ] Canvas ma GraphicRaycaster
- [ ] EventSystem existuje ve scene
- [ ] CardView prefab ulozen v `Assets/Prefabs/UI/`
- [ ] CardView ma Image + Button + CardView.cs
- [ ] Volitelne: Animator Controller s transitions

---

## Faze 5: Propojeni UI komponent

UI je rizeno primo z `MariasGameManager.cs` (MonoBehaviour). Ten vytvari CardView instance, spravuje zobrazeni karet hrace i protivniku, a reaguje na eventy z `MariasGameController`.

### 5.1 MariasGameManager

**Pripoj na:** `GameRoot` nebo dedikovy `GameManager` GameObject
**Skript:** `MariasGameManager.cs`

MariasGameManager je hlavni MonoBehaviour ktery:
- Vytvari a ridi `MariasGameController` (core herni smycka)
- Spravuje UI primo (instantiuje `CardView` prefaby)
- Subscribuje vsechny herni eventy (tah, stych, drazba, hlasky, ...)
- Ridi AI tahy a lidsky vstup

### 5.2 PlayerInfoPanel

**Pripoj na:** kazdy `EnemyBar` a `PlayerInfo` GameObject
**Skript:** `PlayerInfoPanel.cs`

Zobrazuje informace o hraci (jmeno, avatar, penize, body). Pouziva se z `MariasGameManager`.

### 5.3 CardView (prefab)

Zakladni zobrazovaci komponenta pro jednu kartu. Vytvari se dynamicky z prefabu.
Viz Faze 4.2 pro vytvoreni prefabu.

### 5.4 CardAnimationController (volitelne)

**Pripoj na:** CardView prefab
**Skript:** `CardAnimationController.cs`

Ridi animace karet (deal, play, flip, select, discard).

### Kontrolni seznam faze 5

- [ ] MariasGameManager na GameRoot GO s prirazenim cardViewPrefab
- [ ] PlayerInfoPanel na kazdem EnemyBar a PlayerInfo
- [ ] CardView prefab ulozen v Assets/Prefabs/UI/

---

## Faze 6: GameController -- herni smycka

### 6.1 MariasGameController (core, non-MonoBehaviour)

`MariasGameController` je cista C# trida (ne MonoBehaviour). Prijima `IDeckFactory` pres konstruktor -- balicek se vytvari z dat (CardDatabaseSO), ne hardcoded:

```csharp
// V MariasGameManager.cs (MonoBehaviour ve scene)
private MariasGameController _gameController;

void Start()
{
    var locator = ServiceLocator.Instance;
    var deckFactory = locator.GetDeckFactory();

    _gameController = new MariasGameController(deckFactory);
    SubscribeToEvents();
    StartNewGame();
}

void StartNewGame()
{
    var playerNames = new List<string> { "Ty", "Pepa AI", "Karel AI" };
    _gameController.StartNewGame(playerNames);
    // Controller si internre vytvori balicek pres IDeckFactory a zamicha ho
}
```

### 6.2 Event subscription (propojeni controlleru s UI, audio, VFX)

```csharp
void SubscribeToEvents()
{
    var audio = ServiceLocator.Instance.GetAudioManager();
    var vfx = ServiceLocator.Instance.GetVFXManager();

    // Herni flow
    _gameController.OnGameStarted += () => audio?.PlayCardShuffle();
    _gameController.OnCardsDealt += (playerIndex, cards) => HandleCardsDealt(playerIndex, cards);
    _gameController.OnCardPlayed += (playerIndex, card) => {
        audio?.PlayCardPlace();
        vfx?.PlayCardPlaceEffect(GetCardPosition(card));
    };
    _gameController.OnTrickWon += (winner, cards) => {
        audio?.PlayTrickWin();
        vfx?.PlayTrickWinEffect(GetTableCenter());
    };

    // Hlaseni
    _gameController.OnMarriageDeclared += (suit, pts) => {
        audio?.PlayMarriageCall();
        vfx?.PlayMarriageEffect(GetPlayerPosition());
    };
    _gameController.OnSevenDeclared += () => audio?.PlaySevenCall();
    _gameController.OnBettelDeclared += () => audio?.PlayBettelCall();
    _gameController.OnDurchDeclared += () => audio?.PlayDurchCall();

    // Konec hry
    _gameController.OnGameEnded += (winner, result) => {
        if (winner == LocalPlayerIndex) {
            audio?.PlayGameWin();
            vfx?.PlayConfettiEffect(Vector3.zero);
        } else {
            audio?.PlayGameLose();
        }
    };

    // UI
    _gameController.OnPhaseChanged += UpdatePhaseUI;
    _gameController.OnPlayerTurnStarted += HandlePlayerTurn;
    _gameController.OnScoreChanged += UpdateScore;
    _gameController.OnLegalPlaysCalculated += HighlightPlayableCards;
    _gameController.OnTrumpDeclared += ShowTrumpIndicator;
}
```

### 6.3 Vytvoreni hracu (PlayerFactory)

```csharp
// Standardni hra Marias: 1 clovek + 2 AI = 3 hraci
var players = PlayerFactory.CreateStandardGame(settingsManager, avatarManager?.GetAvatarProvider());

// Vlastni sestava
var players = PlayerFactory.CreateGamePlayers(
    humanCount: 1,
    aiCount: 2,
    settingsProvider: settingsManager,
    avatarProvider: avatarManager?.GetAvatarProvider()
);

// AI-only (pro testovani)
var players = PlayerFactory.CreateAIGame(settingsManager);
```

PlayerFactory pouziva strategie podle `aiDifficultyLevel` v GameSettings:
- `1` = `EasyAIStrategy` -- nahodne legalni tahy, vzdy passuje
- `2` = `MediumAIStrategy` -- heuristiky (trumfy, prebijeni, nejlevnejsi vitezna karta)
- `3` = `HardAIStrategy` -- pocitani odehranych karet, analyza stychu, pokrocile drazby

### 6.4 Vytvoreni a michani balicku

Balicek se vytvari data-driven z `CardDatabaseSO` pres `IDeckFactory`. Controller si ho vytvori sam:

```
CardDataManager → IDeckFactory (DeckFactoryService)
                       ↓ CreateStandardDeck()
MariasGameController(IDeckFactory)
                       ↓ StartNewGame()
                  _deckFactory.CreateStandardDeck() → IDeck (32 karet z CardDatabaseSO)
                  (IDeck as IShuffleable).Shuffle()
```

Rucni vytvoreni balicku (napr. pro testovani):
```csharp
var deckFactory = cardDataManager.GetDeckFactory();
var deck = deckFactory.CreateStandardDeck(); // 32 karet z CardDatabaseSO
if (deck is IShuffleable shuffleable)
{
    shuffleable.Shuffle();
}
```

### 6.5 UI callbacks (modaly + MarriageButton)

Drazba a vyber trumfu se resi pres **modaly** ktere vyjizdeji v prislusne fazi hry. MarriageButton je videt jen kdyz hrac vynasi a ma v ruce hlasku (K+Q stejne barvy).

**BiddingModal** -- zobrazit pri `OnPhaseChanged(GamePhase.Bidding)`, skryt po volbe:

| Button | OnClick() callback |
|--------|-------------------|
| GameButton | `OnBidGame()` -> `_gameController.MakeBid(0, BidOption.Game)` |
| SevenButton | `OnBidSeven()` -> `_gameController.MakeBid(0, BidOption.Seven)` |
| HundredButton | `OnBidHundred()` -> `_gameController.MakeBid(0, BidOption.Hundred)` |
| BettelButton | `OnBidBettel()` -> `_gameController.MakeBid(0, BidOption.Bettel)` |
| DurchButton | `OnBidDurch()` -> `_gameController.MakeBid(0, BidOption.Durch)` |
| PassButton | `OnBidPass()` -> `_gameController.MakeBid(0, BidOption.Pass)` |

**TrumpSelectionModal** -- zobrazit pri `OnPhaseChanged(GamePhase.Declaring)`, skryt po volbe:

| Button | OnClick() callback |
|--------|-------------------|
| HeartsButton | `_gameController.DeclareTrump(CardSuit.Hearts)` |
| DiamondsButton | `_gameController.DeclareTrump(CardSuit.Diamonds)` |
| ClubsButton | `_gameController.DeclareTrump(CardSuit.Clubs)` |
| SpadesButton | `_gameController.DeclareTrump(CardSuit.Spades)` |

**MarriageButton** -- zobrazit kdyz `OnPlayerTurnStarted` a hrac vynasi a ma hlasku:

| Button | OnClick() callback |
|--------|-------------------|
| MarriageButton | `_gameController.DeclareMarriage(suit)` |

### Kontrolni seznam faze 6

- [ ] MariasGameManager.cs MonoBehaviour ve scene
- [ ] MariasGameController vytvoren v Start() s `IDeckFactory` z ServiceLocator
- [ ] Vsechny eventy subscribnuty
- [ ] Vsechna UI tlacitka propojena s callbacks
- [ ] BiddingModal se zobrazuje/skryva podle faze (Bidding)
- [ ] TrumpSelectionModal se zobrazuje/skryva podle faze (Declaring)
- [ ] MarriageButton se zobrazuje jen kdyz hrac vynasi a ma hlasku

---

## Faze 7: Audio a VFX napojeni

Audio a VFX se napojuji pres eventy (viz faze 6.2). Zde je kompletni prehled volani:

### Audio -- automaticke prehravani pri eventech

| Event | Zvuk |
|-------|------|
| `OnGameStarted` | `PlayCardShuffle()` |
| `OnCardsDealt` | `PlayCardDeal()` |
| `OnCardPlayed` | `PlayCardPlace()` |
| `OnTrickWon` | `PlayTrickWin()` |
| `OnMarriageDeclared` | `PlayMarriageCall()` |
| `OnSevenDeclared` | `PlaySevenCall()` |
| `OnBettelDeclared` | `PlayBettelCall()` |
| `OnDurchDeclared` | `PlayDurchCall()` |
| `OnGameEnded` (vyhra) | `PlayGameWin()` |
| `OnGameEnded` (prohra) | `PlayGameLose()` |

### VFX -- automaticke spousteni pri eventech

| Event | Efekt | Typ |
|-------|-------|-----|
| `OnCardPlayed` | `PlayCardPlaceEffect(pos)` | One-shot |
| `OnTrickWon` | `PlayTrickWinEffect(pos)` | One-shot |
| `OnMarriageDeclared` | `PlayMarriageEffect(pos)` | One-shot |
| `OnGameEnded` (vyhra) | `PlayConfettiEffect(pos)` | One-shot |
| `OnGameEnded` (vyhra) | `PlayGameWinEffect(pos)` | One-shot |
| Card selected | `PlayCardSelectEffect(pos)` | Persistent* |
| Card playable | `PlayPlayableCardEffect(pos)` | Persistent* |

*Persistent efekty vraci `GameObject` -- musite je rucne znicit pres `vfxManager.DestroyVFX(go)`.

### Ovladani za behu

```csharp
var audio = ServiceLocator.Instance.GetAudioManager();
audio.SetMasterVolume(0.8f);
audio.SetMusicVolume(0.5f);
audio.SetSFXVolume(1.0f);

var vfx = ServiceLocator.Instance.GetVFXManager();
vfx.SetVFXEnabled(false);       // Vypnout VFX (low-end zarizeni)
vfx.SetVFXIntensity(0.5f);      // Snizit intenzitu
```

---

## Faze 8: Overeni a testovani

### 8.1 Editor tester

1. Menu: `PrsiGame > Tools > Game Setup Tester`
2. Pretahni managery do testeru
3. Klikni **Test All** -- musi projit vsechny testy

### 8.2 Runtime kontrola

Po stisknuti Play zkontroluj v Console:

1. **Zadne cervene chyby** -- vsechny reference prirazeny
2. **ServiceLocator log:** "Vsechny sluzby jsou pripraveny" (pokud ma log)
3. **Karty se zobrazuji** -- PlayerHandView vytvari CardView instance
4. **Kliknuti funguje** -- klikni na kartu, sleduj log

### 8.3 Castne problemy

| Priznak | Pricina | Reseni |
|---------|---------|--------|
| Karty se nezobrazuji | Chybi sprite v CardDataSO | Prirad sprites |
| NullReferenceException pri dealingu | Prazdny balicek (IDeckFactory chybi) | Zkontroluj CardDatabaseSO a CardThemeSO v CardDataManager |
| Kliknuti nereaguje | Chybi EventSystem | Pridej EventSystem do scene |
| Kliknuti nereaguje | Chybi GraphicRaycaster na Canvas | Pridej GraphicRaycaster |
| Kliknuti nereaguje | Chybi Button na CardView | Pridej Button komponentu |
| "CardDatabaseSO is not assigned!" | Neprirazeny SO | Prirad v inspektoru CardDataManager |
| "GameSettingsSO is not assigned!" | Neprirazeny SO | Prirad v inspektoru GameSettingsManager |
| "Sprite not found" | Sprite neni v Resources | Over cestu k assetu |
| "Expected 32 cards, found X" | Chybi karty v DB | Doplni karty do CardDatabaseSO |
| ServiceLocator nenachazi managery | Managery nemaji skripty | Prirad skripty na GO |
| UI se neaktualizuje | Nevolano UpdateAllUI() | Volej po kazde zmene |
| Ruby karet se nezobrazuji | Chybi cardBackSprite | Nastav v CardView prefabu a DeckView |
| EnemyBar se nezobrazuje | Neni v enemyBarObjects | Prirad v GameUIController |

### 8.4 Doporucene rozliseni pro testovani

| Zarizeni | Rozliseni |
|----------|-----------|
| Reference | 1080 x 1920 |
| iPhone 13/14 | 1170 x 2532 |
| Minimum | 720 x 1280 |
| Aspect ratio | 9:16 nebo 9:19.5 |

V Game View nastav custom aspect ratio.

---

# Cast 2: Architektura projektu

---

## Adresarova struktura

```
Assets/Scripts/
├── Core/                          # Herni logika (ZADNE Unity zavislosti)
│   ├── ServiceLocator.cs          # Centralni sprava sluzeb (Singleton)
│   ├── Card.cs                    # Immutable datova trida karty
│   ├── CardState.cs               # Enum: stavy karty behem hry
│   ├── CardStateMachine.cs        # State machine pro prechody karet
│   ├── CardWithState.cs           # Card + state management
│   ├── Deck.cs                    # IDeck, IShuffleable
│   ├── GameSettings.cs            # Konfiguracni data (3 hraci, 10 karet)
│   ├── PlayerBase.cs              # IPlayer, IHand, IBetManager
│   ├── MariasGameRules.cs         # Pravidla ceskeho mariase
│   ├── MariasGameState.cs         # Kompletni herni stav (serializovatelny)
│   ├── MariasGameController.cs    # Hlavni kontroler s event systemem
│   └── Interfaces/                # Interfacy
│       ├── IPlayer.cs, IHand.cs, IDeck.cs
│       ├── ICardDataProvider.cs, ICardThemeProvider.cs
│       ├── IDeckFactory.cs, ISettingsProvider.cs
│       ├── IAIStrategy.cs, IAIPlayer.cs, IHumanPlayer.cs
│       ├── ICardChooser.cs, IBetChooser.cs, IBetManager.cs
│       └── ICardCollection.cs, ICardStateMachine.cs, IShuffleable.cs
├── Game/                          # Implementace hracu a herni smycka
│   ├── MariasGameManager.cs       # Hlavni MonoBehaviour (UI + game loop)
│   ├── PlayerFactory.cs           # Factory pro vytvareni hracu (1 human + 2 AI)
│   ├── HumanPlayer.cs             # Lidsky hrac
│   ├── AIPlayer.cs                # AI hrac (pouziva IAIStrategy)
│   └── Strategies/
│       ├── EasyAIStrategy.cs      # Nahodne legalni tahy, vzdy passuje
│       ├── MediumAIStrategy.cs    # Heuristiky (trumfy, prebijeni)
│       └── HardAIStrategy.cs      # Pocitani karet, pokrocile rozhodovani
├── Managers/                      # MonoBehaviour managery (Unity vrstva)
│   ├── CardDataManager.cs         # Data karet + temata
│   ├── GameSettingsManager.cs     # Herni nastaveni
│   ├── AudioManager.cs            # Zvuky (SoundLibrary)
│   ├── VFXManager.cs              # Vizualni efekty (VFXLibrary)
│   ├── AvatarManager.cs           # Avatary hracu
│   └── EnemyManager.cs            # AI protivnici
├── Services/                      # Servisni implementace
│   ├── CardDataService.cs         # ICardDataProvider impl
│   ├── AssetLoaderService.cs      # Cachovane nacitani assetu
│   ├── DeckFactoryService.cs      # IDeckFactory impl
│   ├── CardThemeService.cs        # ICardThemeProvider impl
│   ├── SettingsService.cs         # ISettingsProvider impl
│   ├── SettingsRepository.cs      # Persistence (PlayerPrefs)
│   ├── EnemyService.cs            # IEnemyProvider impl
│   └── AvatarService.cs           # IAvatarProvider impl
├── ScriptableObjects/             # SO definice
│   ├── CardDataSO.cs              # Jedna karta (suit, rank, sprite, sounds)
│   ├── CardDatabaseSO.cs          # Kolekce 32 karet
│   ├── CardThemeSO.cs             # Tema balicku
│   ├── GameSettingsSO.cs          # Herni nastaveni
│   ├── AvatarDataSO.cs            # Jeden avatar
│   ├── AvatarDatabaseSO.cs        # Kolekce avataru
│   ├── EnemyDataSO.cs             # Jeden AI nepritel
│   ├── EnemyDatabaseSO.cs         # Kolekce neprilel
│   └── SoundDataSO.cs             # Zvukova data
├── UI/                            # UI komponenty (MonoBehaviour)
│   ├── CardView.cs                # Zobrazeni jedne karty
│   ├── CardAnimationController.cs # Animace karet
│   └── PlayerInfoPanel.cs         # Info panel hrace (jmeno, avatar, penize)
├── Editor/                        # Unity Editor nastroje
│   ├── CardDatabaseCreator.cs     # Vytvareni card databases
│   └── GameSetupTester.cs         # Testovani game setupu
└── PortraitFit.cs                 # Portrait orientace utility
```

---

## Architekturni vzory

### Service Locator (centralni sprava)

```
ServiceLocator.Instance
├── GetCardDataManager()      → CardDataManager
├── GetSettingsManager()      → GameSettingsManager
├── GetAvatarManager()        → AvatarManager
├── GetEnemyManager()         → EnemyManager
├── GetAudioManager()         → AudioManager
├── GetVFXManager()           → VFXManager
├── GetCardDataProvider()     → ICardDataProvider
├── GetCardThemeProvider()    → ICardThemeProvider
└── GetDeckFactory()          → IDeckFactory
```

Pristup: `ServiceLocator.Instance.GetAudioManager()?.PlayCardPlace();`

Event: `ServiceLocator.Instance.OnServicesReady += () => { ... };`

### State Machine (zivotni cyklus karty)

```
InDeck → Dealing → InHand → Selected → Playing → OnTable
                    InHand → Discarding → InDiscardPile
```

`CardStateMachine` validuje prechody -- nepovoleny prechod vyhodi vyjimku.

### Strategy (AI obtiznost)

```
IAIStrategy
├── EasyAIStrategy     -- Random z legalnich tahu
├── MediumAIStrategy   -- Zakladni heuristiky
└── HardAIStrategy     -- Pocitani karet + pokrocile rozhodovani
```

Metody: `ChooseCardToPlay(legalPlays, gameState)`, `ChooseBid(gameState)`, `ChooseTrumpSuit(hand)`, `ChooseCardsToDiscard(hand, gameState)`

### Factory (vytvrani objektu)

```
PlayerFactory
├── CreateHumanPlayer(settings, avatar)
├── CreateAIPlayer(settings, strategy, avatar)
├── CreateStandardGame(settings, avatarProvider)  -- 1 human + 2 AI = 3 hraci
└── CreateAIGame(settings)                        -- 3 AI (testing)

DeckFactoryService (IDeckFactory)
└── CreateStandardDeck()  -- 32 karet z CardDatabaseSO
    └── Injektovan do MariasGameController pres konstruktor

CardViewFactory
└── CreateCardView(card, parent, faceUp, interactive)
```

### Observer (eventy)

MariasGameController publikuje 40+ eventu -- UI, Audio, VFX systemy se subscribuji. Zadny system primo nereferencuje jiny. Kompletni seznam eventu viz sekce Event system.

---

## Datovy model

### Card (immutable)

```csharp
Card {
    Suit: CardSuit     // Hearts, Diamonds, Clubs, Spades
    Rank: CardRank     // Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
}
```

### CardWithState (Card + state management)

```csharp
CardWithState : Card {
    State: CardState   // InDeck, Dealing, InHand, Selected, Playing, ...
    event OnStateChanged(oldState, newState)
}
```

### MariasGameState (serializovatelny)

```csharp
MariasGameState {
    PlayerHands: List<List<Card>>    // Ruce hracu (indexovane)
    PlayerNames: List<string>
    PlayerCash: List<int>
    PlayerScores: List<int>

    CurrentTrick: List<Card>         // Aktualni stych (0-3 karty)
    TrickHistory: List<List<Card>>   // Historie vsech stychu
    Talon: List<Card>                // 2 karty talonu
    DiscardedCards: List<Card>       // Odhozene karty

    TrumpSuit: CardSuit              // Trumfova barva
    GameType: GameType               // Normal/Seven/Hundred/Bettel/Durch
    Phase: GamePhase                 // Dealing/Bidding/TakingTalon/...
    CurrentPlayerIndex: int          // Kdo je na tahu
    DealerIndex: int                 // Kdo rozdava
    ForehondIndex: int               // Forhont (rozdava+1)

    DeclaredMarriages: List<CardSuit>  // Nahlasene hlasky
    SevenDeclared: bool
    HundredDeclared: bool

    Winner: int                      // Index viteze (-1 = nerozhodnuto)
    GameResult: GameResult           // Vysledek hry
}
```

---

## Event system

Kompletni seznam eventu z `MariasGameController`:

### Herni flow

| Event | Signatura | Kdy se emituje |
|-------|-----------|----------------|
| `OnGameStarted` | `Action` | Nova hra zacala |
| `OnCardsDealt` | `Action<int, List<Card>>` | Hrac dostal karty (playerIndex, cards) |
| `OnCardPlayed` | `Action<int, Card>` | Hrac zahral kartu (playerIndex, card) |
| `OnTrickWon` | `Action<int, List<Card>>` | Stych vyhran (winnerIndex, cards) |
| `OnGameEnded` | `Action<int, GameResult>` | Hra skoncila (winnerIndex, result) |

### Deklarace a hlaseni

| Event | Signatura | Kdy se emituje |
|-------|-----------|----------------|
| `OnTrumpDeclared` | `Action<CardSuit>` | Zvolena trumfova barva |
| `OnGameTypeDeclared` | `Action<GameType>` | Zvolen typ hry |
| `OnMarriageDeclared` | `Action<CardSuit, int>` | Hlaska (barva, body: 20/40) |
| `OnSevenDeclared` | `Action` | Sedma vyhlasena |
| `OnSevenResolved` | `Action<bool>` | Sedma vyresena (uspech/neuspech) |
| `OnHundredDeclared` | `Action` | Stovka vyhlasena |
| `OnHundredResolved` | `Action<bool>` | Stovka vyresena |
| `OnBettelDeclared` | `Action` | Betl vyhlasen |
| `OnDurchDeclared` | `Action` | Durch vyhlasen |
| `OnBetDoubled` | `Action<int>` | Sazka zdvojena (novy nasobitel) |

### UI-specificke

| Event | Signatura | Kdy se emituje |
|-------|-----------|----------------|
| `OnPlayerTurnStarted` | `Action<int>` | Tah hrace zacal (playerIndex) |
| `OnScoreChanged` | `Action<int, int>` | Skore zmeneno (playerIndex, newScore) |
| `OnPhaseChanged` | `Action<GamePhase>` | Faze hry zmenena |
| `OnLegalPlaysCalculated` | `Action<List<Card>>` | Legalni tahy vypocteny |

### GamePhase enum

```
Dealing → Bidding → TakingTalon → DiscardingTalon → Declaring → Playing → Scoring → GameOver
```

---

## AI system

### Strategie

| Trida | Obtiznost | Popis |
|-------|-----------|-------|
| `EasyAIStrategy` | 1 | Nahodny vyber z legalnich tahu |
| `MediumAIStrategy` | 2 | Zakladni heuristiky (preferuje vysoke karty, trumfy) |
| `HardAIStrategy` | 3 | Pocitani odehranych karet, pokrocile rozhodovani |

### Interface

```csharp
public interface IAIStrategy
{
    Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState);
    MariasGameRules.BidOption ChooseBid(MariasGameState gameState);
    CardSuit ChooseTrumpSuit(IReadOnlyList<Card> hand);
    List<Card> ChooseCardsToDiscard(IReadOnlyList<Card> hand, MariasGameState gameState);
}
```

### Pouziti

```csharp
var strategy = PlayerFactory.CreateStrategyForDifficulty(difficultyLevel);
var aiPlayer = new AIPlayer(id, name, strategy, startingCash, avatarIndex);
Card chosen = aiPlayer.ChooseCardToPlay(legalPlays, gameState);
var bid = aiPlayer.ChooseBid(gameState);
var trump = aiPlayer.ChooseTrumpSuit();
var discard = aiPlayer.ChooseCardsToDiscard(gameState);
```

---

## Herni pravidla

### Balik a karty

- **32 karet:** 7, 8, 9, 10, J (Spodek), Q (Svrsek), K (Kral), A (Eso) ve 4 barvach
- **Sila karet:** 7 < 8 < 9 < J < Q < K < 10 < A
- **Bodove hodnoty:** A=11, 10=10, K=4, Q=3, J=2, 7/8/9=0
- **Celkem:** 90 bodu v balicku + hlasky

### Typy her

| Typ | Podminka vyhry | Trumfy |
|-----|---------------|--------|
| Normal (Hra) | 46+ bodu | Ano |
| Sedma | Vyhrat posledni stych trumfovou 7 | Ano |
| Stovka | 100+ bodu | Ano |
| Betl | Nevzit zadny stych | Ne |
| Durch | Vzit vsechny stychy | Ne |

### Hlasky (Marriages)

- Par K+Q stejne barvy = **20 bodu**
- Trumfovy par K+Q = **40 bodu**
- Hlasi se pouze kdyz hrac vynasi (je prvni ve stychu)

### Pravidla hrani

1. **Musi pridat barvu** -- pokud ma kartu vedouci barvy, musi ji zahrat
2. **Musi prebit** -- pokud pridava barvu, musi prebit aktualne nejvyssi kartu (pokud muze)
3. **Musi dat trumf** -- pokud nema barvu, musi trumpnout (pokud ma trumf)
4. **Cokoliv** -- pokud nema ani barvu ani trumf, muze zahrat libovolnou kartu

### Vitez stychu

Vyhraje nejsilnejsi karta ve vedouci barve. Pokud byly zahrany trumfy, vyhraje nejsilnejsi trumf.

### Herni flow

```
1. DEALING         Kazdy hrac dostane 10 karet, 2 karty do talonu
2. BIDDING         Hraci se stridaji v nabidkach (Hra/Sedma/Stovka/Betl/Durch/Pass)
3. TAKING TALON    Forhont si vezme 2 karty z talonu (ma 12 karet)
4. DISCARDING      Forhont odhodi 2 karty (body se mu pricitou)
5. DECLARING       Forhont zvoli trumfy (pokud nejde o Betl/Durch)
6. PLAYING         10 stychu podle pravidel priznani barvy a prebijeni
7. SCORING         Spocitani bodu, hlasek, sedmy
```

---

## Troubleshooting

### Editor nastroje

| Nastroj | Menu cesta | Ucel |
|---------|------------|------|
| Card Database Creator | `MariasGame > Tools > Create New Card Database` | Tvorba DB karet |
| Card Theme Creator | `MariasGame > Tools > Create New Theme (Database + Theme)` | DB + tema naraz |
| Game Setup Tester | `MariasGame > Tools > Game Setup Tester` | Testovani setupu |

### Caste chyby

| Chyba | Reseni |
|-------|--------|
| `CardDatabaseSO is not assigned!` | Prirad `CardDatabase.asset` nebo `CardThemeSO` v CardDataManager inspektoru |
| `GameSettingsSO is not assigned!` | Prirad `GameSettings.asset` v GameSettingsManager inspektoru |
| `Sprite not found` | Over ze sprites jsou ve spravne slozce (Resources nebo SO) |
| `Expected 32 cards, found X` | Doplni vsechny karty do CardDatabaseSO |
| Zvuky se neprehravaji | Over ze AudioSource komponenty existuji na AudioManager GO |
| VFX se nezobrazuji | Over ze VFX prefaby jsou prirazeny ve VFXLibrary |

### Debug rezimy

```csharp
// Audio debug
ServiceLocator.Instance.GetAudioManager().OnSoundPlayed += (name) =>
    Debug.Log($"[Audio] {name}");

// VFX debug
ServiceLocator.Instance.GetVFXManager().OnVFXSpawned += (name, pos) =>
    Debug.Log($"[VFX] {name} @ {pos}");

// Service Locator debug
ServiceLocator.Instance.OnServicesReady += () =>
    Debug.Log("[ServiceLocator] Vsechny sluzby pripraveny");
```

---

*Mariashkey | Unity | 2026*
