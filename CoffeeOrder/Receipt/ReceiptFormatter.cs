// ------------------------------------------------------------
// ReceiptFormatter.cs — quick overview
//
// goal: build a plain-text receipt that is deterministic,
// easy to test, and has all the details we care about.
//
// what it prints (in order):
//   - Title, Author, Created timestamp (you pass timestamp in)
//   - Line items with per-item original, discounts, final
//   - Any allergen warnings under each item (e.g., almond -> tree nuts)
//   - Summary discount lines (HAPPYHOUR / BOGO) from PromotionHelper
//   - Subtotal, Total Discount, Total Due
//
// rules:
//   - No I/O here (no reading clock, no console print).
//   - All money is decimal, rounded to 2dp.
//   - Date/time is passed by you so tests are stable.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;     // for currency formatting
using System.Linq;              // for Sum/Select
using CoffeeOrder.Models;       // Beverage model
using CoffeeOrder.Pricing;      // PriceCalculator
using CoffeeOrder.Promotions;   // PromotionHelper result types
using CoffeeOrder.Validation;   // OrderValidator for warnings

namespace CoffeeOrder.Receipt
{
    public static class ReceiptFormatter
    {
        //tiny helper: force $ and 2 decimals in a stable way (always en-US)
        //why: god knows why InvariantCulture prints the generic ¤ sign (I can't find what it means); tests expect $ haha
        private static readonly CultureInfo UsCulture =
            CultureInfo.GetCultureInfo("en-US");

        private static string Money(decimal v)
            => v.ToString("C2", UsCulture);     //prints like $1,234.56

        //main entry: give us the beverages, the promo codes, your name, and a creation time (UTC recommended)
        public static string Format(
            IEnumerable<Beverage> beverages,      //the drinks I am printing
            IEnumerable<string> promoCodes,       //like ["HAPPYHOUR","BOGO"]
            string authorName,                    //your name goes on the header
            DateTime createdAtUtc                 //you pass this in so tests don’t rely on “now”
        )
        {
            //normalize inputs so I dont null-ref on callers
            var items = beverages?.ToList() ?? new List<Beverage>();
            var codes = promoCodes?.ToList() ?? new List<string>();

            //price each drink (before promos) so I can show per-item "Original"
            var priceInfos = items.Select(b => PriceCalculator.CalculateBeverageSubtotal(b)).ToList();

            //collect warnings (like almond -> tree nuts) using the validator
            //note: warnings don’t make the item invalid, they are a heads-up for the user
            var warnLists = items.Select(b => OrderValidator.Validate(b).Warnings).ToList();

            //apply promos (HAPPYHOUR first, then BOGO), deterministic math
            var promoResult = PromotionHelper.Apply(items, codes);

            //compute an overall “plain subtotal” (sum of original amounts)
            var subtotalBeforePromos = priceInfos.Sum(p => p.Subtotal);

            //now I actually build the lines of the receipt in order
            var lines = new List<string>();

            // ----- header -----
            lines.Add("=== Coffee Shop Receipt ===");                            //simple title
            lines.Add($"Author: {authorName}");                                  //you asked to include your name
            lines.Add($"Created: {createdAtUtc:yyyy-MM-dd HH:mm} UTC");          //fixed format, test-friendly
            lines.Add("");                                                       //blank spacer

            // ----- items -----
            lines.Add("Items:");
            for (int i = 0; i < items.Count; i++)
            {
                var b = items[i];                       //the beverage itself
                var original = priceInfos[i].Subtotal;  //pre-promo price
                var itemTotals = promoResult.Items[i];  //post-promo piece (discount+final)

                //one line per item with key details and per-item money summary
                lines.Add($"{i + 1}) {b.BaseDrink} {b.Size} {b.Temp} - " +
                          $"Original: {Money(original)} | Discounts: {Money(itemTotals.Discount)} | Final: {Money(itemTotals.Final)}");

                //print any warnings (like allergens) indented for readability
                foreach (var w in warnLists[i])
                {
                    lines.Add($"   ! {w}");
                }
            }

            lines.Add("");      //spacer

            // ----- discount lines (summary) -----
            if (promoResult.Lines.Count > 0)
            {
                lines.Add("Discounts:");
                foreach (var dl in promoResult.Lines)
                {
                    lines.Add($" - {dl.Reason}: {Money(dl.Amount)}");
                }
                lines.Add("");      //spacer after discounts section
            }

            // ----- totals -----
            lines.Add($"Subtotal (before promos): {Money(subtotalBeforePromos)}");
            lines.Add($"Total Discounts:           {Money(promoResult.TotalDiscount)}");
            lines.Add($"Total Due:                 {Money(promoResult.FinalOrderTotal)}");

            //join with newlines for a clean, deterministic blob
            return string.Join(Environment.NewLine, lines);
        }
    }
}