// ------------------------------------------------------------
// Beverage.cs  — quick overview
//
// goal: tiny data class for a drink order. I keep fields
// simple and avoid mutating after construction so tests are easy.
//
// key ideas: no I/O, just values (base drink, size, temp, milk,
// plant milk, shots, syrups, toppings, decaf flag).
//
// why: other parts (validator, classifier, pricing, etc.) can
// depend on this without side effects.
// ------------------------------------------------------------

namespace CoffeeOrder.Models;

public class Beverage
{
    public string? BaseDrink { get; }
    public string? Size { get; }
    public string? Temp { get; } // "Hot" or "Iced"
    public string? Milk { get; } // dairy milk (e.g., "2%")
    public string? PlantMilk { get; } // plant-based milk (e.g., "Oat")
    public int Shots { get; } // 0..4
    public string[] Syrups { get; }
    public string[] Toppings { get; }
    public bool IsDecaf { get; }

    public Beverage(
        string? baseDrink,
        string? size,
        string? temp,
        string? milk,
        string? plantMilk,
        int shots,
        IEnumerable<string>? syrups,
        IEnumerable<string>? toppings,
        bool isDecaf
    )
    {
        BaseDrink = baseDrink;
        Size = size;
        Temp = temp;
        Milk = milk;
        PlantMilk = plantMilk;
        Shots = shots;
        Syrups = (syrups ?? Array.Empty<string>()).ToArray();
        Toppings = (toppings ?? Array.Empty<string>()).ToArray();
        IsDecaf = isDecaf;
    }
}