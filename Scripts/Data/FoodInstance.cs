// Обычный C#-класс, не SO: runtime-состояние конкретного куска еды
using Degustation;
using System.Collections.Generic;

public class FoodInstance
{
    public FoodData Base { get; }
    public List<FoodEffect> Modifiers { get; } = new();

    public FoodInstance(FoodData baseData) => Base = baseData;

    public void AddModifier(FoodEffect effect) => Modifiers.Add(effect);
}