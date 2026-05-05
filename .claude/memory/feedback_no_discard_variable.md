---
name: No discard variable name
description: Never use _ as a variable/parameter name
type: feedback
---

Never use `_` as a variable or parameter name (e.g., discard parameters in lambdas like `(_, m) =>`).

**Why:** User preference — explicit names are required.

**How to apply:** Use a descriptive name even for unused parameters. For messenger/delegate callbacks, use `recipient` instead of `_`.
