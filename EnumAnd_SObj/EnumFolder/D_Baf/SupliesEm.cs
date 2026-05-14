using UnityEngine;

namespace Degustator
{
    // Предметы экипировки для защиты органов чувств
    public enum SensoryEquipment
    {
        None,             // Нет защиты
        Glasses,          // Очки: защита для Зрения (Vision)
        Balaclava,        // Балаклава: защита для Рта (Taste) и Ушей (Hearing)
        EarBandage        // Повязки на уши: защита для Слуха (Hearing)
    }

    // Расходники и антисептики, применяемые к органам чувств
    public enum SensoryConsumable
    {
        None,             // Ничего не применено
        HolyWater,        // Святая вода: очищает Зрение, Рот, Руки, Нос
        Alcohol,          // Спирт: дезинфекция для Рта (Taste) и Рук (Touch)
        Herbs,            // Травы: защита/очищение для Рта (Taste) и Носа (Smell)
        Garlic            // Чеснок: барьер для Рта (Taste) и Рук (Touch)
    }



}


