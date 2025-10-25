// ------------------------------------------------------------
// OrderValidator.cs  — quick overview
// goal: sanity-check one Beverage (or later a list).
//
// what it checks now: required fields, temp present, milk XOR,
// shots/syrups limits, and allergen warnings for tree-nut milks.
//
// the why: I keep validation free of pricing/classification so tests
// stay focused and fast.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeOrder.Models;          //the Beverage lives here
using CoffeeOrder.Validation;      //for ValidationResult

namespace CoffeeOrder.Validation
{
    public static class OrderValidator
    {
        public static ValidationResult Validate(Beverage beverage)
        {
            //hold hard "fails" (these make IsValid = false)
            var errors = new List<string>();

            //hold softer "heads-up" messages (these do NOT fail the drink)
            var warnings = new List<string>();

            //required fields:
            //base drink must be present
            if (string.IsNullOrWhiteSpace(beverage.BaseDrink))
                errors.Add("A base drink is required.");

            //size must be present
            if (string.IsNullOrWhiteSpace(beverage.Size))
                errors.Add("A size selection is required.");

            //temp should be chosen (Hot or Iced; your model enforces single choice)
            if (string.IsNullOrWhiteSpace(beverage.Temp))
                errors.Add("Temperature (Hot/Iced) must be selected.");

            // ----- milk selection rule (XOR: dairy or plant, not both) -----
            //if both dairy and plant are set -> invalid
            if (!string.IsNullOrWhiteSpace(beverage.Milk) && !string.IsNullOrWhiteSpace(beverage.PlantMilk))
                errors.Add("Milk selection invalid: choose dairy OR plant milk, not both.");

            // ----- limits (bounds checks) -----
            //shots must be 0..4 inclusive
            if (beverage.Shots < 0 || beverage.Shots > 4)
                errors.Add("Shots must be between 0 and 4 inclusive.");

            //syrups list: 0..5 entries, none of them null
            if (beverage.Syrups is null || beverage.Syrups.Length > 5 || beverage.Syrups.Any(s => s is null))
                errors.Add("Syrups must contain 0..5 non-null entries.");

            // ----- allergen warnings (tree nuts) -----
            //if plant milk is almond (case-insensitive), add a friendly warning.
            //this does NOT fail the order, it just informs the user.
            //you can extend this list later (hazelnut, cashew, etc.) if you want :)
            if (!string.IsNullOrWhiteSpace(beverage.PlantMilk)
                && beverage.PlantMilk.Trim().Equals("Almond", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add("Allergen: contains tree nuts (almond).");
            }

            // ----- wrap up -----
            //if I have hard errors -> fail (but still return any warnings I collected)
            if (errors.Count > 0)
                return ValidationResult.FailWithWarnings(errors, warnings);

            //otherwise success (but keep any warnings)
            return warnings.Count > 0
                ? ValidationResult.OkWithWarnings(warnings)
                : ValidationResult.Ok();
        }
    }
}