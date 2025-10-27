# 🎮 Hangman

Ett avancerat C#-projekt byggt som ett komplett Hänga Gubbe-spel, nu med stöd för både konsol och ett grafiskt WPF (MVVM)-gränssnitt.

Projektet är utvecklat med fokus på ren arkitektur (Separation of Concerns), MVVM, testdriven utveckling (TDD) och flerspråksstöd.

---

## 📁 Projektstruktur

Lösningen är uppdelad i fyra projekt för tydlig ansvarsfördelning:

| Projekt | Typ | Syfte |
|---|---|---|
| `Hangman.Core` | Class Library | Kärnlogik, spelregler, databasmodeller, providers (ord/statistik) och lokaliseringsstöd. |
| `Hangman.Console` | Console App | Det körbara konsol-baserade spelet. |
| `Hangman.WPF` | WPF App | **NYTT:** Grafiskt gränssnitt (GUI) byggt med MVVM-arkitekturen. |
| `HangmanTest` | xUnit Test Project | Enhetstester för `Hangman.Core`. |

---

### 🧱 Mappstruktur
```
Hangman/
├─ Hangman.Core/
│ ├─ Game.cs                # Kärnlogik för en spelrunda
│ ├─ TwoPlayerGame.cs       # Logik för turneringsläge
│ ├─ Models/              # Datamodeller (HighscoreEntry, CustomWordEntry, etc.)
│ ├─ Providers/
│ │ ├─ Db/                # EF Core (HangmanDbContext, SqliteHangmanService)
│ │ ├─ Api/               # ApiWordProvider
│ │ └─ Local/             # Lokal/Anpassad ordprovider
│ └─ Localizations/       # Språkstöd (IUiStrings, SwedishUiStrings, etc.)
│
├─ Hangman.Console/
│ ├─ Program.cs
│ ├─ GameController.cs      # Huvud-loop för konsolappen
│ ├─ ConsoleInput.cs      # Hanterar inmatning
│ └─ ConsoleRenderer.cs   # Hanterar all rendering
│
├─ Hangman.WPF/
│ ├─ App.xaml.cs            # Startpunkt, sätter upp DI/Localization
│ ├─ Views/                 # Alla XAML-vyer (MainWindow, GameView, MenuView...)
│ └─ ViewModels/            # All UI-logik (MainViewModel, GameViewModel, etc.)
│
└─ HangmanTest/
  └─ GameTests.cs
```
---

## 🚀 Kom igång (Build & Run)

Här är instruktionerna för att bygga och köra projektet.

### Förutsättningar

* **.NET 8 SDK:** Projektet är byggt med `net8.0`.
* **Visual Studio 2022 (Rekommenderat):** Inkludera arbetsbelastningen ".NET desktop development" för WPF.
* **Windows-dator:** Krävs för att köra WPF-applikationen.

---

### Köra via Visual Studio 2022 (Rekommenderat)

1.  Klona repot.
2.  Öppna `Hangman.sln`-filen i Visual Studio.
3.  Välj vilket projekt du vill köra:
    * **För konsol-versionen:** Högerklicka på `Hangman.Console`-projektet i Solution Explorer och välj "Set as Startup Project".
    * **För WPF-versionen:** Högerklicka på `Hangman.WPF`-projektet i Solution Explorer och välj "Set as Startup Project".
4.  Tryck på **Start** (F5) för att bygga och köra.

### Köra via kommandoraden (dotnet CLI)

Du kan köra båda applikationerna direkt från din terminal.

