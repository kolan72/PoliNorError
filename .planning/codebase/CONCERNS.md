# CONCERNS

## Technical Concerns
- Dual project style (SDK source + legacy tests) complicates maintenance.
- High overload density increases risk of behavior drift between sync/async and generic/non-generic variants.
- Extension-heavy surface can hide complexity for contributors unfamiliar with internal processor flows.

## Reliability Concerns
- NuGet restore/TLS issues can impact test reliability in some environments.
- Catch-block and fallback edge cases are complex; regressions are possible without focused tests.

## Maintainability Concerns
- Large file count and broad API surface raise onboarding cost.
- Internal coupling between policy, processor, and extension layers needs careful refactoring discipline.

## Refactoring Opportunities
1. Gradual migration of tests to SDK-style project format.
2. Continued consolidation of duplicated overload logic through internal helper abstractions.
3. Introduce architecture docs/diagrams in `src/docs/diagrams` to reduce implicit knowledge.

## Immediate Practical Recommendations
1. Keep adding narrow regression tests for each behavior fix.
2. Validate NuGet restore on CI and local documented setup.
3. For new policy capabilities, prefer additive types (non-breaking), matching current evolution pattern.
