# PROJECT_CONTEXT

## Role of this file

This file is an internal instruction set for AI-assisted development of this repository.  
It is not user-facing documentation.

Its purpose is to:
- preserve architectural decisions;
- reduce repeated debates;
- keep future changes aligned with the real direction of the project;
- help an AI assistant continue development without re-discovering already settled decisions.

---

## Project identity

- Project name: `PatternViewer`
- Repository use: personal project
- Primary user: repository owner only
- UI technology: WinForms
- Runtime: .NET Framework 4.7.2
- Language: C#

This is not a public product and not a reusable library.  
Practical reliability for a single owner is more important than public-facing polish.

---

## Tech stack

- C#
- WinForms
- .NET Framework 4.7.2
- Newtonsoft.Json
- WebSocketSharp
- System.Windows.Forms.DataVisualization.Charting

---

## Functional goal

The application is a desktop viewer for Binance USDT-margined perpetual futures.

The current direction is:
- preload market candle data for the selected interval;
- keep a local in-memory market cache;
- receive real-time updates for all tracked contracts;
- show a filtered subset of symbols instead of the full active universe;
- always keep forced symbols visible;
- allow chart viewing and real-time updates for the selected symbol;
- prepare the codebase for future screening and pattern detection.

Current implemented behavior:
- load active USDT perpetual futures from Binance;
- preload candle history for the selected interval;
- keep candle cache in memory by symbol;
- receive real-time market-wide kline updates into the cache;
- detect broken market-cache continuity per symbol and recover that symbol through targeted REST resync;
- always show forced symbols even if no filter matches;
- allow single-symbol selection;
- allow interval selection;
- persist the selected interval in `Properties.Settings`;
- load the selected chart through REST;
- update the selected chart through a dedicated WebSocket;
- resync the selected chart if candle continuity is broken.

---

## Current architecture

The current architecture is intentional and is the baseline for future work.

### UI

Main UI files:
- `Form1.cs`
- `Form1.Designer.cs`

Form responsibilities:
- initialize controls;
- initialize supported intervals;
- initialize chart styling;
- trigger market loading;
- build the visible symbols list;
- keep the currently selected candle list in memory;
- draw and update the chart;
- coordinate REST and WebSocket services;
- trigger selected-symbol resync when sequence integrity is violated.

### Services

Current service layer is valid and should be extended incrementally.

#### `BinanceRestService`

Responsibilities:
- load `exchangeInfo`;
- filter active USDT perpetual symbols;
- load historical klines through REST.

#### `BinanceWebSocketService`

Responsibilities:
- connect to Binance kline WebSocket stream for the currently selected symbol;
- deserialize incoming kline updates;
- convert payloads into `BinanceCandle`;
- notify the UI through events.

#### `MarketCacheService`

Responsibilities:
- load the active market universe;
- preload candle history for all active symbols for the selected interval;
- keep candle cache in memory by symbol;
- provide symbols eligible for display;
- enforce forced-symbol inclusion;
- update cached candles in real time;
- serve as the base for future screening logic.

#### `MarketWebSocketService`

Responsibilities:
- connect to Binance combined kline streams for tracked symbols;
- receive real-time market-wide updates;
- deserialize combined stream payloads;
- convert payloads into `BinanceCandle`;
- notify the UI through events with `(symbol, candle)`.

### Models

Explicit Binance payload models are used, including:
- `BinanceExchangeInfo`
- `BinanceSymbol`
- `BinanceCandle`
- `BinanceKlineStreamMessage`
- `BinanceKlineData`
- `BinanceCombinedStreamMessage<T>`

### Constants

Constants are for immutable technical values only.

Examples:
- `USDT`
- `PERPETUAL`
- `TRADING`
- chart area / series names
- forced symbols
- fixed Binance interval list
- technical cooldown values

### Settings

`Properties.Settings` is used only for mutable values that may later be edited through a settings form.

Examples:
- REST base URL
- WebSocket base URL
- API endpoints
- default symbol
- default interval
- default candle limit

---

