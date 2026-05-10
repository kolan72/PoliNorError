# CONVENTIONS

## API Design Conventions
- Fluent policy configuration methods (`With...`, `Add...`, `Include...`, `Exclude...`)
- Strong sync/async method pairing (`Handle` / `HandleAsync`)
- Generic and non-generic overload parity
- Cancellation behavior exposed via `CancellationType`

## Naming Patterns
- `Policy`, `Processor`, `...Extensions`, `...Result`, `...Info`
- Feature folders map cleanly to type prefixes (`Fallback*`, `Retry*`, `Simple*`)

## Error Handling Conventions
- Exceptions translated into structured `PolicyResult` states
- Catch-block failures are tracked and surfaced
- Filter-driven handling (include/exclude, inner-error support)

## Testing Conventions
- NUnit attributes with descriptive `Should_...` method names
- Feature-focused test files mirroring source modules
- Extensive use of helper/fixture classes for delegate behavior and errors

## Documentation Conventions
- Rich XML comments in public API surface
- Changelog and README maintained at repository root
