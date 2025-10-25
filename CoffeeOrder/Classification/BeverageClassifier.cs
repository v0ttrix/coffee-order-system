// ------------------------------------------------------------
// BeverageClassifier.cs — quick overview
// goal: stamp simple labels on a Beverage without touching I/O
// or pricing. I will keep rules tiny and obvious so tests are painless.

// what I label: Caffeinated/Decaf, DairyFree, VeganFriendly, KidSafe.
// notes:
// - "Decaf" is whatever the beverage says via IsDecaf.
// - "Caffeinated" = has espresso shots AND not marked decaf.
// - "DairyFree" = no dairy milk string set (plant milk is fine).
// - "VeganFriendly" = for now, same as dairy-free (I am not
//   modeling honey/gelatin toppings here).
// - "KidSafe" = (IsDecaf OR Shots == 0) AND Temp != "ExtraHot".
// ------------------------------------------------------------

using System;
using CoffeeOrder.Models;

namespace CoffeeOrder.Classification
{
    //plain result bag so tests can assert flags cleanly
    public sealed class BeverageLabels
    {
        //true when the drink has caffeine via shots and isn't decaf
        public bool IsCaffeinated { get; init; }

        //mirrors the input flag, I dont overthink here
        public bool IsDecaf { get; init; }

        //true when no dairy is set (plant milk is okay)
        public bool IsDairyFree { get; init; }

        //for this assignment, vegan-friendly == dairy-free
        public bool IsVeganFriendly { get; init; }

        //safe for kids if no espresso (or decaf) and not extra hot
        public bool IsKidSafe { get; init; }
    }

    public static class BeverageClassifier
    {
        //tiny helper to compare strings without case drama
        private static bool EqualsIgnore(string? a, string? b)
            => a is not null && b is not null && a.Equals(b, StringComparison.OrdinalIgnoreCase);

        public static BeverageLabels Classify(Beverage bev)
        {
            //"decaf" straight from the model (keeps things predictable)
            var isDecaf = bev.IsDecaf;

            //has espresso shots? (any positive number means "has caffeine potential")
            var hasShots = bev.Shots > 0;

            //caffeinated iff I have shots AND I am NOT marked decaf
            var isCaffeinated = hasShots && !isDecaf;

            //dairy-free: means the "Milk" (dairy) field is empty/null
            //plant milk is okay and doesn't break dairy-free
            var isDairyFree = string.IsNullOrWhiteSpace(bev.Milk);

            //vegan-friendly: for our scope, same as dairy-free
            var isVeganFriendly = isDairyFree;

            //kid-safe: no espresso (i.e., zero shots) OR explicitly decaf,
            //AND not "ExtraHot" (if someone modeled that temp value)
            var isExtraHot = EqualsIgnore(bev.Temp, "ExtraHot");
            var isKidSafe = (isDecaf || !hasShots) && !isExtraHot;

            return new BeverageLabels
            {
                IsDecaf = isDecaf,
                IsCaffeinated = isCaffeinated,
                IsDairyFree = isDairyFree,
                IsVeganFriendly = isVeganFriendly,
                IsKidSafe = isKidSafe
            };
        }
    }
}