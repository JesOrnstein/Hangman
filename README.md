# 🎮 Hangman

Ett avancerat **C#-projekt** byggt som ett komplett konsolspel av typen *Hänga Gubbe*, utvecklat med fokus på **ren arkitektur**, **testdriven utveckling (TDD)** och **hög kodkvalitet**.

---

## 📁 Projektstruktur

Lösningen består av tre separata projekt för tydlig ansvarsfördelning:

| Projekt | Typ | Syfte |
|----------|------|--------|
| **Hangman.Core** | Class Library | Innehåller all spel-logik, status och ordhantering. |
| **Hangman.Console** | Console App | Körbara spelet med användargränssnitt i konsolen. |
| **HangmanTest** | xUnit Test Project | Enhetstester för `Game` och andra delar av Core-projektet. |

---

### 🧱 Mappstruktur
```
Hangman/
├─ Hangman.Core/
│ ├─ Game.cs
│ ├─ GameStatus.cs
│ ├─ Providers/
│ │ ├─ Api/
│ │ │ ├─ ApiDifficulty.cs
│ │ │ ├─ ApiWordProvider.cs
│ │ │ └─ IAsyncWordProvider.cs
│ │ └─ Local/
│ │   ├─ IWordProvider.cs
│ │   └─ WordProvider.cs
│
├─ Hangman.Console/
│ ├─ Program.cs
│ └─ ConsoleUi.cs
│
└─ HangmanTest/
  └─ GameTests.cs
```
---

## ⚙️ Funktioner (hittills)

Kärnlogiken är komplett och testad, och gränssnittet har utökats med fullt stöd för asynkrona ordkällor.

- **Huvudmeny:** Användaren kan välja mellan att Spela och Avsluta.
- **Asynkron Ordhantering:** Stöd för att hämta ord från externa källor utan att blockera tråden, via `IAsyncWordProvider`.
- **API-integration:** Använder `ApiWordProvider` för att hämta slumpmässiga ord från [https://random-word-api.herokuapp.com/word](https://random-word-api.herokuapp.com/word).
- **Svårighetsgrader:** API-ord kan hämtas baserat på ordlängd med valen Lätt (3-4 bokstäver), Medium (5-7) och Svår (8-11).
- Starta nytt spel via `StartNew()`
- Hantera gissningar med `Guess(char)`
- Automatisk vinst- och förlustlogik
- Events för `LetterGuessed`, `WrongLetterGuessed` och `GameEnded`
- Maskerat ord med `GetMaskedWord()`

---

## 🧪 Testning

Projektet använder **xUnit**.  
Alla tester finns i `HangmanTest/GameTests.cs` och täcker:
- Initiering av spel
- Rätt och fel gissningar
- Dubbelgissningar
- Vinst- och förlustvillkor
- Eventhantering
- Edge cases (tomma ord, specialtecken, case-insensitivity)

---

## 🧠 Använd teknik

.NET 8

C# 12

xUnit

TDD (Test Driven Development)

Events och delegates

HashSet, IEnumerable och immutabla collections

Clean architecture & separation of concerns

API - [https://random-word-api.herokuapp.com/home](https://random-word-api.herokuapp.com/home)

---

### 🧩 Avancerade C#-koncept som används

| Område | Exempel | Förklaring |
|---------|----------|------------|
| **Asynchronous Programming** | `Task<string> GetWordAsync()`, `await ui.RunAsync()` | Hela applikationsflödet och API-anrop hanteras asynkront för skalbarhet och responsivitet. |
| **Design Patterns** | *Strategy Pattern* via `IAsyncWordProvider` | Gör det möjligt att byta ordkälla (API, lokal fil, etc.) utan att ändra spel-logiken. |
| **Events & Delegates** | `LetterGuessed`, `WrongLetterGuessed`, `GameEnded` | Händelser som UI kan prenumerera på för att reagera på speländringar. |
| **Collections & Generics** | `HashSet<char>`, `IReadOnlyCollection<char>` | Effektiv hantering av använda bokstäver och dubblettkontroll. |
| **Exception Handling** | `ArgumentException`, `InvalidOperationException` | Säkerställer stabilitet vid ogiltig indata eller nätverksfel. |

---
---




