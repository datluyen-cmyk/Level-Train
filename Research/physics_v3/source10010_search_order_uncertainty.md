# Source 10010 Search Ordering Uncertainty

The bounded solver's `unknown` result for source 10010 is a search outcome, not an unsolvability proof. `CompareSearchNodes` orders states first by removal progress and active binding count, then by prioritized/guided status, unblocked status, and finally ascending total pose travel. The frontier is capped at 128 states. In addition, search can stop enumerating a parent's siblings after generating a prioritized unblocked child.

This ordering can favor small guided, unblocked pose changes over a different legal setup path that requires more travel. Therefore the prior 15-second and 45-second source 10010 `unknown` reports do not establish that slider 2/ring 3 endpoint setup is unreachable or that no solution exists. The alternating slider 2/ring 3 diagnostic is a replayable eight-action trace, but it produces no removals and does not test the full endpoint/setup sequence.

Treat search bias as an uncertainty when interpreting source 10010. Keep the production movement rules unchanged. If the targeted legal diagnostic does not solve the level, an endpoint/setup-diversity ordering may be evaluated as a separate Editor-only search portfolio with its own policy identifier and report; do not substitute it for the default policy without regression evidence.