## Current runtime flow

### Application startup state

On form load, the application:
- initializes chart styling;
- initializes the supported interval list;
- creates an empty candle list;
- draws an empty chart;
- shows status `"Готово к запуску"`.

### Start button flow

When the user presses `Start`:
1. app checks cooldown;
2. app switches to running state;
3. app calls `LoadSymbolsAsync()`.

### `LoadSymbolsAsync()` flow

Current behavior:
1. read selected interval;
2. read candle limit from settings;
3. call `marketCacheService.InitializeAsync(interval, candleLimit)`;
4. get tracked symbols from `marketCacheService.GetTrackedSymbols()`;
5. start `MarketWebSocketService` for tracked symbols;
6. get display symbols from `marketCacheService.GetDisplaySymbols()`;
7. bind the result to `listBoxSymbols`;
8. restore previous/default selection if possible;
9. call `ReloadChartAndStreamAsync()`.

### Selected chart flow

For the currently selected symbol:
1. disconnect current selected-symbol WebSocket;
2. load candles through REST;
3. replace local selected-symbol candle list;
4. redraw chart;
5. connect a fresh WebSocket stream for the selected symbol.

### Market-wide real-time flow

For all tracked symbols:
1. `MarketWebSocketService` opens a combined stream connection;
2. incoming combined-stream payload is deserialized;
3. symbol is extracted from stream name;
4. candle is converted to `BinanceCandle`;
5. `MarketCacheService.UpdateCandle(...)` attempts to update in-memory cache;
6. if continuity is valid, cache is updated normally;
7. if continuity is broken, the app triggers targeted REST resync only for that symbol.

### Stop button flow

When the user presses `Stop`:
- market-wide WebSocket is disconnected;
- selected-symbol WebSocket is disconnected;
- app leaves running state;
- status becomes `"Остановлено"`.

---

## Current market display policy

The app does **not** show the full active market by default.

The visible symbol list is produced by `MarketCacheService.GetDisplaySymbols()`:
- forced symbols are always included;
- additional symbols may be included later by filter logic;
- symbols without cached candles are excluded;
- currently `MatchesFilter(...)` is still a stub.

At the current stage, the practical visible result is:
- `BTCUSDT`
- `ETHUSDT`

unless future filter logic is implemented.

---

## Forced symbol policy

The following symbols must always be shown:
- `BTCUSDT`
- `ETHUSDT`

These are product rules, not hacks.  
They must remain visible even when real screening logic is added later.

---

## Binance interval policy

Supported Binance kline intervals are a fixed API contract and are intentionally hardcoded.

They must **not** be moved into `Properties.Settings`.

If needed, keep them in:
- a dedicated immutable helper;
- a static readonly collection;
- constants intended for immutable API values.

---

## Interval persistence policy

The selected interval is user state.

When the user changes the interval:
- it becomes the new default interval in `Properties.Settings`;
- it should persist between launches.

This is intentional.

---

## Market bootstrap policy

The active futures universe is dynamic.  
Do not assume the Binance symbol set is static.

Market bootstrap currently does:
1. load active Binance USDT perpetual symbols;
2. load recent candle history for each symbol for the selected interval;
3. build an in-memory candle cache keyed by symbol.

The preload strategy uses asynchronous parallel loading with `Task.WhenAll(...)`.  
This is intentional and should remain the default unless there is a concrete rate-limit or reliability reason to redesign it.

---

## Selected chart loading policy

The display list comes from `MarketCacheService`.

The currently selected chart is still loaded independently through `BinanceRestService` and updated through `BinanceWebSocketService`.

This separation is intentional:
- `MarketCacheService` supports market bootstrap and future screening;
- selected-symbol chart loading uses a fresh authoritative source;
- chart correctness is more important than avoiding reloads.

---

## Real-time policy

### Current implemented real-time behavior

Right now, real-time updates exist in **two layers**:

1. **Selected-symbol real-time**
- one dedicated WebSocket connection is used for the selected chart;
- incoming kline updates update only the selected chart state;
- if continuity is suspicious, the selected symbol is resynced through REST.

