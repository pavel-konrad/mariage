# 🎴 Mariáš - Designer Guide

Komplexní návod pro designéry (zvuk, animace, UI, VFX) jak pracovat s herním systémem.

---

## 📁 Struktura projektu

```
Assets/Scripts/
├── Core/                    # Herní logika (bez Unity závislostí)
│   ├── MariasGameRules.cs   # Pravidla českého mariáše
│   ├── MariasGameState.cs   # Herní stav
│   ├── MariasGameController.cs # Hlavní herní kontrolér s eventy
│   ├── Card.cs              # Datová třída karty
│   └── ServiceLocator.cs    # Centrální správa služeb
├── Managers/                # Unity managery
│   ├── AudioManager.cs      # 🔊 Zvuky
│   ├── VFXManager.cs        # ✨ Vizuální efekty
│   ├── CardDataManager.cs   # Data karet
│   └── ...
├── UI/                      # UI komponenty
│   ├── CardView.cs          # Zobrazení karty
│   ├── CardAnimationController.cs # 🎬 Animace
│   └── ...
└── ScriptableObjects/       # Konfigurovatelná data
```

---

## 🔊 ZVUKAŘ (Audio Designer)

### AudioManager - Hlavní zvukový systém

**Lokace:** `Managers/AudioManager.cs`

### Jak přidat zvuky

1. **Vytvořte AudioClip assety** (WAV/OGG/MP3)
2. **Přiřaďte do SoundLibrary** v inspektoru AudioManageru

### Dostupné zvukové sloty

```
📁 Card Sounds (SoundLibrary.cardSounds)
├── cardPlace      - Položení karty na stůl
├── cardDraw       - Lížení karty z balíčku
├── cardShuffle    - Míchání karet
├── cardDeal       - Rozdávání karet
└── cardFlip       - Otočení karty

📁 Game Sounds (SoundLibrary.gameSounds)
├── trickWin       - Výhra štychu
├── gameWin        - Výhra celé hry
├── gameLose       - Prohra hry
├── marriageCall   - Hlášení (20/40 bodů)
├── sevenCall      - Hlášení sedmy
├── bettelCall     - Vyhlášení betlu
└── durchCall      - Vyhlášení durchu

📁 UI Sounds (SoundLibrary.uiSounds)
├── buttonClick    - Kliknutí na tlačítko
├── buttonHover    - Najetí na tlačítko
└── notification   - Notifikace/upozornění
```

### Jak volat zvuky z kódu

```csharp
// Přes ServiceLocator (doporučeno)
ServiceLocator.Instance.GetAudioManager()?.PlayCardPlace();

// Přímo na AudioManager instanci
audioManager.PlayCardPlace();
audioManager.PlayTrickWin();
audioManager.PlayMarriageCall();
```

### Subscribnutí na herní eventy

```csharp
// V libovolném MonoBehaviour
void Start()
{
    var gameController = /* získat referenci */;
    var audioManager = ServiceLocator.Instance.GetAudioManager();
    
    // Automatické přehrávání zvuků při událostech
    gameController.OnCardPlayed += (playerIndex, card) => audioManager.PlayCardPlace();
    gameController.OnTrickWon += (winner, cards) => audioManager.PlayTrickWin();
    gameController.OnMarriageDeclared += (suit, points) => audioManager.PlayMarriageCall();
    gameController.OnSevenDeclared += () => audioManager.PlaySevenCall();
    gameController.OnGameEnded += (winner, result) => {
        if (winner == localPlayerIndex)
            audioManager.PlayGameWin();
        else
            audioManager.PlayGameLose();
    };
}
```

### Ovládání hlasitosti

```csharp
audioManager.SetMasterVolume(0.8f);  // 0-1
audioManager.SetMusicVolume(0.5f);   // 0-1
audioManager.SetSFXVolume(1.0f);     // 0-1
```

### Registrace vlastních zvuků

```csharp
// Pro speciální zvuky, které nejsou v SoundLibrary
audioManager.RegisterCustomSound("special_combo", myAudioClip);
audioManager.PlayCustomSound("special_combo");
```

---

## 🎬 ANIMÁTOR (Animation Designer)

### CardAnimationController - Animace karet

**Lokace:** `UI/CardAnimationController.cs`

### Jak přidat animace

1. **Vytvořte Animator Controller** pro karty
2. **Přidejte animation states** pro každý typ animace
3. **Nastavte triggery/parametry** podle tabulky níže

