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
│ │ ├─ IWordProvider.cs
│ │ └─ RandomWordProvider.cs
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
---

## 🗺️ Roadmap (planerade funktioner)

Nedan är planerade förbättringar uppdelade i etapper. Alla bygger vidare på den nuvarande, testade kärnlogiken i `Hangman.Core`.

### 🔹 Nästa steg (kortsiktigt)
1) **Huvudmeny i konsolen**
   - Välj: *Spela*, *Svårighetsgrad*, *Visa statistik*, *Avsluta*.
   - Ren loop som anropar `ConsoleUi` och tjänster.
   - Enkel state-maskin: `MainMenu → InGame → Results → MainMenu`.

2) **Spela mot människa (lokalt)**
   - Ny UI-flöde där Spelare 1 matar in ordet (dolt eko), Spelare 2 gissar.
   - Återanvänder `Game` rakt av, bara ordkällan ändras:
     - `HumanWordProvider` som läser in ord från tangentbord (utan att skriva ut tecknen).

3) **Fler ordkällor (Strategy via IWordProvider)**
   - `FileWordProvider` – läser ord från fil (en per rad).
   - `ApiWordProvider` – hämtar ord via HTTP GET (t.ex. `random-word-api.herokuapp.com`).
   - `CombinedWordProvider` – mixar flera källor (finns design klar).

> Exempel på asynkrona signaturer:
> ```csharp
> public interface IAsyncWordProvider
> {
>     Task<string> GetWordAsync(CancellationToken ct = default);
>     string DifficultyName { get; }
> }
> ```

### 🔹 Mellan sikt
4) **Asynkron programmering**
   - Gör IO icke-blockerande:
     - `ApiWordProvider.GetWordAsync()` (HttpClient + `await`).
     - `FileWordProvider` med `File.ReadAllLinesAsync`.
   - UI:
     - `await` när ord laddas.
     - Visa “Laddar…”-indikering i konsolen.

5) **Data & statistik**
   - `IStatisticsService` som loggar resultat:
     - `Task SaveAsync(GameResult result)`
     - `Task<StatisticsSummary> GetSummaryAsync()`
   - Spara i JSON (ev. uppgradera till SQLite).
   - `IStatisticsExporter` för export (CSV):
     - `Task ExportAsync(string path, CancellationToken ct = default)`
   - Visa: vinster/förluster, snittgissningar, mest frekvent felbokstav, senaste 10 spel.

6) **Dependency Injection (lös koppling)**
   - Injicera `IWordProvider` och `IStatisticsService` i `ConsoleUi`/kompositionen i `Program.cs`.
   - (Bonus) Egen mini-DI-container:
     ```csharp
     var services = new ServiceCollection()
        .AddSingleton<IWordProvider, RandomWordProvider>()
        .AddSingleton<IStatisticsService, JsonStatisticsService>()
        .BuildServiceProvider();
     ```

7) **Logging & felhantering**
   - `ILogger`-interface + implementationer:
     - `ConsoleLogger`, `FileLogger`.
   - Centraliserad felhantering i UI:
     - Visa vänligt felmeddelande, återgå till meny.
   - Logga viktiga events (start, vinst/förlust, undantag) med tidsstämpel.

### 🔹 Lång sikt / bonus
8) **Multitrådning**
   - “Timer mode”: 60 sekunder per ord (`System.Timers.Timer` eller `CancellationTokenSource`).
   - Parallell “hänggubbe-animation” med `Task.Run()`.
   - Tråd-säker statistik & loggning.

9) **GUI-version (WPF/WinUI)**
   - Samma `Hangman.Core`-logik återanvänds rakt av.
   - MVVM + Data Binding till `Game`-state.
   - Visuell gubbe, knapprad A–Ö, statuspanel.

10) **Internationellt stöd**
   - `CultureInfo`-medveten normalisering (ÅÄÖ).
   - Språkfiler för UI (sv/eng).
   - Ordlistor per språk.

---

## ✨ Förslag på API-kontrakt (utdrag)

```csharp
public record GameResult(
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    string Word,
    int Mistakes,
    GameStatus Status
);

public interface IStatisticsService
{
    Task SaveAsync(GameResult result, CancellationToken ct = default);
    Task<StatisticsSummary> GetSummaryAsync(CancellationToken ct = default);
}

public record StatisticsSummary(
    int TotalGames,
    int Wins,
    int Losses,
    double AverageGuesses,
    char? MostMissedLetter
);

public interface IStatisticsExporter
{
    Task ExportAsync(string path, CancellationToken ct = default);
}


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