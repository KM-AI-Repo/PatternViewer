# PROJECT_CONTEXT

## Role of this file

This file is an internal instruction set for AI-assisted development of this repository.
It is not user-facing documentation.

Its purpose is to:
- preserve architectural decisions;
- reduce repeated debates;
- keep future changes aligned with the real direction of the project;
- help an AI assistant continue development without re-discovering already settled decisions.

## Project identity

- Project name: `PatternViewer`
- Repository use: personal project
- Primary user: repository owner only
- UI technology: WinForms
- Runtime: .NET Framework 4.7.2
- Language: C#

This is not a public product and not a reusable library.
Practical reliability for a single owner is more important than public-facing polish.

## Tech stack

- C#
- WinForms
- .NET Framework 4.7.2
- Newtonsoft.Json
- WebSocketSharp
- System.Windows.Forms.DataVisualization.Charting

## Functional goal

The application is a desktop viewer for Binance USDT-margined perpetual futures.

The current direction is:
- preload market candle data for the selected interval;
- keep a local in-memory market cache;
- receive real-time updates for all tracked contracts;
- show a filtered subset of symbols instead of the full active universe;
- always keep forced symbols visible;
- allow chart viewing and real-time updates for the selected symbol;
- identify futures whose recent price path is sufficiently different from BTCUSDT and ETHUSDT;
- allow temporary validation of the comparison algorithm by switching to a “show similar” mode;
- prepare the codebase for future screening and pattern detection.

Current implemented behavior:
- load active USDT perpetual futures from Binance;
- preload candle history for the selected interval;
- keep candle cache in memory by symbol;
- receive real-time market-wide kline updates into the cache;
- detect broken market-cache continuity per symbol and recover that symbol through targeted REST resync;
- update the visible symbols list item-by-item instead of full rebinding;
- preserve manual symbol selection during real-time market updates;
- suppress internal selection events during programmatic restoration of the selected symbol;
- prevent redundant selected-chart reloads when the chosen symbol did not actually change;
- allow single-symbol selection;
- allow interval selection;
- persist the selected interval in `Properties.Settings`;
- persist comparison filter parameters in `Properties.Settings`;
- load the selected chart through REST;
- update the selected chart through a dedicated WebSocket;
- resync the selected chart if candle continuity is broken;
- expose live comparison-filter parameters on the form;
- expose a temporary mode switch for displaying similar instead of dissimilar symbols;
- re-screen symbols from cache when comparison-filter parameters are changed without reloading market data through REST.

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
- load persisted comparison settings into controls;
- apply current filter parameters to `MarketCacheService`;
- trigger market loading;
- build and maintain the visible symbols list;
- keep the currently selected candle list in memory;
- draw and update the chart;
- coordinate REST and WebSocket services;
- trigger selected-symbol resync when sequence integrity is violated;
- suppress event side effects during internal UI state restoration;
- reapply symbol screening when comparison settings are changed.

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
- compare non-forced symbols against BTCUSDT and ETHUSDT by normalized recent close-path distance;
- store and apply `comparisonWindow`, `distanceThreshold`, and temporary comparison direction mode;
- update cached candles in real time;
- request targeted symbol resync when continuity is broken;
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
- `BinanceCombinedStreamMessage`
- `MarketCandleUpdateResult`

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
- default comparison window
- default distance threshold

Important current distinction:
- comparison filter values `DefaultComparisonWindow` and `DefaultDistanceThreshold` are now persisted in `Properties.Settings`;
- the temporary `show similar` validation mode is still controlled by the form checkbox and is not persisted in settings;
- Binance interval options remain hardcoded immutable values.

## Current runtime flow

### Application startup state

On form load, the application:
- initializes chart styling;
- initializes the supported interval list;
- loads persisted comparison settings into controls;
- applies current comparison settings from the form controls to `MarketCacheService`;
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
7. populate `listBoxSymbols`;
8. restore previous/default selection if possible under suppression of selection events;
9. call `ReloadChartAndStreamAsync()` intentionally once after selection is set.

### Selected chart flow

For the currently selected symbol:
1. disconnect current selected-symbol WebSocket;
2. load candles through REST;
3. replace local selected-symbol candle list;
4. redraw chart;
5. connect a fresh WebSocket stream for the selected symbol;
6. remember the loaded symbol in `currentChartSymbol`.

### Market-wide real-time flow

For all tracked symbols:
1. `MarketWebSocketService` opens a combined stream connection;
2. incoming combined-stream payload is deserialized;
3. symbol is extracted from stream name;
4. candle is converted to `BinanceCandle`;
5. `MarketCacheService.UpdateCandle(...)` attempts to update in-memory cache;
6. if continuity is valid, cache is updated normally;
7. if continuity is broken, the app triggers targeted REST resync only for that symbol;
8. UI visibility is updated per symbol through add/remove operations instead of full list rebinding;
9. visibility is decided by `MarketCacheService.ShouldDisplaySymbol(symbol)` using the current comparison filter and current comparison mode.