### Animator Parametry (Triggery)

| Parametr | Typ | Popis |
|----------|-----|-------|
| `Deal` | Trigger | Animace rozdávání karty |
| `Discard` | Trigger | Animace odhození karty |
| `Play` | Trigger | Animace zahrání karty |
| `Flip` | Trigger | Animace otočení karty |
| `Select` | Trigger | Animace výběru karty |
| `FaceUp` | Bool | Zda je karta lícem nahoru |
| `Speed` | Float | Rychlost animace |

### Animator States (doporučené)

```
📁 Card Animator Controller
├── Idle           - Výchozí stav
├── Dealing        - Animace příjezdu karty do ruky
├── Selected       - Karta je vybraná (zvýrazněná)
├── Playing        - Karta letí na stůl
├── Discarding     - Karta mizí do odhazovacího balíčku
├── Flipping       - Otáčení karty
└── InHand         - Karta v ruce (idle)
```

### Jak volat animace z kódu

```csharp
// CardAnimationController je na CardView prefabu
cardView.GetComponent<CardAnimationController>()?.PlayDealAnimation();
cardView.GetComponent<CardAnimationController>()?.PlaySelectAnimation();
cardView.GetComponent<CardAnimationController>()?.PlayFlipAnimation(faceUp: true);
```

### Automatické triggery

Animace se spouští automaticky při změně stavu karty:

```csharp
// V CardView.cs - OnCardStateChanged
// Toto už je implementováno, stačí nastavit Animator
switch (newState)
{
    case CardState.Dealing:
        animationController.PlayDealAnimation();
        break;
    case CardState.Selected:
        animationController.PlaySelectAnimation();
        break;
    case CardState.Playing:
        animationController.PlayPlayAnimation();
        break;
    // ...
}
```

### Fallback AnimationClips

Pokud nemáte Animator Controller, můžete přiřadit AnimationClips přímo:

```
Inspector: CardAnimationController
├── dealAnimation      [AnimationClip]
├── discardAnimation   [AnimationClip]
├── playAnimation      [AnimationClip]
├── flipAnimation      [AnimationClip]
└── selectAnimation    [AnimationClip]
```

---

## ✨ VFX ARTIST (Visual Effects Designer)

### VFXManager - Systém vizuálních efektů

**Lokace:** `Managers/VFXManager.cs`

### Jak přidat VFX

1. **Vytvořte VFX prefaby** (ParticleSystem, VFX Graph, nebo custom)
2. **Přiřaďte do VFXLibrary** v inspektoru VFXManageru

### Dostupné VFX sloty

```
📁 Card Effects (VFXLibrary)
├── cardPlaceEffect     - Efekt při položení karty
├── cardDrawEffect      - Efekt při lížení karty
├── cardFlipEffect      - Efekt při otočení
├── cardSelectEffect    - Highlight vybrané karty (persistent)
└── playableCardEffect  - Glow hratelné karty (persistent)

📁 Game Effects
├── trickWinEffect      - Efekt výhry štychu
├── gameWinEffect       - Velký efekt výhry hry
├── gameLoseEffect      - Efekt prohry
├── marriageEffect      - Efekt hlášení (srdíčka/korunky)
├── sevenEffect         - Efekt sedmy
├── bettelEffect        - Efekt betlu
├── durchEffect         - Efekt durchu
└── confettiEffect      - Konfety pro velké výhry

📁 UI Effects
├── buttonHighlightEffect   - Highlight tlačítka
├── notificationEffect      - Efekt notifikace
├── scoreIncreaseEffect     - +body efekt
└── scoreDecreaseEffect     - -body efekt
```

### Jak volat VFX z kódu

```csharp
var vfxManager = ServiceLocator.Instance.GetVFXManager();

// One-shot efekty (automaticky se zničí)
vfxManager.PlayCardPlaceEffect(cardPosition);
vfxManager.PlayTrickWinEffect(tableCenter);
vfxManager.PlayConfettiEffect(screenCenter);

// Persistent efekty (vrací GameObject, musíte zničit ručně)
GameObject highlight = vfxManager.PlayCardSelectEffect(cardPosition);
// ... později ...
vfxManager.DestroyVFX(highlight);

// Score efekty
vfxManager.PlayScoreChangeEffect(scorePosition, isPositive: true);
```

### Subscribnutí na herní eventy

