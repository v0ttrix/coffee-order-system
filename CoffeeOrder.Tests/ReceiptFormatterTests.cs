// ------------------------------------------------------------
// ReceiptFormatterTests.cs — quick overview
// Purpose: prove the text is deterministic and complete:
//   - includes author + timestamp
//   - shows line items with original/discount/final
//   - shows allergen warnings for almond
//   - shows discount summary lines and correct totals
// I pass a fixed timestamp so the test stays stable.
// ------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeOrder.Models;
using CoffeeOrder.Receipt;
using System;

namespace CoffeeOrder.Tests
{
    [TestClass]
    public class ReceiptFormatterTests
    {
        [TestMethod]
        public void Receipt_IncludesAuthor_AndTimestamp_AndLineItems()
        {
            //Arrange
            var a = new Beverage("Latte", "Tall", "Hot", null, "Oat", 2, new[] { "V" }, Array.Empty<string>(), false);      //5.40
            var b = new Beverage("Tea", "Grande", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);        //3.50
            var when = new DateTime(2025, 09, 26, 12, 00, 00, DateTimeKind.Utc);    //fixed time for stable test
            var promos = new[] { "HAPPYHOUR" };     //HH on both hot drinks

            //Act
            var txt = ReceiptFormatter.Format(new[] { a, b }, promos, "Jane Student", when);

            //Assert
            StringAssert.Contains(txt, "Author: Jane Student");
            StringAssert.Contains(txt, "Created: 2025-09-26 12:00 UTC");

            //After HH:
            //a 5.40 -> 20% off 1.08 => final 4.32
            //b 3.50 -> 20% off 0.70 => final 2.80
            StringAssert.Contains(txt, "Original: $5.40 | Discounts: $1.08 | Final: $4.32");
            StringAssert.Contains(txt, "Original: $3.50 | Discounts: $0.70 | Final: $2.80");

            //Totals:
            //subtotal = 8.90, discount = 1.78, total due = 7.12
            StringAssert.Contains(txt, "Subtotal (before promos): $8.90");
            StringAssert.Contains(txt, "Total Discounts:           $1.78");
            StringAssert.Contains(txt, "Total Due:                 $7.12");

            //Discount summary line should be present
            StringAssert.Contains(txt, "HAPPYHOUR: 20% off Hot drinks: $1.78");
        }

        [TestMethod]
        public void Receipt_IncludesAlmondAllergenWarning()
        {
            //Arrange
            var almond = new Beverage("Latte", "Grande", "Hot", null, "Almond", 1, Array.Empty<string>(), Array.Empty<string>(), false);
            var when = new DateTime(2025, 09, 26, 13, 45, 00, DateTimeKind.Utc);

            //Act
            var txt = ReceiptFormatter.Format(new[] { almond }, Array.Empty<string>(), "Jane Student", when);

            //Assert
            StringAssert.Contains(txt, "! Allergen: contains tree nuts (almond).");
        }

        [TestMethod]
        public void Receipt_BogoOnce_UsesPostHappyHourPrices_AndTotalsMatch()
        {
            //Arrange
            //Use the same pair as earlier to reuse known math:
            //a: 5.40 -> HH 1.08 => 4.32
            //b: 3.50 -> HH 0.70 => 2.80
            //BOGO then frees the cheaper of the top two: 2.80
            var a = new Beverage("Latte", "Tall", "Hot", null, "Oat", 2, new[] { "V" }, Array.Empty<string>(), false);
            var b = new Beverage("Tea", "Grande", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true);
            var when = new DateTime(2025, 09, 26, 14, 00, 00, DateTimeKind.Utc);
            var promos = new[] { "HAPPYHOUR", "BOGO" };

            //Act
            var txt = ReceiptFormatter.Format(new[] { a, b }, promos, "Jane Student", when);

            //Assert
            //total discount = 1.08 + 0.70 + 2.80 = 4.58
            //total due = (5.40 + 3.50) - 4.58 = 4.32
            StringAssert.Contains(txt, "BOGO: free item (once per order): $2.80");
            StringAssert.Contains(txt, "Total Discounts:           $4.58");
            StringAssert.Contains(txt, "Total Due:                 $4.32");
        }

        [TestMethod]
        public void Receipt_NoPromos_IsJustSubtotal()
        {
            //Arrange
            var x = new Beverage("Tea", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true); // $3.00
            var y = new Beverage("Tea", "Tall", "Hot", null, null, 0, Array.Empty<string>(), Array.Empty<string>(), true); // $3.00
            var when = new DateTime(2025, 09, 26, 15, 00, 00, DateTimeKind.Utc);

            //Act
            var txt = ReceiptFormatter.Format(new[] { x, y }, Array.Empty<string>(), "Jane Student", when);

            //Assert
            StringAssert.Contains(txt, "Subtotal (before promos): $6.00");
            StringAssert.Contains(txt, "Total Discounts:           $0.00");
            StringAssert.Contains(txt, "Total Due:                 $6.00");
        }
    }
}