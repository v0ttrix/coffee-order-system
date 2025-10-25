# Coffee Shop Order Builder

C#/.NET 8 solution with a small coffee domain and a solid unit test suite grown with TDD (red → green → refactor). Everything is deterministic, fast, and free of external I/O.

## Projects

- **CoffeeOrder** (production)
  - Models: `Beverage`
  - Validation: `OrderValidator`, `ValidationResult`
  - Classification: `BeverageClassifier`
  - Pricing: `PriceCalculator`, `PricingConstants`
  - Promotions: `PromotionHelper`
  - Receipt: `ReceiptFormatter`
  - App glue: `AppDriver` (no UI; thin helpers only)

- **CoffeeOrder.Tests** (MSTest)
  - `OrderValidatorTests`
  - `BeverageClassifierTests`
  - `PriceCalculatorTests`
  - `PromotionHelperTests`
  - `ReceiptFormatterTests`
  - `PendingValidatorTests` (**2 intentionally red tests** for TDD-in-progress)

## Requirements and conventions

- **Language and framework:** C# on .NET 8 with MSTest.
- **AAA pattern:** All tests use Arrange, Act, Assert with blank lines between sections.
- **Naming:** Tests have intent-revealing names like `Validate_MissingBase_ReturnsInvalid`.
- **Separation of concerns:**
  - Validation is in `OrderValidator` (no pricing or promos inside).
  - Pricing is pure and currency-safe (`decimal` everywhere; rounding to 2 dp).
  - Promotions are deterministic: `HAPPYHOUR` (20% off **Hot** only) applied first per-item, then `BOGO` once per order using post-HAPPYHOUR prices.
  - Receipt formatting is deterministic text and includes author name and creation timestamp (provided as a parameter).
- **Allergen note:** Almond plant milk adds a tree nuts warning (warning, not an error).
- **KidSafe:** no espresso shots or explicitly decaf, and not `ExtraHot` (if present).

## Build and test

From the repo root:

```bash
dotnet build
dotnet test CoffeeOrder.Tests
```

You will see 2 failing tests by design. These are intentionally red tests to show work in progress.

To run green day to day and exclude the pending tests:

```bash
dotnet test CoffeeOrder.Tests --filter "TestCategory!=Pending"
```

You can also run from inside the test project:

```bash
cd CoffeeOrder.Tests
dotnet test
```

## Folder structure

```text
CoffeeOrder/
  App/AppDriver.cs
  Classification/BeverageClassifier.cs
  Models/Beverage.cs
  Pricing/PriceCalculator.cs
  Pricing/PricingConstants.cs
  Promotions/PromotionHelper.cs
  Receipt/ReceiptFormatter.cs
  Validation/OrderValidator.cs
  Validation/ValidationResult.cs

CoffeeOrder.Tests/
  BeverageClassifierTests.cs
  OrderValidatorTests.cs
  PendingValidatorTests.cs   # [TestCategory("Pending")] - intentionally red
  PriceCalculatorTests.cs
  PromotionHelperTests.cs
  ReceiptFormatterTests.cs
```

## Quick demo (optional, no I/O)

```bash
dotnet run --project .\CoffeeOrder.Demo\CoffeeOrder.Demo.csproj
```

## Features

- CoffeeOrder and CoffeeOrder.Tests included
- 25+ tests total with AAA naming and style
- 2 red tests present and tagged Pending (TDD in progress)
- README.md and Reflection.md documentation
- Builds and tests run in a clean environment
- No external I/O; deterministic output