```csharp
void Start()
{
    var gameController = /* získat referenci */;
    var vfxManager = ServiceLocator.Instance.GetVFXManager();
    
    gameController.OnCardPlayed += (playerIndex, card) => {
        var cardPosition = GetCardWorldPosition(card);
        vfxManager.PlayCardPlaceEffect(cardPosition);
    };
    
    gameController.OnTrickWon += (winner, cards) => {
        vfxManager.PlayTrickWinEffect(GetTableCenter());
    };
    
    gameController.OnMarriageDeclared += (suit, points) => {
        vfxManager.PlayMarriageEffect(GetPlayerPosition(currentPlayer));
    };
    
    gameController.OnGameEnded += (winner, result) => {
        if (winner == localPlayerIndex)
            vfxManager.PlayGameWinEffect(Vector3.zero);
    };
}
```

### Nastavení VFX

```csharp
// Zapnout/vypnout VFX
vfxManager.SetVFXEnabled(false);

// Nastavit intenzitu (0-1)
vfxManager.SetVFXIntensity(0.5f);
```

### Doporučení pro VFX prefaby

1. **Používejte ParticleSystem** s `Play On Awake = true`
2. **Nastavte správnou délku** - efekt se automaticky zničí po dokončení
3. **Testujte různé intensity** - VFXManager škáluje `startSize`
4. **Canvas efekty** - pro UI efekty použijte World Space Canvas

---

## 🎨 UI DESIGNER

### Herní eventy pro UI

**Lokace:** `Core/MariasGameController.cs`

### Dostupné eventy

```csharp
// Herní flow
event Action OnGameStarted;
event Action<int, List<Card>> OnCardsDealt;          // playerIndex, cards
event Action<int, Card> OnCardPlayed;                 // playerIndex, card
event Action<int, List<Card>> OnTrickWon;            // winnerIndex, cards
event Action<int, MariasGameRules.GameResult> OnGameEnded; // winnerIndex, result

// Hlášení
event Action<CardSuit, int> OnMarriageDeclared;      // suit, points
event Action<MariasGameRules.GameType> OnGameTypeDeclared;
event Action<CardSuit> OnTrumpDeclared;
event Action<int> OnBetDoubled;                       // newMultiplier
event Action OnSevenDeclared;
event Action<bool> OnSevenResolved;                   // won
event Action OnHundredDeclared;
event Action<bool> OnHundredResolved;                 // won
event Action OnBettelDeclared;
event Action OnDurchDeclared;

// UI specific
event Action<int> OnPlayerTurnStarted;
event Action<int, int> OnScoreChanged;               // playerIndex, newScore
event Action<GamePhase> OnPhaseChanged;
event Action<List<Card>> OnLegalPlaysCalculated;
```

### Příklad UI subscribování

```csharp
public class GameUI : MonoBehaviour
{
    [SerializeField] private Text turnIndicator;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject marriagePopup;
    
    private MariasGameController _gameController;
    
    void Start()
    {
        _gameController = /* získat referenci */;
        
        _gameController.OnPlayerTurnStarted += ShowTurnIndicator;
        _gameController.OnScoreChanged += UpdateScore;
        _gameController.OnMarriageDeclared += ShowMarriagePopup;
        _gameController.OnPhaseChanged += UpdatePhaseUI;
        _gameController.OnLegalPlaysCalculated += HighlightPlayableCards;
    }
    
    void ShowTurnIndicator(int playerIndex)
    {
        turnIndicator.text = $"Na tahu: Hráč {playerIndex + 1}";
    }
    
    void UpdateScore(int playerIndex, int newScore)
    {
        scoreText.text = $"Skóre: {newScore}";
    }
    
    void ShowMarriagePopup(CardSuit suit, int points)
    {
        marriagePopup.SetActive(true);
        marriagePopup.GetComponentInChildren<Text>().text = 
            $"Hláška! {GetSuitName(suit)} - {points} bodů";
    }
    
    void UpdatePhaseUI(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Bidding:
                ShowBiddingUI();
                break;
            case GamePhase.Playing:
                ShowPlayingUI();
                break;
            case GamePhase.GameOver:
                ShowResultsUI();
                break;
        }
    }
    
    void HighlightPlayableCards(List<Card> legalPlays)
    {
        // Zvýraznit karty, které může hráč zahrát
        foreach (var cardView in playerHandView.CardViews)
        {
            bool isPlayable = legalPlays.Contains(cardView.Card);
            cardView.SetHighlight(isPlayable);
        }
    }
}
```

