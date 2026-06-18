# PROJECT_CONTEXT

## Role of this file
This file is an internal instruction set for AI-assisted development of this repository.
It is not user-facing documentation.
It exists to preserve architectural decisions, prevent repeated debates, and keep future code changes aligned with the actual project direction.

## Project identity
- Project name: PatternViewer
- Repository visibility/use: personal project, not a public product
- Primary user: repository owner only
- Current UI technology: WinForms
- Target runtime: .NET Framework 4.7.2
- Language: C#

## Tech stack
- C#
- WinForms
- .NET Framework 4.7.2
- Newtonsoft.Json
- WebSocketSharp
- System.Windows.Forms.DataVisualization.Charting

## Functional goal
The application is a desktop viewer for Binance USDT-margined perpetual futures.

The project is moving toward filtered symbol display based on candle analysis and future pattern detection.
The long-term goal is to show the user only symbols that currently match selected market patterns, while still allowing chart viewing for the selected instrument.

Current implemented behavior:
- load active USDT perpetual futures from Binance
- preload market candle data for the selected interval
- keep a local in-memory candle cache by symbol
- always include forced symbols in the display list
- allow single-symbol selection
- allow interval selection while the program is stopped
- persist the last selected interval in user settings
- load the selected chart through REST
- update the selected chart through WebSocket
- resync the selected chart if candle continuity is broken
- require explicit Start before any market loading begins
- allow explicit Stop to terminate active runtime connections
- rate-limit repeated Start actions with a fixed cooldown

## Runtime model
The application now has explicit idle and running states.

### Idle state
When the form opens:
- no market data is loaded automatically
- no chart data is loaded automatically
- no WebSocket connection is opened automatically
- interval selection is allowed
- Start is available
- Stop is disabled

### Running state
After Start:
- market data is loaded
- display symbols are rebuilt from market cache
- selected symbol chart data is loaded through REST
- selected symbol WebSocket is connected
- interval selection is disabled
- Start is disabled
- Stop is enabled

### Busy state
During market loading, chart loading, or selected-symbol resync:
- both Start and Stop are disabled
- interval selection is disabled
- the user cannot interrupt loading

### Stop behavior
When Stop is pressed:
- active real-time selected-symbol WebSocket connection is disconnected
- the form returns to idle state
- market loading does not continue automatically
- interval selection becomes available again

## Current architecture
The current architecture is intentional and should be treated as the baseline.

### UI
Main UI:
- Form1.cs
- Form1.Designer.cs

Form responsibilities:
- initialize controls
- initialize supported intervals
- initialize chart styling
- maintain explicit idle/running UI state
- trigger market loading only from Start
- keep the current selected-symbol candle list in memory
- draw and update the chart
- coordinate REST and WebSocket services
- trigger selected-symbol resync when sequence integrity is violated

### Services
Current service layer is correct and should be extended incrementally.

#### BinanceRestService
Responsibilities:
- load exchangeInfo
- filter active USDT perpetual symbols
- load historical klines through REST

#### BinanceWebSocketService
Responsibilities:
- connect to Binance kline WebSocket streams for the currently selected symbol
- deserialize incoming kline updates
- convert payloads into BinanceCandle
- notify UI through events

#### MarketCacheService
Responsibilities:
- load the active market universe
- preload candle history for symbols for the selected interval
- keep candle cache in memory by symbol
- provide symbols eligible for display
- enforce forced-symbol inclusion
- serve as the base for future screening logic

### Models
Explicit Binance payload models are used, including:
- BinanceExchangeInfo
- BinanceSymbol
- BinanceCandle
- BinanceKlineStreamMessage
- BinanceKlineData

### Constants
Constants are for immutable technical values only.

Examples:
- USDT
- PERPETUAL
- TRADING
- chart area and series names
- forced symbols
- fixed Binance interval list
- Start cooldown

### Settings
Properties.Settings is used only for mutable values that may later be edited through a settings form.

Examples:
- REST base URL
- WebSocket base URL
- REST endpoints
- default symbol
- default interval
- default candle limit

