// ------------------------------------------------------------
// OrderValidatorTests.cs  — quick overview
// goal: MSTest unit tests for the validator, using AAA style.
// I are adding warning cases: almond triggers tree-nut warning,
// oat does not. also check that warnings dont flip validity.
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;       //Beverage
using CoffeeOrder.Validation;   //OrderValidator, ValidationResult
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class OrderValidatorTests
    {
            [TestMethod]
    public void Validate_MissingBase_ReturnsInvalid()
    {
        var bev = new Beverage(
            baseDrink: null,
            size: "Tall",
            temp: "Hot",
            milk: "2%",
            plantMilk: null,
            shots: 1,
            syrups: new[] { "Vanilla" },
            toppings: Array.Empty<string>(),
            isDecaf: false
        );

        var result = OrderValidator.Validate(bev);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(string.Join("|", result.Errors), "base drink");
    }

    [TestMethod]
    public void Validate_DairyAndPlantMilkTogether_ReturnsInvalid()
    {
        var bev = new Beverage(
            baseDrink: "Latte",
            size: "Grande",
            temp: "Hot",
            milk: "2%",
            plantMilk: "Oat",
            shots: 1,
            syrups: Array.Empty<string>(),
            toppings: Array.Empty<string>(),
            isDecaf: false
        );

        var result = OrderValidator.Validate(bev);

        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(string.Join("|", result.Errors), "Milk selection invalid");
    }

    [TestMethod]
    public void Validate_TypicalLatte_ReturnsValid()
    {
        var bev = new Beverage(
            baseDrink: "Latte",
            size: "Tall",
            temp: "Hot",
            milk: null,
            plantMilk: "Oat",
            shots: 2,
            syrups: new[] { "Vanilla" },
            toppings: Array.Empty<string>(),
            isDecaf: false
        );

        var result = OrderValidator.Validate(bev);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

        [TestMethod]
        public void Validate_AlmondPlantMilk_AddsTreeNutWarning_AndIsStillValid()
        {
            //Arrange
            //a normal latte with almond milk (valid choices overall)
            var bev = new Beverage(
                baseDrink: "Latte",             //legit base drink
                size: "Grande",                 //legit size
                temp: "Hot",                    //pick a single temp
                milk: null,                     //no dairy
                plantMilk: "Almond",            //almond triggers tree-nut warning
                shots: 1,                       //within 0..4
                syrups: Array.Empty<string>(),  //0..5 ok
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var result = OrderValidator.Validate(bev);

            //Assert
            Assert.IsTrue(result.IsValid, "Almond milk should not invalidate the drink.");
            StringAssert.Contains(
                string.Join("|", result.Warnings),
                "tree nuts",
                "We expect a tree nut warning when almond milk is used."
            );
        }

        [TestMethod]
        public void Validate_OatPlantMilk_NoTreeNutWarning()
        {
            //Arrange
            //oat milk is not a tree nut -> no warning expected
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Tall",
                temp: "Hot",
                milk: null,
                plantMilk: "Oat",          //NOT a tree nut
                shots: 2,
                syrups: new[] { "Vanilla" },
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var result = OrderValidator.Validate(bev);

            //Assert
            Assert.IsTrue(result.IsValid, "Oat milk should be valid.");
            Assert.AreEqual(0, result.Warnings.Count, "No tree-nut warning for oat milk.");
        }

        [TestMethod]
        public void Validate_DairyAndAlmondTogether_IsInvalid_AndWarnsTreeNuts()
        {
            //Arrange
            //user picked both dairy and plant (almond). this is invalid AND should still warn.
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Grande",
                temp: "Hot",
                milk: "2%",               //dairy selected
                plantMilk: "Almond",      //also plant milk selected -> invalid
                shots: 1,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var result = OrderValidator.Validate(bev);

            //Assert
            Assert.IsFalse(result.IsValid, "Dairy + plant milk should be invalid.");
            StringAssert.Contains(
                string.Join("|", result.Errors),
                "Milk selection invalid",
                "We should get the XOR milk rule error."
            );
            StringAssert.Contains(
                string.Join("|", result.Warnings),
                "tree nuts",
                "Even when invalid, we still want to surface allergen warnings."
            );
        }
    }
}