**Köra Konsol-versionen:**
```bash
# Navigera till konsol-projektets mapp
cd Hangman/Hangman.Console

# Kör applikationen
dotnet run

Köra WPF-versionen:

# Navigera till WPF-projektets mapp
cd Hangman.WPF

# Kör applikationen
dotnet run

### Databashantering

Databasen (Hangman.db) skapas och konfigureras automatiskt vid första körningen. 
HangmanDbContext använder Database.EnsureCreated() för att skapa filen i bin/Debug/net8.0-mappen. Ingen manuell migrering eller setup krävs.

⚙️ Funktioner
Dubbla Gränssnitt: Spela antingen i ett klassiskt konsolfönster eller i ett modernt WPF-gränssnitt.

WPF (MVVM): En fullt fungerande GUI-applikation byggd med Model-View-ViewModel-arkitektur, vilket separerar UI (View) från logik (ViewModel).

Flerspråksstöd (i18n): Hela applikationen (både konsol och WPF) kan växla mellan svenska och engelska med hjälp av ett Strategy Pattern (IUiStrings).

Databas (SQLite): Använder Entity Framework Core 8 för att spara highscores och anpassade ord i en lokal SQLite-databas (Hangman.db).

Highscore-system: Sparar "consecutive wins" per spelare och svårighetsgrad i databasen.

Anpassade Ordlistor: Användare kan lägga till egna ord (på svenska eller engelska) via gränssnittet, vilka sparas permanent i databasen.

Turneringsläge: Ett 2-spelarläge där spelare tävlar mot varandra med 3 "liv" var.

Speltimer: Varje runda (både enspelare och turnering) har en 60-sekunders timer.

Asynkron Ordhantering: Hämtar ord asynkront från olika källor via IAsyncWordProvider (API, lokal lista, databas).

API-integration: Hämtar engelska ord från ett externt API.

Ren Konsol-arkitektur: Konsolappen är uppdelad i ConsoleInput och ConsoleRenderer för bättre Separation of Concerns.

🧪 Testning
Projektet använder xUnit. Alla tester finns i HangmanTest/GameTests.cs och täcker:

Initiering av spel

Rätt och fel gissningar

Dubbelgissningar

Vinst- och förlustvillkor

Eventhantering

Edge cases (tomma ord, specialtecken, case-insensitivity)

🧠 Använd teknik
.NET 8 & C# 12: Hela lösningen (Core, Console, WPF och Tester) är byggd på den senaste .NET 8-plattformen och använder moderna C# 12-funktioner som required-medlemmar i datamodeller.

Dubbla UI-Ramverk:

WPF (Windows Presentation Foundation): Ett modernt, grafiskt gränssnitt för Windows. Hela Hangman.WPF-projektet är dedikerat till detta.

Konsolapplikation: Ett klassiskt textbaserat gränssnitt som körs i terminalen.

Arkitektur & Designmönster:

MVVM (Model-View-ViewModel): Arkitekturen som driver hela WPF-applikationen.

View: XAML-filerna (GameView.xaml, MenuView.xaml, etc.) definierar hur UI:t ser ut.

ViewModel: Klasser som GameViewModel.cs och MenuViewModel.cs innehåller all UI-logik och binder data till vyerna.

Model: Kärnklasserna från Hangman.Core (som Game.cs och HighscoreEntry.cs) agerar modeller.

Clean Architecture (Separation of Concerns): Strikt uppdelning av ansvar:

Hangman.Core: Innehåller all affärslogik, databasåtkomst och spelregler. Vet inget om UI.

Hangman.Console: Hanterar enbart in- och utmatning för konsolen.

Hangman.WPF: Hanterar enbart den grafiska presentationen och användarinteraktion.

Strategy Pattern: Används på två viktiga platser för att göra systemet utbytbart:

Ord-källor: IAsyncWordProvider låter spellogiken hämta ord utan att veta varifrån de kommer (API, lokal fil eller databas).

Lokalisering (i18n): IUiStrings låter hela applikationen byta språk (mellan SwedishUiStrings.cs och EnglishUiStrings.cs) genom att byta ut en strategi-implementation.

Dependency Injection (Manuell): I stället för ett tungt ramverk "injiceras" tjänster (beroenden) manuellt vid start. Både App.xaml.cs (för WPF) och Program.cs (för Konsol) skapar instanser av IStatisticsService och LocalizationProvider och skickar dem till de ViewModels och Controllers som behöver dem.

Databas:

Entity Framework Core 8: Används som ORM (Object-Relational Mapper) för all databaskommunikation.

SQLite: En lättvikts-databas som lagrar all data (highscores och anpassade ord) i en enda fil (Hangman.db) direkt i programkatalogen.

Testning:

xUnit: Testramverket som används för enhetstester.

TDD (Test Driven Development): Game.cs är utvecklad med TDD, vilket bevisas av den omfattande testfilen GameTests.cs som täcker alla regler och edge-cases.

API-Kommunikation:

HttpClient och System.Net.Http.Json används i ApiWordProvider.cs för att asynkront hämta slumpmässiga ord från ett externt webb-API.

🧩 Avancerade C#-koncept som används
Här är en tabell som bryter ner några av de mer avancerade koncepten och var de används i projektet:

Område,Exempel i Koden,Förklaring
Asynkron Programmering,"async Task RunAsync(), await _wordProvider.GetWordAsync()","Hela applikationsflödet, ordhämtning och timers hanteras asynkront. I WPF (GameViewModel) säkerställer detta att UI:t aldrig ""fryser"". I Konsol (GameController) används Task.Run och CancellationTokenSource för att hantera speltimern parallellt med användarinmatning."
Data Binding (MVVM),"INotifyPropertyChanged, RelayCommand","I WPF ärver alla ViewModels från BaseViewModel för att meddela UI:t om ändringar. ICommand (RelayCommand) hanterar knapptryckningar, vilket helt separerar logik från XAML-vyn."
Events & Delegates,Game.GameEnded += OnGameEnded,"Kärnlogiken (Game.cs) använder traditionella C#-events för att meddela sin ""ägare"" (en ViewModel eller Controller) om att speltillståndet har ändrats (t.ex. att spelet är vunnet)."
Strategy Pattern,"IUiStrings, IAsyncWordProvider","Används för att ""injicera"" beteenden. MainViewModel kan starta ett spel med vilken som helst IAsyncWordProvider (API, DB, Lokal) utan att veta implementationen. LocalizationProvider använder samma mönster för att byta språk."
LINQ,context.Highscores.OrderBy(...).Take(N),"Används flitigt för att fråga och transformera datamängder, särskilt i SqliteHangmanService för att hämta och filtrera topplistor från databasen."
Avancerade Collections,"HashSet<char>, ObservableCollection<T>",HashSet används i Game.cs för O(1)-prestanda vid gissningskontroll. ObservableCollection används i HighscoreViewModel för att automatiskt uppdatera WPF-gränssnitten när listan ändras.
Anpassad Felhantering,NoCustomWordsFoundException,"Projektet definierar egna undantag. När en ordlista är tom kastas ett specifikt undantag som fångas i UI-lagret (GameViewModel/GameController) och översätts till ett användarvänligt, lokaliserat meddelande."