### Comparison settings change flow

When the user changes comparison controls on the form:
1. form reads `numericComparisonWindow`, `numericDistanceThreshold`, and `checkBoxShowSimilar`;
2. form persists `numericComparisonWindow` and `numericDistanceThreshold` into `Properties.Settings`;
3. form passes the current values into `marketCacheService.SetSimilarityFilterSettings(...)`;
4. form rebuilds the visible symbols list from current in-memory cache;
5. selection is preserved under suppression;
6. selected chart is not reloaded only because filter settings changed;
7. no REST reload of market candles is performed.

### Stop button flow

When the user presses `Stop`:
- market-wide WebSocket is disconnected;
- selected-symbol WebSocket is disconnected;
- app leaves running state;
- status becomes `"Остановлено"`.

## Current market display policy

The app does not show the full active market by default.

The visible symbol list is produced by `MarketCacheService.GetDisplaySymbols()`:
- forced symbols are always included;
- additional symbols may be included by filter logic;
- symbols without cached candles are excluded;
- currently the filter is driven by normalized close-path distance to BTCUSDT and ETHUSDT;
- the final inclusion rule may be inverted temporarily by the “show similar” mode.

The visible list is then maintained in the UI incrementally:
- `AddSymbolToListBox(...)` inserts a symbol into the sorted list;
- `RemoveSymbolFromListBox(...)` removes a symbol if it is not currently selected;
- `RestoreSelectedSymbol(...)` keeps the prior selection without triggering chart reload.

## Current comparison filter

The current active filter is path-distance based and replaces the old green-last-candle display rule.

### Goal

The main goal is to show symbols that are moving sufficiently independently from the two market leaders:
- `BTCUSDT`
- `ETHUSDT`

There is also a temporary validation mode that shows symbols whose graphs are sufficiently similar to the leaders instead.

### Current filter rule

For every non-forced symbol:
1. take the last `comparisonWindow` candles of that symbol;
2. take the last `comparisonWindow` candles of `BTCUSDT`;
3. take the last `comparisonWindow` candles of `ETHUSDT`;
4. build normalized close paths using the first close in the window as the base;
5. calculate average absolute distance between the normalized symbol path and the normalized BTC path;
6. calculate average absolute distance between the normalized symbol path and the normalized ETH path;
7. keep the smaller of the two distances;
8. apply the final inclusion rule depending on current mode.

### Current normalization rule

Normalized close path is built as:

`(Close_i / Close_0) - 1`

This means:
- the first point of every window is `0`;
- comparison is done by shape of movement rather than by absolute price level;
- instruments with very different price scales remain comparable.

### Current distance metric

Current comparison uses average absolute point-by-point distance across the normalized path.

This is intentionally simple and is the baseline implementation.
It should remain the default unless a better algorithm is explicitly requested.

### Current filter parameters

Current runtime parameters:
- `comparisonWindow`
- `distanceThreshold`
- `showSimilarSymbols`

Current persisted defaults:
- `DefaultComparisonWindow = 50`
- `DefaultDistanceThreshold = 0.1`

Current runtime default for comparison mode:
- `showSimilarSymbols = false`

These values are tuning defaults, not fixed product truths.

### Current parameter UI

The form currently exposes:
- `numericComparisonWindow`
- `numericDistanceThreshold`
- `checkBoxShowSimilar`

These controls are intended for live experimentation and algorithm validation.

Changing `numericComparisonWindow` or `numericDistanceThreshold` should:
- persist the new value in `Properties.Settings`;
- re-screen the visible symbols from cache;
- preserve selection;
- avoid unnecessary selected-chart reload;
- avoid market REST reload.

Changing `checkBoxShowSimilar` should:
- switch only the final inclusion rule of the current algorithm;
- re-screen the visible symbols from cache;
- preserve selection;
- avoid unnecessary selected-chart reload;
- avoid market REST reload.

### Current inclusion logic

Current inclusion logic is:

- when `showSimilarSymbols == false`:
  - display symbol only if `minDistance > distanceThreshold`

- when `showSimilarSymbols == true`:
  - display symbol only if `minDistance <= distanceThreshold`

This temporary inversion mode exists for practical validation of the current comparison algorithm.
It should not be treated as a different algorithm.

## Forced symbol policy

The following symbols must always be shown:
- `BTCUSDT`
- `ETHUSDT`

These are product rules, not hacks.
They must remain visible even when screening logic is expanded later.

## Binance interval policy

Supported Binance kline intervals are a fixed API contract and are intentionally hardcoded.

They must not be moved into `Properties.Settings`.