## Architectural rules
1. Do not mix constants and settings.
2. Do not move mutable values into constants.
3. Do not move immutable API contract values into user settings.
4. Keep UI logic in Form1 and network logic in services.
5. Prefer incremental refactoring over large rewrites.
6. Preserve the current project structure unless a change is explicitly requested.
7. Prefer practical reliability over abstract architectural purity.

## Binance interval policy
The supported kline intervals are a fixed Binance API contract and are intentionally hardcoded.
They must not be moved into Properties.Settings.
If needed, keep them in a dedicated immutable helper or static readonly collection.

## Interval persistence policy
The selected interval is user state.
When the user changes the interval while the program is stopped, the new interval becomes the new default interval in Properties.Settings and persists between launches.

Changing the interval while stopped must not:
- trigger market loading
- trigger chart loading
- trigger chart redraw logic beyond normal initialization needs
- open or reopen network connections

The interval in idle state is preparation for the next Start, not a trigger for immediate work.

## Start/Stop policy
Program execution is controlled explicitly through Start and Stop buttons.

Rules:
- the application must not auto-start loading on form open
- Start is the only allowed entry point for full market loading
- Stop is the normal way to end runtime activity
- manual reload buttons for market bootstrap are intentionally removed
- Start actions are rate-limited to reduce accidental API overuse

## Start cooldown policy
Repeated Start actions must be limited by a fixed cooldown window.

Current rule:
- cooldown is an immutable technical value stored in constants
- the current implementation uses `BinanceConstants.StartCooldown`
- the guard is checked when Start is clicked
- if cooldown has not expired, the program does not start and instead shows the remaining wait time in the status label

This cooldown exists specifically to reduce the risk of repeatedly reloading historical data and exceeding API limits.

## Market universe policy
The active futures universe is dynamic.
Do not assume the Binance symbol set is static.

The application should:
- request the active market universe from Binance
- rebuild market cache for the selected interval
- treat display symbols as a filtered subset of the active universe

## Forced symbol policy
The following symbols must always be shown:
- BTCUSDT
- ETHUSDT

These symbols must survive future screening/filtering rules.

## Screening direction
The long-term direction is:
- do not show all active symbols by default
- preload candle history first
- evaluate symbols against filter logic
- show only symbols that pass the filter
- still include forced symbols even if they do not pass the filter

Current status:
- MarketCacheService already contains the screening entry point
- `MatchesFilter(...)` is currently a stub
- current visible list intentionally consists of forced symbols only, plus future filter matches when implemented

## Initial market loading policy
Market bootstrap currently loads:
1. active Binance USDT perpetual symbols
2. recent candle history for each symbol for the selected interval
3. an in-memory candle cache keyed by symbol

The preload strategy uses asynchronous parallel loading with `Task.WhenAll`.
This is intentional and should remain the default approach unless a concrete performance or rate-limit reason requires redesign.

## Selected chart loading policy
The display list comes from MarketCacheService.
The currently selected chart is still loaded directly through BinanceRestService when selected or resynced.

This separation is intentional:
- MarketCacheService supports market-wide screening/bootstrap
- BinanceRestService provides an authoritative fresh source for the currently selected chart

## Critical implementation facts

### 1. Kline identity must use open time
Binance kline identity must be based on candle open time.
Use `OpenTimeMs` as the candle identity key.

Do not use close time as the identity key.

### 2. BinanceKlineData must explicitly map both `t` and `T`
The Binance payload contains both:
- `t` = open time
- `T` = close time

These fields differ only by case and have different meaning.
They must both be explicitly mapped in the model.

### 3. BinanceCandle model must carry continuity fields
`BinanceCandle` must explicitly contain:
- OpenTimeMs
- CloseTimeMs
- IsClosed

REST and WebSocket conversion must both fill these fields consistently.

### 4. Candle continuity validation must use Binance boundaries
For selected-chart integrity, continuity must be validated using:
- previous candle close time
- next candle open time
- previous candle closed state

The intended rule is:
- `incoming.OpenTimeMs == lastCandle.CloseTimeMs + 1`
- `lastCandle.IsClosed == true`