2. **Market-wide real-time**
- one combined WebSocket connection is used for tracked symbols;
- incoming kline updates update only the market cache;
- this cache is intended for future screening / filtering logic.

### Important current UX rule

Although market-wide real-time cache updates are active, the visible symbols list is **not** currently rebound on every market tick.

This is intentional.

A previous attempt to refresh `listBoxSymbols` in real time caused two concrete problems:
- selecting another symbol became unreliable;
- the `STOP` button flickered between enabled and disabled states too often to be usable.

Therefore, at the current stage:
- market-wide updates may update cache;
- market-wide updates must **not** trigger high-frequency `listBoxSymbols` rebinding;
- market-wide updates must **not** frequently toggle general UI state.

### Current temporary rule

`RefreshSymbolsListPreserveSelection()` exists, but its real-time invocation is intentionally disabled for now.

Do not re-enable per-tick UI rebinding without a more stable strategy, such as:
- throttling;
- periodic timer-driven refresh;
- refresh only when the display set actually changes.

---

## Critical implementation facts

### 1. Kline identity must use open time

Binance kline identity must be based on candle open time.

Use:
- `OpenTimeMs`

Do not use close time as the candle identity key.

### 2. `BinanceKlineData` must explicitly map both `t` and `T`

The Binance payload contains:
- `t` = open time
- `T` = close time

These fields differ only by case and have different meaning.  
They must both be explicitly mapped in the model.

### 3. `BinanceCombinedStreamMessage<T>` is required for market-wide streams

Combined Binance streams wrap payloads in:
- `stream`
- `data`

Therefore market-wide stream deserialization must use `BinanceCombinedStreamMessage<T>` instead of trying to deserialize directly into a plain kline message.

### 4. `BinanceCandle` must carry continuity fields

`BinanceCandle` must explicitly contain:
- `OpenTimeMs`
- `CloseTimeMs`
- `IsClosed`

REST and WebSocket conversion must fill these fields consistently.

### 5. Candle continuity validation must use Binance boundaries

For selected-chart integrity, continuity must be validated using:
- previous candle close time;
- next candle open time;
- previous candle closed state.

Intended rule:
- `incoming.OpenTimeMs == lastCandle.CloseTimeMs + 1`
- `lastCandle.IsClosed == true`

If this rule fails, do not silently continue.  
Trigger a REST resync for the selected symbol.

### 6. Market cache continuity recovery is targeted per symbol

`MarketCacheService.UpdateCandle(...)` updates an existing candle if `OpenTimeMs` matches.

If a new candle is appended, it must satisfy both conditions:
- `incoming.OpenTimeMs == lastCandle.CloseTimeMs + 1`
- `lastCandle.IsClosed == true`

If either condition fails, market-cache continuity is considered broken for that symbol.

In that case, `UpdateCandle(...)` must not silently accept the data and must not silently ignore the problem.  
Instead, it returns `MarketCandleUpdateResult.ResyncRequired`.

The recovery strategy is:
- detect the broken sequence for one symbol;
- trigger targeted REST resync only for that symbol;
- reload fresh candles through `BinanceRestService`;
- replace cached candles through `ReplaceSymbolCandles(...)`.

This is intentional.

The goal is:
- keep market-wide real-time processing lightweight;
- recover only the damaged symbol cache;
- avoid global market reload when only one symbol becomes inconsistent.

### 7. Selected chart correctness is more important than avoiding reloads

For the currently selected symbol, it is acceptable to reload candles through REST when continuity is suspicious.

Correctness has higher priority than minimizing reload count.

### 8. .NET Framework 4.7.2 limitation

Do not use LINQ methods unavailable in .NET Framework 4.7.2, such as:
- `TakeLast()`

Use:
- `Skip(...)`
- explicit list trimming
- simple compatible alternatives

### 9. `ListBox` rebinding rule

After rebinding `ListBox.DataSource`, always reassign:
- `DisplayMember`
- `ValueMember`