If needed, keep them in:
- a dedicated immutable helper;
- a static readonly collection;
- constants intended for immutable API values.

## Interval persistence policy

The selected interval is user state.

When the user changes the interval:
- it becomes the new default interval in `Properties.Settings`;
- it should persist between launches.

This is intentional.

## Comparison settings persistence policy

The comparison window and distance threshold are user state.

When the user changes them:
- they become the new default values in `Properties.Settings`;
- they should persist between launches.

This is intentional.

At the moment, the temporary `show similar` validation mode is not part of persisted settings.

## Market bootstrap policy

The active futures universe is dynamic.
Do not assume the Binance symbol set is static.

Market bootstrap currently does:
1. load active Binance USDT perpetual symbols;
2. load recent candle history for each symbol for the selected interval;
3. build an in-memory candle cache keyed by symbol.

The preload strategy uses asynchronous parallel loading with `Task.WhenAll(...)`.
This is intentional and should remain the default unless there is a concrete rate-limit or reliability reason to redesign it.

## Selected chart loading policy

The display list comes from `MarketCacheService`.

The currently selected chart is still loaded independently through `BinanceRestService` and updated through `BinanceWebSocketService`.

This separation is intentional:
- `MarketCacheService` supports market bootstrap and screening;
- selected-symbol chart loading uses a fresh authoritative source;
- chart correctness is more important than avoiding reloads;
- redundant reloads caused by internal selection restoration are not acceptable and must be suppressed;
- changing screen/filter settings must not be treated as a reason to reload the selected chart.

## Real-time policy

### Current implemented real-time behavior

Right now, real-time updates exist in two layers:

1. **Selected-symbol real-time**
- one dedicated WebSocket connection is used for the selected chart;
- incoming kline updates update only the selected chart state;
- if continuity is suspicious, the selected symbol is resynced through REST.

2. **Market-wide real-time**
- one combined WebSocket connection is used for tracked symbols;
- incoming kline updates update only the market cache;
- these updates may also change whether a symbol should be visible in the UI;
- visibility updates should be applied incrementally, not through full rebinding.

### Important current UX rules

The symbol list may change in real time, but the selected symbol must remain stable.

Programmatic selection restoration must not be treated as a user action.

Specifically:
- `suppressSymbolSelectionChanged` is required when selection is restored internally;
- `ListBoxSymbols_SelectedIndexChanged(...)` must immediately return while suppression is active;
- the handler must also ignore cases where the selected symbol equals `currentChartSymbol`;
- market-driven list updates must not trigger redundant chart reloads;
- market-driven list updates must not cause button flicker or unstable selection;
- filter-parameter-driven list rebuilds must not trigger redundant chart reloads.

### Legacy rebinding rule

`RefreshSymbolsListPreserveSelection()` exists only as a legacy full-rebuild approach.
It is intentionally not used in live symbol updates because it previously caused unstable selection and `STOP` button flicker.

Do not reintroduce high-frequency full rebinding without a more stable strategy.

At the same time, a controlled list rebuild from cache is currently acceptable when comparison parameters change, provided that:
- selection is suppressed during restoration;
- the rebuild does not trigger `ReloadChartAndStreamAsync()`;
- the rebuild does not cause REST reloading of market data.

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

### 3. `BinanceCombinedStreamMessage` is required for market-wide streams

Combined Binance streams wrap payloads in:
- `stream`
- `data`

Therefore market-wide stream deserialization must use `BinanceCombinedStreamMessage` instead of trying to deserialize directly into a plain kline message.

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
- `incoming.OpenTimeMs == last.CloseTimeMs + 1`
- `last.IsClosed == true`

If either condition fails, market-cache continuity is considered broken for that symbol.

In that case, `UpdateCandle(...)` returns `MarketCandleUpdateResult.ResyncRequired`.

The recovery strategy is:
- detect the broken sequence for one symbol;
- trigger targeted REST resync only for that symbol;
- reload fresh candles through `BinanceRestService`;
- replace cached candles through `ReplaceSymbolCandles(...)`.

### 7. Selected chart correctness is more important than avoiding reloads

For the currently selected symbol, it is acceptable to reload candles through REST when continuity is suspicious.

Correctness has higher priority than minimizing reload count.

At the same time, false reloads caused by UI side effects must be prevented.

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

### 10. Programmatic selection must not trigger selected-chart reload

When the app restores or preserves the selected symbol internally:
- it must use `suppressSymbolSelectionChanged`;
- it must not call `ReloadChartAndStreamAsync()` indirectly through `SelectedIndexChanged`;
- only an actual user-driven symbol change should reload the selected chart.

This rule exists to prevent:
- REST overuse;
- redundant WebSocket reconnects;
- chart flicker;
- disappearing or unstable latest candles.

### 11. Candle coloring in WinForms chart

