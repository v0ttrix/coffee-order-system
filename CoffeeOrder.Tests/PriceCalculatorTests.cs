// ------------------------------------------------------------
// PriceCalculatorTests.cs — quick overview
//
// goal: make sure size base + add-ons math is correct and
// uses decimals/rounding we expect. No promos in these tests.
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;
using CoffeeOrder.Pricing;
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class PriceCalculatorTests
    {
        [TestMethod]
        public void Calculate_TallLatte_2Shots_1Syrup_OatMilk_Subtotal540()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "Tall",      //base 3.00
                temp: "Hot",
                milk: null,
                plantMilk: "Oat",  //+0.60
                shots: 2,          //+1.50
                syrups: new[] { "Vanilla" }, //+0.30
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var bd = PriceCalculator.CalculateBeverageSubtotal(bev);

            //Assert
            Assert.AreEqual(3.00m, bd.BasePrice);
            Assert.AreEqual(1.50m, bd.Shots);
            Assert.AreEqual(0.30m, bd.Syrups);
            Assert.AreEqual(0.60m, bd.PlantMilk);
            Assert.AreEqual(0.00m, bd.Toppings);
            Assert.AreEqual(5.40m, bd.Subtotal);        //3.00 + 1.50 + 0.30 + 0.60
        }

        [TestMethod]
        public void Calculate_GrandeTea_NoAddons_Subtotal350()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Tea",
                size: "Grande",   //base 3.50
                temp: "Hot",
                milk: null,
                plantMilk: null,
                shots: 0,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //Act
            var bd = PriceCalculator.CalculateBeverageSubtotal(bev);

            //Assert
            Assert.AreEqual(3.50m, bd.Subtotal);
        }

        [TestMethod]
        public void Calculate_VentiChocolate_MaxShots4_MaxSyrups5_3Toppings_Dairy_Subtotal925()
        {
            //Arrange
            //base 4.00 + shots 4*0.75=3.00 + syrups 5*0.30=1.50 + toppings 3*0.25=0.75
            var bev = new Beverage(
                baseDrink: "Chocolate",
                size: "Venti",
                temp: "Iced",
                milk: "2%",            //dairy => no plant surcharge
                plantMilk: null,
                shots: 4,
                syrups: new[] { "S1", "S2", "S3", "S4", "S5" },
                toppings: new[] { "T1", "T2", "T3" },
                isDecaf: false
            );

            //Act
            var bd = PriceCalculator.CalculateBeverageSubtotal(bev);

            //Assert
            Assert.AreEqual(4.00m, bd.BasePrice);
            Assert.AreEqual(3.00m, bd.Shots);
            Assert.AreEqual(1.50m, bd.Syrups);
            Assert.AreEqual(0.00m, bd.PlantMilk);
            Assert.AreEqual(0.75m, bd.Toppings);
            Assert.AreEqual(9.25m, bd.Subtotal);
        }

        [TestMethod]
        public void CalculateOrderSubtotal_SumsAllBeverages()
        {
            //Arrange
            var a = new Beverage("Latte", "Tall", "Hot", null, "Oat", 2, new[] { "V" }, Array.Empty<string>(), false);      //5.40 (from earlier test)
            var b = new Beverage("Tea", "Grande", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);        //3.50

            //Act
            var total = PriceCalculator.CalculateOrderSubtotal(new[] { a, b });

            //Assert
            Assert.AreEqual(8.90m, total);          //5.40 + 3.50
        }

        [TestMethod]
        public void Calculate_UnknownSize_DefaultsToTallBase()
        {
            //Arrange
            var bev = new Beverage(
                baseDrink: "Latte",
                size: "MegaMega",     //unknown => treat like Tall
                temp: "Hot",
                milk: null,
                plantMilk: null,
                shots: 0,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: false
            );

            //Act
            var bd = PriceCalculator.CalculateBeverageSubtotal(bev);

            //Assert
            Assert.AreEqual(3.00m, bd.BasePrice);
            Assert.AreEqual(3.00m, bd.Subtotal);
        }
    }
}