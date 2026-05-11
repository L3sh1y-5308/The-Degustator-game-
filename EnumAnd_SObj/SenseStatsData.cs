// SenseStatsData.cs
// ScriptableObject — состояние органов чувств персонажа
// Значения берутся из SBScore.txt

using UnityEngine;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Enum состояний каждого органа
    // ──────────────────────────────────────────────
    public enum EyeState
    {
        Clear,      // 100–75: Идеальное зрение
        Blurry,     // 75–25:  Расплывается
        Tunnel,     // 25–1:   Туннельное / серое
        Blind       // 0:      Темнота
    }

    public enum MouthState
    {
        Fine,       // 100–75: Все оттенки вкуса
        Dull,       // 75–50:  Пресный / картонный
        Numb,       // 50–1:   Онемение
        Paralyzed   // 0:      Полная потеря
    }

    public enum TouchState
    {
        Sensitive,  // 100–75: Чувствует малейшее
        Numbness,   // 75–40:  Покалывание
        Tremor,     // 40–1:   Дрожь, сложно держать
        Insensitive // 0:      Полная потеря
    }

    public enum NoseState
    {
        Sharp,      // 100–80: Острый нюх
        Faded,      // 80–30:  Едва различимы
        Phantom,    // 30–1:   Фантомные запахи
        Anosmia     // 0:      Нет реакции
    }

    public enum EarState
    {
        Acuteness,  // 100–80: Слышит каждый шорох
        Muffled,    // 80–40:  Вата в ушах
        Tinnitus,   // 40–1:   Звон, речь неразличима
        Deaf        // 0:      Тишина
    }

    // ──────────────────────────────────────────────
    // SO: текущий HP органов
    // Создаётся как Asset и живёт как runtime-данные персонажа
    // ──────────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "SenseStatsData",
        menuName = "Degustation/Sense Stats Data")]
    public class SenseStatsData : ScriptableObject
    {
        [Header("Желудок")]
        [Range(0, 100)] public int stomachHp = 100;

        [Header("Зрение")]
        [Range(0, 100)] public int eyeHp = 100;

        [Header("Рот / Вкус")]
        [Range(0, 100)] public int mouthHp = 100;

        [Header("Осязание")]
        [Range(0, 100)] public int touchHp = 100;

        [Header("Обоняние")]
        [Range(0, 100)] public int noseHp = 100;

        [Header("Слух")]
        [Range(0, 100)] public int earHp = 100;

        // ──────────────────────────────────────────────
        // Авто-вычисление состояния по HP
        // Вызывается из UI или логики проверки доступности действий
        // ──────────────────────────────────────────────
        public EyeState   GetEyeState()   => eyeHp   switch { >= 75 => EyeState.Clear,   >= 25 => EyeState.Blurry, >= 1 => EyeState.Tunnel,     _ => EyeState.Blind };
        public MouthState GetMouthState() => mouthHp switch { >= 75 => MouthState.Fine,  >= 50 => MouthState.Dull, >= 1 => MouthState.Numb,      _ => MouthState.Paralyzed };
        public TouchState GetTouchState() => touchHp switch { >= 75 => TouchState.Sensitive, >= 40 => TouchState.Numbness, >= 1 => TouchState.Tremor, _ => TouchState.Insensitive };
        public NoseState  GetNoseState()  => noseHp  switch { >= 80 => NoseState.Sharp,  >= 30 => NoseState.Faded, >= 1 => NoseState.Phantom,    _ => NoseState.Anosmia };
        public EarState   GetEarState()   => earHp   switch { >= 80 => EarState.Acuteness, >= 40 => EarState.Muffled, >= 1 => EarState.Tinnitus, _ => EarState.Deaf };

        // Проверка: можно ли вообще использовать данное чувство
        public bool CanUseSense(SenseType sense) => sense switch
        {
            SenseType.Vision  => eyeHp   > 0,
            SenseType.Taste   => mouthHp > 0,
            SenseType.Touch   => touchHp > 0,
            SenseType.Smell   => noseHp  > 0,
            SenseType.Hearing => earHp   > 0,
            _                 => false
        };
    }
}
