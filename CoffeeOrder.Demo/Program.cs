//super tiny demo that prints a sample receipt to the console
//note: this is outside the core logic, so it is fine for a quick run.
//This is optional and can be deleted (AKA not part of submission).

using System;
using CoffeeOrder.App;   // AppDriver lives here

class Program
{
    static void Main()
    {
        //build a small, known-good order (latte + tea) with HAPPYHOUR
        var (items, codes) = AppDriver.BuildSampleOrder();

        //my real name is name here so it shows on the receipt header :)
        var receipt = AppDriver.BuildReceipt(items, codes, "Jaden Mardini", DateTime.UtcNow);

        //print the receipt text
        Console.WriteLine(receipt);
    }
} 