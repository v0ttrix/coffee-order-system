// ------------------------------------------------------------
// PromotionHelper.cs — quick overview
//
// goal: apply named promos to a list of beverages without
// touching core pricing logic. I lean on PriceCalculator for
// base subtotals, then compute discounts in a deterministic way.
//
// included promos:
//  - "HAPPYHOUR": 20% off Hot drinks only (case-insensitive)
//  - "BOGO": once per order, cheapest of the two most expensive
//            discounted items becomes free (== 100% off)
//
// stacking rule (predictable):
//   1) apply HAPPYHOUR per-beverage first
//   2) apply BOGO once, based on the post-HAPPYHOUR prices
//
// Notes:
// - all math is decimal. I round each discount to 2 dp using
//   MidpointRounding.AwayFromZero so it looks like a receipt.
// - I return a result object that lists per-item adjusted totals,
//   total discount, and friendly lines for tests/receipt.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeOrder.Models;
using CoffeeOrder.Pricing;

namespace CoffeeOrder.Promotions
{
    //one line I can print on a receipt or assert in tests
    public sealed class DiscountLine
    {
        public string Reason { get; init; } = string.Empty;     //quick “why I discounted”
        public decimal Amount { get; init; }                    //how much I took off (2 dp)
    }

    //per-item totals after promos (useful for asserts)
    public sealed class PromotionItemTotal
    {
        public decimal Original { get; init; }      //original price from PriceCalculator
        public decimal Discount { get; init; }      //item-level discount sum

        //final = original - discount (clamped to 2 dp in a receipt-friendly way)
        public decimal Final => Round2(Original - Discount);

        private static decimal Round2(decimal v) =>
            decimal.Round(v, 2, MidpointRounding.AwayFromZero);
    }

    //overall result
    public sealed class PromotionResult
    {
        public IReadOnlyList<PromotionItemTotal> Items { get; init; } = Array.Empty<PromotionItemTotal>();
        public decimal TotalDiscount { get; init; }        //sum of all discounts
        public decimal FinalOrderTotal { get; init; }      //sum of all item finals
        public IReadOnlyList<DiscountLine> Lines { get; init; } = Array.Empty<DiscountLine>();
    }

    public static class PromotionHelper
    {
        private const decimal HappyHourRate = 0.20m;

        //round like a receipt
        private static decimal Round2(decimal v) =>
            decimal.Round(v, 2, MidpointRounding.AwayFromZero);

        //“Hot” check that ignores case; anything else isn’t “Hot”
        private static bool IsHot(string? temp) =>
            temp != null && temp.Equals("Hot", StringComparison.OrdinalIgnoreCase);

        public static PromotionResult Apply(IEnumerable<Beverage> beverages, IEnumerable<string> promoCodes)
        {
            //normalize input
            var codes = (promoCodes ?? Array.Empty<string>())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Select(c => c.Trim().ToUpperInvariant())
                        .ToArray();

            var list = beverages?.ToList() ?? new List<Beverage>();

            //price each drink using our clean calculator
            var priceInfos = list.Select(b => PriceCalculator.CalculateBeverageSubtotal(b)).ToList();

            //start with pass-through items (no discount yet)
            var perItem = priceInfos.Select(p => new PromotionItemTotal
            {
                Original = p.Subtotal,
                Discount = 0m
            }).ToList();

            var lines = new List<DiscountLine>();

            // ---- 1) HAPPYHOUR: 20% off Hot drinks only (per-item) ----
            if (codes.Contains("HAPPYHOUR"))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (IsHot(list[i].Temp))
                    {
                        var d = Round2(perItem[i].Original * HappyHourRate);        //20% off that item
                        if (d > 0)
                        {
                            perItem[i] = new PromotionItemTotal
                            {
                                Original = perItem[i].Original,
                                Discount = Round2(perItem[i].Discount + d)
                            };
                        }
                    }
                }

                //compute HH-only discount for the summary line
                var hhAmount = ComputeHappyHourOnly(list, priceInfos);
                if (hhAmount > 0)
                {
                    lines.Add(new DiscountLine
                    {
                        Reason = "HAPPYHOUR: 20% off Hot drinks",
                        Amount = hhAmount
                    });
                }
            }

            //effective prices after HH (used to decide BOGO target)
            var effective = perItem.Select(it => it.Final).ToList();

            // ---- 2) BOGO: once per order, free (equal/lesser) among the two most expensive ----
            if (codes.Contains("BOGO") && effective.Count >= 2)
            {
                //sort indices by price desc, then by index to keep it stable
                var idxs = effective
                    .Select((price, idx) => (price, idx))
                    .OrderByDescending(t => t.price)
                    .ThenBy(t => t.idx)
                    .ToList();

                var first  = idxs[0];  //highest effective price
                var second = idxs[1];  //second highest

                //free the cheaper of these two (equal/lesser rule)
                var freeIdx = first.price <= second.price ? first.idx : second.idx;
                var freeAmt = Round2(effective[freeIdx]);

                if (freeAmt > 0)
                {
                    perItem[freeIdx] = new PromotionItemTotal
                    {
                        Original = perItem[freeIdx].Original,
                        Discount = Round2(perItem[freeIdx].Discount + freeAmt)
                    };

                    lines.Add(new DiscountLine
                    {
                        Reason = "BOGO: free item (once per order)",
                        Amount = freeAmt
                    });
                }
            }

            //final tallies
            var totalDiscount = Round2(perItem.Sum(it => it.Discount));
            var finalOrder    = Round2(perItem.Sum(it => it.Final));

            return new PromotionResult
            {
                Items = perItem,
                TotalDiscount = totalDiscount,
                FinalOrderTotal = finalOrder,
                Lines = lines
            };
        }

        //HH-only sum for a clean “HAPPYHOUR” line
        private static decimal ComputeHappyHourOnly(IReadOnlyList<Beverage> beverages, IReadOnlyList<PriceBreakdown> priceInfos)
        {
            decimal sum = 0m;
            for (int i = 0; i < beverages.Count; i++)
            {
                if (IsHot(beverages[i].Temp))
                {
                    sum += Round2(priceInfos[i].Subtotal * HappyHourRate);
                }
            }
            return Round2(sum);
        }
    }
}