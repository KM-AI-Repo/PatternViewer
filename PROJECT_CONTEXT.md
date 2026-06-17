# PROJECT_CONTEXT

## Role of this file
This file is an internal instruction set for AI-assisted development of this repository.
It is not user-facing documentation.
It exists to preserve architectural decisions, prevent repeated debates, and keep future code changes aligned with the current project direction.

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
Current features:
- load active USDT perpetual futures symbols from Binance
- allow single-symbol selection
- allow kline interval selection
- load the latest 99 candles through REST
- display a candlestick chart on the main form
- update the chart in real time through WebSocket
- use Properties.Settings for mutable configuration values

## Current architecture
The current architecture is intentional and should be treated as the baseline.

### UI
Main UI:
- Form1.cs
- Form1.Designer.cs

Form responsibilities:
- initialize controls
- load symbols on startup
- react to symbol/interval changes
- keep current candle collection in memory
- draw/update the chart
- coordinate REST and WebSocket services

### Services
- BinanceRestService: exchangeInfo loading, symbol filtering, historical klines loading
- BinanceWebSocketService: WebSocket connection, real-time kline updates, event-based delivery to UI

### Models
The project uses explicit Binance payload models, including:
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
- chart series names
- chart area names

### Settings
Properties.Settings is used only for mutable values that may later be edited by the owner through a settings form.
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
6. Preserve the existing project structure unless a change is explicitly requested.

## Binance interval policy
The supported kline intervals are a fixed Binance API contract and are intentionally hardcoded.
They should not be moved into Properties.Settings.
If cleanup is needed, they may be stored in a static readonly collection or a dedicated immutable helper, but not in mutable user settings.

## Critical implementation facts
### 1. Kline identity must use open time
Binance WebSocket kline payload contains:
- `t` = open time
- `T` = close time

The application must use `t` as the candle identity key.
Matching candles by close time is incorrect and breaks real-time updates.

### 2. BinanceKlineData must explicitly map both `t` and `T`
Because the JSON contains both properties with different casing and different meaning, the model must explicitly declare both.
Do not rely on inferred or ambiguous mapping.

### 3. .NET Framework 4.7.2 limitation
Do not use LINQ methods unavailable in .NET Framework 4.7.2, such as `TakeLast()`.
Use compatible alternatives like `Skip(...)` or explicit list trimming.

### 4. ListBox rebinding rule
After rebinding `ListBox.DataSource`, reassign:
- `DisplayMember`
- `ValueMember`

Otherwise the control may display the model type name instead of the symbol text.
Also keep `BinanceSymbol.ToString()` returning `Symbol` as a defensive fallback.

### 5. Real-time update flow
When symbol or interval changes:
1. disconnect current WebSocket
2. reload historical candles through REST
3. redraw chart
4. connect a new WebSocket stream

This behavior is intentional and should be preserved unless explicitly redesigned.

## Decisions that are already settled
Do not re-argue these unless the user explicitly reopens the decision.

- The project remains WinForms.
- The project remains on .NET Framework 4.7.2.
- The project is for personal use, not for public distribution.
- A public README is not required.
- The current mismatch between repository name and project name is intentional and should not be flagged as a problem.
- Binance intervals are intentionally hardcoded and should not be suggested as user-editable settings.
- The current architecture split into Models / Services / Constants / Settings / Form1 is correct.

## Guidance for future suggestions
When proposing changes:
- work within the current architecture first
- provide minimal-disruption solutions
- prefer ready-to-paste code blocks
- specify which file each change belongs to
- explain what must be replaced, removed, or added
- avoid enterprise-style overengineering
- avoid framework migration suggestions unless explicitly requested
- avoid suggesting WPF, MAUI, Avalonia, web UI, or a full rewrite unless explicitly requested

## Guidance for future refactoring
Refactoring is welcome only when it has direct practical value for this project.
Good reasons:
- improved reliability
- easier extension of pattern-detection logic
- better separation of responsibilities
- clearer settings handling
- safer real-time updates

Weak reasons:
- abstract “clean architecture” purity
- unnecessary modernization
- changes made only because they are fashionable

## Expected next directions
Likely future work includes:
- pattern detection logic
- candle analysis utilities
- chart behavior improvements
- settings form
- additional services built on top of the existing baseline

Future work should preserve the already working behavior:
- symbol loading
- interval selection
- chart rendering
- REST historical loading
- WebSocket real-time updates

## Preferred collaboration style
For future assistance, use this operating style:
- assume this file is authoritative unless the user overrides it
- ask fewer conceptual questions when the answer is already here
- avoid repeating previously rejected suggestions
- keep responses practical and implementation-focused
- optimize for continuity with the current codebase
