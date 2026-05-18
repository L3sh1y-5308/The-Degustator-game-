
namespace Degustation
{
    public class PoisonosActions
    {
        enum PoisonosAction
        {
            LickPoison,         // Лизнуть яд
            TastePoison,        // Попробовать яд
            HoldOnTonguePoison, // Подержать яд на языке
            HoldUnderTonguePoison // Подержать яд под языком
        }

        public enum PoisonType
        {
            None,          // Без яда: чистая еда
            Arsenic,       // Мышьяк: боли в животе, имитирует холеру
            Belladonna,    // Белладонна: расширяет зрачки, галлюцинации
            Aconite,       // Аконит: немеет язык, быстрая смерть от удушья
            Henbane,       // Белена: безумие, дезориентация, падение статов
            Mercury,       // Ртуть: накапливаемый яд, дрожь в руках
            Lead           // Свинец: медленный упадок здоровья, язвы
        }

        public enum DiseaseType
        {
            None,          // Здоров: персонаж ничем не болен
            Plague,        // Чума: бубоны, высокая смертность, без шансов
            SweatingSickness, // Английский пот: лихорадка, убивает за сутки
            Syphilis,      // Сифилис: язвы на теле, штраф к харизме
            Scurvy,        // Цинга: выпадают зубы, истощение от плохой еды
            Ergotism       // Эрготизм: галлюцинации, жжение и гангрена рук/ног
        }



    }
}
