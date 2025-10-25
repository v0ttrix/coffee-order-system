// ------------------------------------------------------------
// PendingValidatorTests.cs — quick overview
//
// goal: two "red" tests to document the next TDD targets.
// These intentionally fail today and are tagged [TestCategory("Pending")]
// so we can exclude them in normal runs. They keep us honest about
// finishing stricter validation rules later.

// targets:
//  1) temp must be one of: Hot / Iced / ExtraHot (if modeled)
//  2) syrups must contain non-empty (not just non-null) values
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;
using CoffeeOrder.Validation;
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class PendingValidatorTests
    {
        [TestMethod]
        [TestCategory("Pending")]
        public void Validate_TempNotInAllowedSet_ReturnsInvalid()
        {
            //Arrange
            //"Lukewarm" is not an allowed value, I want the validator to bark.
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Tall",
                temp: "Lukewarm",       //<-- invalid domain value (should be Hot/Iced/ExtraHot)
                milk: null,
                plantMilk: "Oat",
                shots: 1,
                syrups: new[] { "Vanilla" },
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var result = OrderValidator.Validate(bev);

            //Assert
            //today this will (incorrectly) be valid because I only check "present".
            //future behavior: invalid + an error mentioning Hot/Iced (allowed domain hint).
            Assert.IsFalse(result.IsValid, "Temp outside allowed values should be invalid.");
            StringAssert.Contains(
                string.Join("|", result.Errors),
                "Hot/Iced",
                "Error message should hint at the allowed temperature options."
            );
        }

        [TestMethod]
        [TestCategory("Pending")]
        public void Validate_SyrupsContainsEmptyOrWhitespace_ReturnsInvalid()
        {
            //Arrange
            //I want to reject empty/whitespace syrup entries, not just nulls.
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Grande",
                temp: "Hot",
                milk: null,
                plantMilk: null,
                shots: 0,
                syrups: new[] { "Vanilla", "  " },   // <-- whitespace-only entry
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //Act
            var result = OrderValidator.Validate(bev);

            //Assert
            //today this will (incorrectly) be valid because I only check for nulls.
            //future behavior: invalid + a clean message.
            Assert.IsFalse(result.IsValid, "Whitespace syrup entries should be invalid.");
            StringAssert.Contains(
                string.Join("|", result.Errors),
                "Syrups",
                "Expect a friendly error about non-empty syrup entries."
            );
        }
    }
}