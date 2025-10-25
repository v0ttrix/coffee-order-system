// ------------------------------------------------------------
// PromotionHelperTests.cs — quick overview
// Purpose: make sure HAPPYHOUR and BOGO behave exactly as specced.
// Stacking rule: HAPPYHOUR first (per-item), then BOGO once using
// the discounted prices to pick which item is free.
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;
using CoffeeOrder.Promotions;
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class PromotionHelperTests
    {
        [TestMethod]
        public void HappyHour_AppliesOnlyToHot_20Percent()
        {
            //Arrange
            var hot = new Beverage("Latte", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), false);     //3.00
            var iced = new Beverage("Tea", "Tall", "Iced", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);      //3.00
            var promos = new[] { "HAPPYHOUR" };

            //Act
            var res = PromotionHelper.Apply(new[] { hot, iced }, promos);

            //Assert
            //HH: 20% off only the hot one => 0.60 total discount
            Assert.AreEqual(0.60m, res.TotalDiscount);
            Assert.AreEqual(5.40m, res.FinalOrderTotal);        //(3.00 - 0.60) + 3.00 = 5.40
        }

        [TestMethod]
        public void Bogo_TwoItems_FreeCheaperOfTopTwo_AfterHappyHour()
        {
            //Arrange
            //a hot Tall latte with 2 shots + 1 syrup + oat milk = 5.40 (from earlier price tests)
            var a = new Beverage("Latte", "Tall", "Hot", null, "Oat", 2, new[] { "V" }, Array.Empty<string>(), false);
            //a hot Grande tea w/o addons = 3.50
            var b = new Beverage("Tea", "Grande", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);

            //apply both promos: HH first, then BOGO once
            var promos = new[] { "HAPPYHOUR", "BOGO" };

            //act
            var res = PromotionHelper.Apply(new[] { a, b }, promos);

            //Assert
            //Step 1: HH on both (they're Hot):
            //  a: 5.40 * 20% = 1.08  => 4.32
            //  b: 3.50 * 20% = 0.70  => 2.80
            //Step 2: BOGO across post-HH prices: top two are 4.32 and 2.80
            //  free cheaper => 2.80 off
            //Total discount = 1.08 + 0.70 + 2.80 = 4.58
            //Final total = (5.40 + 3.50) - 4.58 = 4.32
            Assert.AreEqual(4.58m, res.TotalDiscount);
            Assert.AreEqual(4.32m, res.FinalOrderTotal);
        }

        [TestMethod]
        public void Bogo_OnlyOnce_EvenWithThreeItems()
        {
            //Arrange
            var x = new Beverage("Latte", "Venti", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), false);      //4.00 base
            var y = new Beverage("Tea", "Grande", "Iced", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);       //3.50 base
            var z = new Beverage("Chocolate", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), false);   //3.00 base

            //just BOGO (no HH) -> choose top two (4.00 & 3.50), free cheaper = 3.50
            var promos = new[] { "BOGO" };

            //Act
            var res = PromotionHelper.Apply(new[] { x, y, z }, promos);

            //Assert
            //total before: 4.00 + 3.50 + 3.00 = 10.50
            //BOGO once => -3.50 (y free)
            //final: 7.00
            Assert.AreEqual(3.50m, res.TotalDiscount);
            Assert.AreEqual(7.00m, res.FinalOrderTotal);
        }

        [TestMethod]
        public void NoPromos_JustPassThroughTotals()
        {
            //Arrange
            var a = new Beverage("Latte", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), false);   //3.00
            var b = new Beverage("Tea", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);      //3.00

            //Act
            var res = PromotionHelper.Apply(new[] { a, b }, Array.Empty<string>());

            //Assert
            Assert.AreEqual(0.00m, res.TotalDiscount);
            Assert.AreEqual(6.00m, res.FinalOrderTotal);
        }

        [TestMethod]
        public void HappyHour_NotHot_NoDiscount()
        {
            //Arrange
            var iced = new Beverage("Latte", "Tall", "Iced", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), false);       //3.00

            //Act
            var res = PromotionHelper.Apply(new[] { iced }, new[] { "HAPPYHOUR" });

            //Assert
            Assert.AreEqual(0.00m, res.TotalDiscount);
            Assert.AreEqual(3.00m, res.FinalOrderTotal);
        }
    }
}