Also keep `BinanceSymbol.ToString()` returning `Symbol` as a defensive fallback.

### 10. Selected chart reload flow is intentional

When symbol changes:
1. disconnect current selected-symbol WebSocket;
2. load selected-symbol candles through REST;
3. redraw chart;
4. connect a new selected-symbol WebSocket stream.

This behavior is intentional and should be preserved unless explicitly redesigned.

---

## Architectural rules

1. Do not mix constants and settings.
2. Do not move mutable values into constants.
3. Do not move immutable API contract values into user settings.
4. Keep UI logic in `Form1` and network logic in services.
5. Prefer incremental refactoring over large rewrites.
6. Preserve the current project structure unless a change is explicitly requested.
7. Prefer practical reliability over abstract architectural purity.
8. Do not introduce high-frequency UI rebinding as a side effect of real-time processing.

---

## Decisions that are already settled

Do not reopen these without an explicit request.

- The project remains WinForms.
- The project remains on .NET Framework 4.7.2.
- The project is for personal use, not for public distribution.
- A public `README` is not required.
- The mismatch between repository name and project name is intentional.
- Binance intervals are intentionally hardcoded and are not user-editable settings.
- The current split into `Models / Services / Constants / Settings / Form1` is correct.
- `MarketCacheService` is a valid part of the architecture and not temporary clutter.
- `MarketWebSocketService` is a valid part of the architecture.
- Forced symbols are a product rule, not a hack.
- Showing only forced symbols at the current stage is acceptable while screening logic is still being built.
- The selected chart may reload through REST when WebSocket continuity is questionable.
- UI stability has higher priority than aggressive real-time list refresh.
- Real-time market cache updates are allowed even when UI list refresh is temporarily disabled.

---

## Guidance for future suggestions

When proposing changes:
- work inside the current architecture first;
- prefer minimal-disruption changes;
- provide ready-to-paste code blocks;
- specify which file each change belongs to;
- explain what must be added, replaced, or removed;
- avoid enterprise-style overengineering;
- avoid framework migration suggestions unless explicitly requested;
- avoid suggesting WPF, MAUI, Avalonia, Web UI, or full rewrites unless explicitly requested;
- treat this file as authoritative unless explicitly overridden.

---

## Guidance for future refactoring

Refactoring is welcome only when it has direct practical value for this project.

Good reasons:
- improved reliability;
- easier screening / pattern extension;
- better separation of responsibilities;
- safer real-time updates;
- clearer settings handling;
- faster or cleaner market bootstrap.

Weak reasons:
- abstract clean architecture purity;
- unnecessary modernization;
- changes made only because they are fashionable.

---

## Expected next directions

Likely future work includes:
- implementing real filter logic in `MatchesFilter(...)`;
- pattern detection logic;
- candle analysis helpers;
- expanding `MarketCacheService`;
- optional refinement of market-cache resync strategy (backoff, retry policy, diagnostics);
- careful reintroduction of dynamic visible-symbol refresh;
- chart behavior improvements;
- a settings form;
- additional services built on top of the existing baseline.

If dynamic symbol refresh is added later, it should:
- update cache first;
- avoid rebinding `ListBox` on every tick;
- avoid frequent `UpdateControlsState()` calls from market events;
- preserve correct manual symbol switching;
- preserve stable `STOP` button behavior.

Future work should preserve the already working behavior:
- market bootstrap;
- market-wide cache updates;
- forced symbol inclusion;
- interval selection;
- interval persistence;
- symbol display through `MarketCacheService`;
- selected-symbol REST loading;
- selected-symbol dedicated WebSocket real-time updates;
- candle continuity checks;
- selected-symbol resync behavior.

---

## Preferred collaboration style

For future assistance, use this operating style:
- assume this file is authoritative unless explicitly overridden;
- ask fewer conceptual questions when the answer is already here;
- avoid repeating previously rejected suggestions;
- keep responses practical and implementation-focused;
- optimize for continuity with the current codebase;
- prefer small safe changes over large rewrites.