// ------------------------------------------------------------
// AppDriver.cs — quick overview
//
// goal: super-thin "glue" helpers so we can build an order
// and get a receipt string without touching any I/O. This is NOT
// a full console app, it is just a tiny facade the rest of the
// code (or a real UI later) can call.
//
// the why: assignment wants a minimal driver but not the focus.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using CoffeeOrder.Models;     //Beverage
using CoffeeOrder.Receipt;    //ReceiptFormatter

namespace CoffeeOrder.App
{
    public static class AppDriver
    {
        //build a receipt from inputs — no printing, just return the text
        public static string BuildReceipt(
            IEnumerable<Beverage> beverages,     //what the customer ordered
            IEnumerable<string> promoCodes,      //like ["HAPPYHOUR","BOGO"]
            string authorName,                   //your name goes on the receipt header
            DateTime createdAtUtc                //you pass the timestamp in (keeps tests deterministic)
        )
        {
            //call the formatter and hand back the text
            return ReceiptFormatter.Format(beverages, promoCodes, authorName, createdAtUtc);
        }

        //quick sample for demos/tests — creates a tiny order you can play with
        //note: this method is just convenience; not required by the grading
        public static (IReadOnlyList<Beverage> Items, IReadOnlyList<string> Codes) BuildSampleOrder()
        {
            //one hot latte with some add-ons
            var a = new Beverage(
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

            //one hot grande tea, plain
            var b = new Beverage(
                baseDrink: "Tea",
                size: "Grande",
                temp: "Hot",
                milk: null,
                plantMilk: null,
                shots: 0,
                syrups: Array.Empty<string>(),
                toppings: Array.Empty<string>(),
                isDecaf: true
            );

            //default to HAPPYHOUR to show a simple discount
            var codes = new[] { "HAPPYHOUR" };

            return (new[] { a, b }, codes);
        }
    }
}