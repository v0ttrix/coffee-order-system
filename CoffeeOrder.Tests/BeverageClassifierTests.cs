// ------------------------------------------------------------
// BeverageClassifierTests.cs — quick overview

// goal: AAA MSTest checks for the labelling rules. I keep each
// test small and obvious so failures read like a story.

// what I cover: caffeinated/decaf, dairy-free, vegan-friendly,
// kid-safe with/without "ExtraHot".
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;
using CoffeeOrder.Classification;
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class BeverageClassifierTests
    {
        [TestMethod]
        public void Classify_Shots2_NotDecaf_IsCaffeinatedTrue()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Grande",
                temp: "Hot",
                milk: "2%",
                plantMilk: null,
                shots: 2,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsTrue(labels.IsCaffeinated, "2 shots + not decaf should be caffeinated.");
            Assert.IsFalse(labels.IsDecaf, "Not decaf should be reported as such.");
        }

        [TestMethod]
        public void Classify_IsDecafTrue_Shots2_IsDecafTrue_CaffeinatedFalse()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Tall",
                temp: "Hot",
                milk: null,
                plantMilk: "Oat",
                shots: 2,           //even with shots, explicit decaf means I dont call it caffeinated
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsTrue(labels.IsDecaf, "Explicit decaf flag should be honored.");
            Assert.IsFalse(labels.IsCaffeinated, "Decaf overrides shot count for this label.");
        }

        [TestMethod]
        public void Classify_NoDairyPlantMilkOnly_IsDairyFreeAndVeganFriendly()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Tea",
                size: "Tall",
                temp: "Hot",
                milk: null,            //no dairy
                plantMilk: "Oat",      //plant milk is fine
                shots: 0,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsTrue(labels.IsDairyFree, "No dairy milk means dairy-free.");
            Assert.IsTrue(labels.IsVeganFriendly, "For our scope, dairy-free is vegan-friendly.");
        }

        [TestMethod]
        public void Classify_WithDairy_IsNotDairyFreeOrVeganFriendly()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Chocolate",
                size: "Grande",
                temp: "Iced",
                milk: "Whole",         //dairy present
                plantMilk: null,
                shots: 0,
                syrups: new[] { "Caramel" },
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsFalse(labels.IsDairyFree, "Dairy milk should break dairy-free.");
            Assert.IsFalse(labels.IsVeganFriendly, "And therefore not vegan-friendly here.");
        }

        [TestMethod]
        public void Classify_ZeroShots_Hot_KidSafeTrue()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Chocolate",
                size: "Tall",
                temp: "Hot",           //not extra hot
                milk: null,
                plantMilk: null,
                shots: 0,              //no espresso
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsTrue(labels.IsKidSafe, "No shots + not extra hot => kid-safe.");
        }

        [TestMethod]
        public void Classify_ExtraHot_NotDecaf_WithShots_KidSafeFalse()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Grande",
                temp: "ExtraHot",      //this alone should block kid-safe
                milk: null,
                plantMilk: "Oat",
                shots: 1,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var labels = BeverageClassifier.Classify(bev);

            //Assert
            Assert.IsFalse(labels.IsKidSafe, "ExtraHot should not be kid-safe.");
        }
    }
}