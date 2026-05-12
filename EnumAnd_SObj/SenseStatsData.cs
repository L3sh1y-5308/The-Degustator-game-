// SenseStatsData.cs
// ScriptableObject — состояние органов чувств персонажа
// Каждый орган имеет 4 состояния, каждое состояние — свой спрайт

using UnityEngine;
using BayatGames.SaveGameFree;

namespace Degustation
{
    // ──────────────────────────────────────────────
    // Enums состояний (без изменений)
    // ──────────────────────────────────────────────
    public enum EyeState { Clear, Blurry, Tunnel, Blind }
    public enum MouthState { Fine, Dull, Numb, Paralyzed }
    public enum TouchState { Sensitive, Numbness, Tremor, Insensitive }
    public enum NoseState { Sharp, Faded, Phantom, Anosmia }
    public enum EarState { Acuteness, Muffled, Tinnitus, Deaf }

    // ──────────────────────────────────────────────
    // Обёртка: 4 спрайта для одного органа
    // Порядок элементов массива совпадает с порядком значений enum
    // Пример для Eye: [0]=Clear, [1]=Blurry, [2]=Tunnel, [3]=Blind
    // ──────────────────────────────────────────────
    [System.Serializable]
    public class SenseStateSprites
    {
        [Tooltip("Ровно 4 спрайта — по одному на каждое состояние органа (0→лучшее, 3→худшее)")]
        public Sprite[] stateSprites = new Sprite[4];

        // Безопасный геттер — не упадёт при незаполненном массиве
        public Sprite Get(int stateIndex)
        {
            if (stateSprites == null || stateIndex < 0 || stateIndex >= stateSprites.Length)
                return null;
            return stateSprites[stateIndex];
        }
    }

    // ──────────────────────────────────────────────
    // SO: HP органов + спрайты состояний
    // ──────────────────────────────────────────────
    [CreateAssetMenu(
        fileName = "SenseStatsData",
        menuName = "Degustation/Sense Stats Data")]
    public class SenseStatsData : ScriptableObject
    {
        // ── HP ────────────────────────────────────
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

        // ── Спрайты состояний ─────────────────────
        // Каждый SenseStateSprites содержит массив [4] спрайтов
        [Header("Спрайты состояний — Зрение")]
        public SenseStateSprites eyeSprites;

        [Header("Спрайты состояний — Рот / Вкус")]
        public SenseStateSprites mouthSprites;

        [Header("Спрайты состояний — Осязание")]
        public SenseStateSprites touchSprites;

        [Header("Спрайты состояний — Обоняние")]
        public SenseStateSprites noseSprites;

        [Header("Спрайты состояний — Слух")]
        public SenseStateSprites earSprites;

        // ── Состояния по HP ───────────────────────
        public EyeState GetEyeState() => eyeHp switch { >= 75 => EyeState.Clear, >= 25 => EyeState.Blurry, >= 1 => EyeState.Tunnel, _ => EyeState.Blind };
        public MouthState GetMouthState() => mouthHp switch { >= 75 => MouthState.Fine, >= 50 => MouthState.Dull, >= 1 => MouthState.Numb, _ => MouthState.Paralyzed };
        public TouchState GetTouchState() => touchHp switch { >= 75 => TouchState.Sensitive, >= 40 => TouchState.Numbness, >= 1 => TouchState.Tremor, _ => TouchState.Insensitive };
        public NoseState GetNoseState() => noseHp switch { >= 80 => NoseState.Sharp, >= 30 => NoseState.Faded, >= 1 => NoseState.Phantom, _ => NoseState.Anosmia };
        public EarState GetEarState() => earHp switch { >= 80 => EarState.Acuteness, >= 40 => EarState.Muffled, >= 1 => EarState.Tinnitus, _ => EarState.Deaf };

        // ── Получить актуальный спрайт органа ─────
        // Возвращает спрайт текущего состояния; удобно вызывать прямо из UI
        // Пример: senseData.GetCurrentSprite(SenseType.Vision)
        public Sprite GetCurrentSprite(SenseType sense) => sense switch
        {
            SenseType.Vision => eyeSprites.Get((int)GetEyeState()),
            SenseType.Taste => mouthSprites.Get((int)GetMouthState()),
            SenseType.Touch => touchSprites.Get((int)GetTouchState()),
            SenseType.Smell => noseSprites.Get((int)GetNoseState()),
            SenseType.Hearing => earSprites.Get((int)GetEarState()),
            _ => null
        };

        // ── Доступность чувства ────────────────────
        public bool CanUseSense(SenseType sense) => sense switch
        {
            SenseType.Vision => eyeHp > 0,
            SenseType.Taste => mouthHp > 0,
            SenseType.Touch => touchHp > 0,
            SenseType.Smell => noseHp > 0,
            SenseType.Hearing => earHp > 0,
            _ => false
        };

        // ── Сохранение / Загрузка ─────────────────
        public static void SaveSenses(SenseStatsData s)
        {
            SaveGame.Save("eye", s.eyeHp);
            SaveGame.Save("mouth", s.mouthHp);
            SaveGame.Save("touch", s.touchHp);
            SaveGame.Save("nose", s.noseHp);
            SaveGame.Save("ear", s.earHp);
            SaveGame.Save("stomach", s.stomachHp);
        }

        public static void LoadSenses(SenseStatsData s)
        {
            s.eyeHp = SaveGame.Load("eye", 100);
            s.mouthHp = SaveGame.Load("mouth", 100);
            s.touchHp = SaveGame.Load("touch", 100);
            s.noseHp = SaveGame.Load("nose", 100);
            s.earHp = SaveGame.Load("ear", 100);
            s.stomachHp = SaveGame.Load("stomach", 100);
        }
    }
}