# Development Reflection: Start, Stop, Continue

## Start
- Writing the test first for each slice. I started with validator tests for missing base or size, milk XOR, and limits, then added tests for almond warnings. After that I moved to classifier, pricing, promotions, and finally receipt. Having a failing test up front kept each class small and focused.
- Passing time as a parameter. ReceiptFormatter does not read the clock. The tests pass a fixed UTC timestamp, which makes the receipt assertions stable and keeps the function pure.
- Locking the promotion order early. I wrote tests that assume HAPPYHOUR happens first and BOGO uses the post-HAPPYHOUR prices. That decision removed ambiguity and kept the math straightforward in both code and tests.
- Centralizing numbers. I put base and add-on prices in one place (PricingConstants). Updates are low-friction and tests stay honest.

## Stop
- Letting responsibilities leak across classes. In past projects I mixed validation and pricing. Here I kept validation in OrderValidator, price math in PriceCalculator, and formatting in ReceiptFormatter. The tests got simpler because each piece had one job.
- Relying on presence checks only. I added two pending tests to push stricter validation next: allowed Temp values and rejecting whitespace syrup entries. Leaving them red is a reminder to finish that refinement.
- Using culture-dependent formatting blindly. I hit the generic currency symbol when I used InvariantCulture. I switched to an explicit en-US currency format so the receipt is predictable and tests do not flake on different machines.

## Continue
- AAA test structure and intent-revealing names. Keeping tests in Arrange, Act, Assert with blank lines and clear method names made failures easy to read and fix.
- Pure functions and decimal for money. Avoiding I/O in core logic keeps everything fast and isolated. Using decimal with rounding at the edges avoids floating-point drift and makes totals look like a real receipt.
- Small refactors after green. After getting tests to pass, I factored out helpers like Money, Round2, and promotion summary lines. The code reads better without changing behavior.

## What I learned
Pushing toward determinism helped a lot. No hidden time, fixed currency format, and consistent rounding made the tests reliable and let me focus on behavior, not setup. Committing to a simple promo stacking rule early saved me from tricky edge cases later. The two pending tests are healthy pressure to keep improving validation without bloating the validator all at once.