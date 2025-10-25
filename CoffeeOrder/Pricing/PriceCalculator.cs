// ------------------------------------------------------------
// PriceCalculator.cs — quick overview
//
// goal: do the boring math for a single drink and for an order.
//
// I keep it tiny and pure: base price by Size + add-ons.
// no promotions here (that is next step), no I/O, only decimals.
//
// rounding: 2 decimals using AwayFromZero so totals look like receipts.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeOrder.Models;

namespace CoffeeOrder.Pricing
{
    //just a tiny container so tests can assert the pieces if needed
    public sealed class PriceBreakdown
    {
        public decimal BasePrice { get; init; }    //the size-based price
        public decimal Shots { get; init; }        //shots cost
        public decimal Syrups { get; init; }       //syrups cost
        public decimal PlantMilk { get; init; }    //plant milk surcharge
        public decimal Toppings { get; init; }     //toppings cost
        public decimal Subtotal { get; init; }     //sum rounded at 2 dp
    }

    public static class PriceCalculator
    {
        //prices kept internal + obvious, change here = tests tell you what broke
        private const decimal TallBase   = 3.00m;
        private const decimal GrandeBase = 3.50m;
        private const decimal VentiBase  = 4.00m;

        private const decimal ShotPrice      = 0.75m;
        private const decimal SyrupPrice     = 0.30m;
        private const decimal PlantMilkPrice = 0.60m;       //dairy is free in this simple model
        private const decimal ToppingPrice   = 0.25m;

        //helper: round like receipts (no bankers rounding surprises)
        private static decimal Round2(decimal v)
            => decimal.Round(v, 2, MidpointRounding.AwayFromZero);

        //figure out base price from Size (simple switch on common strings)
        private static decimal GetBaseBySize(string? size)
        {
            if (string.IsNullOrWhiteSpace(size)) return 0m;     //if validator missed it, I dont blow up

            //accept typical coffee sizes case-insensitively
            var s = size.Trim();
            if (s.Equals("Tall",   StringComparison.OrdinalIgnoreCase))   return TallBase;
            if (s.Equals("Grande", StringComparison.OrdinalIgnoreCase))   return GrandeBase;
            if (s.Equals("Venti",  StringComparison.OrdinalIgnoreCase))   return VentiBase;

            //unknown size -> treat as Tall price to stay predictable
            return TallBase;
        }

        //price a single beverage and return a breakdown (nice for tests)
        public static PriceBreakdown CalculateBeverageSubtotal(Beverage bev)
        {
            //base is purely from size
            var basePrice = GetBaseBySize(bev.Size);

            //shots are 0.75 each, negative counts shouldn't happen (validator),
            //but I will clamp at 0 to avoid weird totals if called directly.
            var shotsCost = ShotPrice * Math.Max(0, bev.Shots);

            //syrups: 0.30 each (null list => 0)
            var syrupCount = bev.Syrups?.Length ?? 0;
            var syrupsCost = SyrupPrice * Math.Max(0, syrupCount);

            //plant milk surcharge if any plant milk is set (not null/empty)
            var plantMilkCost = string.IsNullOrWhiteSpace(bev.PlantMilk) ? 0m : PlantMilkPrice;

            //toppings: 0.25 each (null list => 0)
            var toppingCount = bev.Toppings?.Length ?? 0;
            var toppingsCost = ToppingPrice * Math.Max(0, toppingCount);

            //sum up and round like a receipt
            var subtotal = Round2(basePrice + shotsCost + syrupsCost + plantMilkCost + toppingsCost);

            return new PriceBreakdown
            {
                BasePrice = Round2(basePrice),
                Shots = Round2(shotsCost),
                Syrups = Round2(syrupsCost),
                PlantMilk = Round2(plantMilkCost),
                Toppings = Round2(toppingsCost),
                Subtotal = subtotal
            };
        }

        //price multiple drinks — super simple sum of subtotals
        public static decimal CalculateOrderSubtotal(IEnumerable<Beverage> beverages)
        {
            if (beverages is null) return 0m;

            decimal total = 0m;
            foreach (var b in beverages)
            {
                var one = CalculateBeverageSubtotal(b).Subtotal;
                total += one;
            }
            return Round2(total);
        }
    }
}