If this rule fails, do not silently continue.
Trigger a REST resync for the selected symbol.

### 5. Selected chart correctness is more important than avoiding reloads
For the currently selected symbol, it is acceptable to reload candles through REST when continuity is suspicious.
Chart correctness has higher priority than reducing reload count.

### 6. .NET Framework 4.7.2 limitation
Do not use LINQ methods unavailable in .NET Framework 4.7.2, such as `TakeLast()`.
Use `Skip(...)` or explicit list trimming instead.

### 7. ListBox rebinding rule
After rebinding `ListBox.DataSource`, always reassign:
- DisplayMember
- ValueMember

Also keep `BinanceSymbol.ToString()` returning `Symbol` as a defensive fallback.

### 8. Selected-symbol real-time reload flow
When the selected symbol changes, or when selected-symbol resync is required:
1. disconnect current selected-symbol WebSocket
2. reload selected-symbol candles through REST
3. redraw chart
4. connect a new selected-symbol WebSocket stream

This behavior is intentional and should be preserved unless explicitly redesigned.

### 9. Chart initialization order matters
Chart initialization must happen before any code path that could trigger chart drawing from interval selection or startup initialization.

In the current UI lifecycle:
- `InitializeChart()` should happen before `InitializeIntervals()`

This prevents startup errors caused by interval selection events firing before chart setup is complete.

### 10. Interval change in idle state is non-operative
When the application is stopped, changing the interval should only persist the new default interval.
It should not initiate loading or runtime behavior.

## Decisions that are already settled
Do not reopen these without an explicit request.

- The project remains WinForms.
- The project remains on .NET Framework 4.7.2.
- The project is for personal use, not for public distribution.
- A public README is not required.
- The mismatch between repository name and project name is intentional.
- Binance intervals are intentionally hardcoded and are not user-editable settings.
- The current split into Models / Services / Constants / Settings / Form1 is correct.
- MarketCacheService is a valid part of the architecture and should not be treated as temporary clutter.
- Forced symbols are a product rule, not a hack.
- Showing only forced symbols at the current stage is acceptable while screening logic is still being built.
- Manual symbol reload button is intentionally removed.
- Start cooldown belongs to immutable technical constants, not user settings.

## Guidance for future suggestions
When proposing changes:
- work inside the current architecture first
- prefer minimal-disruption changes
- provide ready-to-paste code blocks
- specify which file each change belongs to
- explain what must be added, replaced, or removed
- avoid enterprise-style overengineering
- avoid framework migration suggestions unless explicitly requested
- avoid suggesting WPF, MAUI, Avalonia, web UI, or full rewrites unless explicitly requested
- treat this file as authoritative unless explicitly overridden

## Guidance for future refactoring
Refactoring is welcome only when it has direct practical value for this project.

Good reasons:
- improved reliability
- easier screening/pattern extension
- better separation of responsibilities
- safer real-time updates
- clearer settings handling
- faster or cleaner market bootstrap

Weak reasons:
- abstract “clean architecture” purity
- unnecessary modernization
- changes made only because they are fashionable

## Expected next directions
Likely future work includes:
- implementing real filter logic in `MatchesFilter(...)`
- real-time updates for market-wide latest candles
- pattern detection logic
- candle analysis helpers
- expanding market cache behavior
- a dedicated market-wide WebSocket service
- chart behavior improvements
- a settings form
- additional services built on top of the existing baseline

Future work should preserve the already working behavior:
- explicit Start/Stop runtime control
- market bootstrap
- forced symbol inclusion
- interval selection in idle state only
- interval persistence
- symbol display through MarketCacheService
- selected-symbol REST loading
- selected-symbol WebSocket real-time updates
- candle continuity checks
- selected-symbol resync behavior
- Start cooldown protection

## Preferred collaboration style
For future assistance, use this operating style:
- assume this file is authoritative unless the user overrides it
- ask fewer conceptual questions when the answer is already here
- avoid repeating previously rejected suggestions
- keep responses practical and implementation-focused
- optimize for continuity with the current codebase
- prefer small safe changes over large rewrites