# Pipeline: Bind Card Data a Card Database

## Cíl
Nastavit datovou vrstvu tak, aby hra:
- načetla 32 karet z `CardDatabase`,
- vytvořila balíček přes `DeckFactoryService`,
- běžela bez chyb o chybějících databázích.

## Zdroj pravdy v kódu
- `Assets/Scripts/Managers/CardDataManager.cs`
- `Assets/Scripts/ScriptableObjects/CardData.cs`
- `Assets/Scripts/ScriptableObjects/CardDatabase.cs`
- `Assets/Scripts/ScriptableObjects/CardThemeConfig.cs`
- `Assets/Scripts/Game/GameBootstrapper.cs`

Poznámka: starší README používá historické názvy `*SO` a `PrsiGame`. Aktuálně používej `CardData`, `CardDatabase`, `CardThemeConfig` a menu `MariasGame`.

## Pipeline krok za krokem

### 1) Vytvoř data karet
1. `Create > MariasGame > Card Data`
2. Vytvoř 32 assetů (4 barvy, 7-A).
3. U každé karty vyplň:
   - Suit
   - Rank
   - `IsInGame = true`
   - Sprite

### 2) Slož CardDatabase
1. `Create > MariasGame > Card Database`
2. Pojmenuj například `CardDatabase_Classic`.
3. Do pole `Cards` přetáhni všech 32 `CardData` assetů.
4. Zkontroluj warningy v Inspectoru (`Expected 32 cards, found X` nesmí zůstat).

### 3) Vytvoř téma (doporučeno)
1. `Create > MariasGame > Card Theme Config`
2. Vyplň minimálně:
   - `ThemeName`
   - `CardDatabase` (asset z kroku 2)
   - `CardBackSprite`
3. Volitelně doplň `ThemePreview` a popis.

### 4) Nabinduj scénu
Na objektu `Managers/CardDataManager`:
- Preferovaně:
  - `Available Themes` = seznam `CardThemeConfig`
  - `Default Theme` = výchozí `CardThemeConfig`
- Fallback:
  - `Card Database` v sekci Legacy

Na objektu s `GameBootstrapper`:
- Ověř referenci na správný `CardDataManager`.

### 5) Ověř funkčnost
1. Spusť Play Mode.
2. Konzole musí být bez chyb:
   - `No valid theme or card database found`
   - `No card database or themes assigned`
3. Ověř deck creation:
   - `MariasGame > Tools > Game Setup Tester`
   - `Test Deck Creation` nebo `Test All`

## Kontrolní checklist pro studenty
- `CardDatabase` obsahuje 32 validních karet se sprity.
- `CardThemeConfig` má navázaný `CardDatabase` a `CardBackSprite`.
- `CardDataManager` je správně nakonfigurovaný (theme path nebo legacy fallback).
- Play Mode běží bez červených chyb v konzoli pro card data/deck.

## Nejčastější chyby
- `No valid theme or card database found`
  - Theme nemá nastavený `CardDatabase` nebo není `Default Theme`.
- `Expected 32 cards, found X`
  - Databáze není kompletní.
- Prázdný deck při startu
  - `CardDataManager` není správně navázaný ve scéně nebo data nejsou validní.