### CardView stavy

```csharp
public enum CardState
{
    InDeck,          // V balíčku (rub)
    Dealing,         // Rozdávání (animace)
    InHand,          // V ruce hráče (líc)
    Selected,        // Vybraná karta
    Playing,         // Hraní karty (animace)
    Discarding,      // Odhazování (animace)
    InDiscardPile,   // V odhazovacím balíčku
    OnTable          // Na stole (štych)
}
```

---

## 🎮 HERNÍ FLOW MARIÁŠE

```
1. DEALING (Rozdávání)
   └─► Každý hráč dostane 10 karet, 2 karty do talonu

2. BIDDING (Dražba)
   └─► Hráči se střídají v nabídkách (Hra/Sedma/Stovka/Betl/Durch/Pass)

3. TAKING TALON (Braní talonu)
   └─► Forhont si vezme 2 karty z talonu (má 12 karet)

4. DISCARDING (Odhazování)
   └─► Forhont odhodí 2 karty (body se mu přičtou)

5. DECLARING (Hlášení)
   └─► Forhont zvolí trumfy (pokud nejde o Betl/Durch)

6. PLAYING (Hraní)
   └─► 10 štychů, pravidla přiznání barvy a přebíjení

7. SCORING (Bodování)
   └─► Spočítání bodů, hlášek, sedmy
```

---

## 📋 CHECKLIST PRO IMPLEMENTACI

### Zvukař
- [ ] Vytvořit SoundLibrary ScriptableObject
- [ ] Nahrát/vytvořit zvuky pro všechny sloty
- [ ] Přiřadit AudioClips do AudioManageru
- [ ] Nastavit výchozí hlasitosti
- [ ] Otestovat všechny zvuky ve hře

### Animátor
- [ ] Vytvořit Animator Controller pro karty
- [ ] Vytvořit animace: Deal, Select, Play, Discard, Flip
- [ ] Nastavit triggery a transitions
- [ ] Přiřadit Animator do CardView prefabu
- [ ] Otestovat všechny přechody

### VFX Artist
- [ ] Vytvořit VFX prefaby pro všechny efekty
- [ ] Nastavit ParticleSystem parametry
- [ ] Přiřadit prefaby do VFXManageru
- [ ] Otestovat efekty na různých pozicích
- [ ] Optimalizovat pro mobilní zařízení

### UI Designer
- [ ] Navrhnout layout pro všechny fáze hry
- [ ] Implementovat UI pro dražbu
- [ ] Implementovat UI pro hraní
- [ ] Implementovat UI pro výsledky
- [ ] Subscribnout na herní eventy
- [ ] Přidat animace UI elementů

---

## 🔧 TIPY A TRIKY

### ServiceLocator

```csharp
// Vždy používejte ServiceLocator pro přístup k managerům
var audio = ServiceLocator.Instance.GetAudioManager();
var vfx = ServiceLocator.Instance.GetVFXManager();

// Můžete se subscribnout na event když jsou služby ready
ServiceLocator.Instance.OnServicesReady += () => {
    Debug.Log("Všechny služby jsou připraveny!");
};
```

### Null-safe volání

```csharp
// Používejte ?. operátor pro bezpečné volání
ServiceLocator.Instance.GetAudioManager()?.PlayCardPlace();
ServiceLocator.Instance.GetVFXManager()?.PlayTrickWinEffect(position);
```

### Debug režim

```csharp
// V AudioManageru a VFXManageru jsou eventy pro debugging
audioManager.OnSoundPlayed += (soundName) => Debug.Log($"Zvuk: {soundName}");
vfxManager.OnVFXSpawned += (effectName, pos) => Debug.Log($"VFX: {effectName} @ {pos}");
```

---

## ❓ FAQ

**Q: Jak přidám nový typ zvuku?**
A: Přidejte nový AudioClip slot do SoundLibrary a novou metodu do AudioManageru.

**Q: Jak změním rychlost animací?**
A: Použijte `CardAnimationController.SetAnimationSpeed(float speed)`.

**Q: Jak vypnu VFX pro low-end zařízení?**
A: Zavolejte `VFXManager.SetVFXEnabled(false)` nebo `SetVFXIntensity(0.5f)`.

**Q: Jak přidám lokalizované zvuky?**
A: Použijte `RegisterCustomSound()` s různými jazykovými variantami.

---

*Vytvořeno pro projekt Mariáš | 2026*
