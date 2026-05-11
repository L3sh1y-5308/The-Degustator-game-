using UnityEngine;

// SB // Balance NPC money — Кошелёк НПС с классом богатства
[CreateAssetMenu(fileName = "NPCWallet", menuName = "SB/NPCWallet")]
public class NPCWallet : ScriptableObject
{
    // ── Класс богатства ───────────────────────────────────────────
    public enum WealthClass
    {
        SuperRich,   // Super Reach
        Rich,        // Reach
        Medium,      // Medium class
        PoorPlus,    // Poor +
        Poor         // Poor
    }

    [Header("Класс НПС")]
    public WealthClass wealthClass = WealthClass.Medium;

    [Header("Наличные у НПС (сколько при себе)")]
    [Tooltip("Устанавливается автоматически при Init(), но можно переопределить")]
    [SerializeField] private float _cash;

    // ── Диапазоны выплат за работу ────────────────────────────────
    // Читай: «сколько НПС готов заплатить за квест / работу»
    // Каждый класс имеет (min, max) — рандом выбирается при PayForJob()

    // SuperRich  : 500–2000
    // Rich       : 200–700
    // Medium     : 50–200
    // PoorPlus   : 15–60
    // Poor       : 3–20

    private static readonly float[,] PayRange = new float[,]
    {
        { 500f, 2000f },   // SuperRich
        { 200f,  700f },   // Rich
        {  50f,  200f },   // Medium
        {  15f,   60f },   // PoorPlus
        {   3f,   20f }    // Poor
    };

    // ── Стартовые наличные ────────────────────────────────────────
    // Наличные = случайная сумма ≈ 3–5 выплат
    private static readonly float[,] CashRange = new float[,]
    {
        { 3000f, 8000f },  // SuperRich
        { 1000f, 3500f },  // Rich
        {  150f,  700f },  // Medium
        {   40f,  180f },  // PoorPlus
        {    5f,   50f }   // Poor
    };

    // ── API ───────────────────────────────────────────────────────

    /// Инициализировать кошелёк (вызови при спавне НПС)
    public void Init()
    {
        int i = (int)wealthClass;
        _cash = Random.Range(CashRange[i, 0], CashRange[i, 1]);
    }

    /// Вернуть случайную сумму оплаты за работу (НЕ снимает деньги)
    public float RollJobPayment()
    {
        int i = (int)wealthClass;
        return Random.Range(PayRange[i, 0], PayRange[i, 1]);
    }

    /// Выплатить игроку: снять с кошелька и отдать в PlayerScore
    /// Возвращает фактически выплаченную сумму (может быть меньше, если НПС беден)
    public float PayPlayer(PlayerScore player)
    {
        float payment = RollJobPayment();
        float actual = Mathf.Min(payment, _cash);   // не больше, чем есть
        _cash -= actual;
        player.AddMoney(actual);

        // Небольшой репутационный бонус за выполненную работу
        player.AddReputation(2);
        return actual;
    }

    public float Cash => _cash;

    // ── Debug ─────────────────────────────────────────────────────
    public override string ToString() =>
        $"[NPCWallet] Class={wealthClass} Cash={_cash:F1} " +
        $"PayRange=[{PayRange[(int)wealthClass, 0]}-{PayRange[(int)wealthClass, 1]}]";
}