For the candlestick chart, `PriceUpColor` and `PriceDownColor` alone are not treated as sufficient for correct visual coloring of all candle parts in the current implementation.

The chart currently relies on per-point coloring:
- green when `Close >= Open`
- red when `Close < Open`

Do not remove point-level candle coloring unless the wick/tail coloring problem is solved and visually verified.

### 12. Comparison filter uses normalized close-path distance

The current market screening logic for non-forced symbols is based on:
- normalized close-path construction from recent candles;
- comparison against `BTCUSDT` and `ETHUSDT`;
- average absolute distance across the normalized path.

This is the current baseline algorithm and must not be silently replaced by another comparison method without explicit discussion.

### 13. `candlesBySymbol` is the canonical market cache store

Inside `MarketCacheService`, the canonical in-memory storage is:

- `candlesBySymbol`

Do not introduce parallel duplicate symbol-to-candles stores unless there is a strong reason.
New screening logic should use `candlesBySymbol` as the source of truth.

### 14. `showSimilarSymbols` only changes final inclusion direction

The temporary similar/dissimilar switch does not change:
- normalization;
- reference symbols;
- distance calculation;
- comparison window.

It only changes the final inclusion rule:
- dissimilar mode uses `minDistance > distanceThreshold`;
- similar mode uses `minDistance <= distanceThreshold`.

## Architectural rules

1. Do not mix constants and settings.
2. Do not move mutable values into constants.
3. Do not move immutable API contract values into user settings.
4. Keep UI logic in `Form1` and network logic in services.
5. Prefer incremental refactoring over large rewrites.
6. Preserve the current project structure unless a change is explicitly requested.
7. Prefer practical reliability over abstract architectural purity.
8. Do not introduce event-driven UI side effects that cause redundant REST reloads.
9. Prefer incremental list maintenance over high-frequency full list rebinding.
10. Prefer screening based on current in-memory market cache over extra REST requests when only filter parameters change.

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
- The selected chart may reload through REST when WebSocket continuity is questionable.
- UI stability has higher priority than aggressive real-time list refresh.
- Market-driven symbol list changes must preserve selection and must not trigger redundant chart reload.
- Point-level candle coloring is currently an accepted implementation detail.
- The current first screening algorithm is normalized close-path distance against BTCUSDT and ETHUSDT.
- `comparisonWindow` and `distanceThreshold` are now persisted in `Properties.Settings`.
- The `show similar` switch is currently a temporary validation tool and is not persisted in settings.

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

## Guidance for future refactoring

Refactoring is welcome only when it has direct practical value for this project.

Good reasons:
- improved reliability;
- easier screening / pattern extension;
- better separation of responsibilities;
- safer real-time updates;
- clearer settings handling;
- faster or cleaner market bootstrap;
- better comparability diagnostics for symbol-screening results.

Weak reasons:
- abstract clean architecture purity;
- unnecessary modernization;
- changes made only because they are fashionable.

## Expected next directions

Likely future work includes:
- tuning the current `comparisonWindow` and `distanceThreshold`;
- evaluating whether the temporary `show similar` mode should remain, be removed, or become a permanent diagnostic feature;
- exposing more readable diagnostics for why a symbol was included or excluded;
- optional display of distance-to-BTC / distance-to-ETH values;
- expanding screening logic beyond the current single distance rule;
- pattern detection logic;
- candle analysis helpers;
- expanding `MarketCacheService`;
- optional refinement of market-cache resync strategy (backoff, retry policy, diagnostics);
- careful improvement of dynamic visible-symbol refresh;
- chart behavior improvements;
- a settings form;
- additional services built on top of the existing baseline.

If symbol screening is extended further, it should:
- update cache first;
- prefer item-level add/remove over full rebinding in high-frequency realtime updates;
- preserve correct manual symbol switching;
- preserve stable `STOP` button behavior;
- preserve selected-chart continuity;
- avoid extra REST chart reloads;
- reuse the existing market cache instead of introducing unnecessary parallel data sources.

Future work should preserve the already working behavior:
- market bootstrap;
- market-wide cache updates;
- forced symbol inclusion;
- interval selection;
- interval persistence;
- comparison window persistence;
- distance threshold persistence;
- symbol display through `MarketCacheService`;
- selected-symbol REST loading;
- selected-symbol dedicated WebSocket real-time updates;
- candle continuity checks;
- selected-symbol resync behavior;
- suppression of false reloads caused by internal selection restoration;
- runtime adjustment of comparison filter settings without market REST reload.

## Preferred collaboration style

For future assistance, use this operating style:
- assume this file is authoritative unless explicitly overridden;
- ask fewer conceptual questions when the answer is already here;
- avoid repeating previously rejected suggestions;
- keep responses practical and implementation-focused;
- optimize for continuity with the current codebase;
- prefer small safe changes over large rewrites.