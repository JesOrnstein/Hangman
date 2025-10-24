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

Hangman/
├─ Hangman.Core/
│ ├─ Game.cs
│ ├─ GameStatus.cs
│ ├─ Providers/
│ │ ├─ IWordProvider.cs
│ │ └─ RandomWordProvider.cs
│
├─ Hangman.Console/
│ ├─ Program.cs
│ └─ ConsoleUi.cs
│
└─ HangmanTest/
└─ GameTests.cs




---

## ⚙️ Funktioner (hittills)
- Starta nytt spel via `StartNew()`
- Hantera gissningar med `Guess(char)`
- Automatisk vinst- och förlustlogik
- Events för `LetterGuessed`, `WrongLetterGuessed` och `GameEnded`
- Maskerat ord med `GetMaskedWord()`
- Testad enligt AAA-mönstret (Arrange–Act–Assert)

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

---

### 🧩 Avancerade C#-koncept som används
| Område | Exempel | Förklaring |
|---------|----------|------------|
| **Events & Delegates** | `LetterGuessed`, `WrongLetterGuessed`, `GameEnded` | Händelser som UI kan prenumerera på för att reagera på speländringar. |
| **Collections & Generics** | `HashSet<char>`, `IReadOnlyCollection<char>` | Effektiv hantering av använda bokstäver och dubblettkontroll. |
| **Exception Handling** | `ArgumentException`, `InvalidOperationException` | Säkerställer stabilitet vid ogiltig indata. |
| **Encapsulation** | `private set;` och readonly-fält | Skyddar intern spelstate från extern manipulation. |
| **Immutability** | `IReadOnlyCollection` för `UsedLetters` | Förhindrar oavsiktlig förändring av data utifrån. |
| **Asynchronous Programming** | `await ui.RunAsync()` i `Program.cs` | Konsolgränssnittet körs asynkront för framtida utbyggnad. |
| **Design Patterns** | *Strategy Pattern* via `IWordProvider` | Gör det möjligt att byta ordkälla utan att ändra spel-logiken. |
| **Test Driven Development (TDD)** | `GameTests.cs` | Alla metoder i `Game` har utvecklats och verifierats genom enhetstester. |

---

// Gammal v1.0 checklista
1. Högkvalietet av kod 
+Hyfsat bra
-Read me ej fixat
2. Build andrun wothout critical bugs
+ utan problem
3. Apply advanced C# concepts
+Collections & generics
+ Delegastes/events/lambdas
+ Async
4. Unit tests for some part of the code
Ej gjort
5. At least one design pattern
+ Strategy
+Observer
6. Mindful memory usage
+ Små samlingar, inga onödiga
allokeringar
Kan använda HashSet genom Clear() vid ny runda.
7. README File how to build & run the project
Ej gjort