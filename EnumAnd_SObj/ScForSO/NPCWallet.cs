using UnityEngine;

// SB // Balance NPC money — Кошелёк НПС с классом богатства + репутационный модификатор
[CreateAssetMenu(fileName = "NPCWallet", menuName = "SB/NPCWallet")]
public class NPCWallet : ScriptableObject
{
    // ── Класс богатства ───────────────────────────────────────────
    public enum WealthClass
    {
        SuperRich,
        Rich,
        Medium,
        PoorPlus,
        Poor
    }

    [Header("Класс НПС")]
    public WealthClass wealthClass = WealthClass.Medium;

    // ── Репутационные пороги ──────────────────────────────────────
    [Header("Репутация: пороги поведения")]
    [Tooltip("Репутация игрока ВЫШЕ этого порога → НПС платит больше нормы (множитель bonusMultiplier)")]
    public int reputationThresholdBonus  =  30;   // напр. +30 и выше

    [Tooltip("Репутация игрока НИЖЕ этого порога → НПС платит меньше нормы (множитель penaltyMultiplier)")]
    public int reputationThresholdPenalty = -20;  // напр. -20 и ниже

    [Tooltip("Множитель выплаты при хорошей репутации (>= thresholdBonus)")]
    [Range(1f, 3f)]
    public float bonusMultiplier  = 1.5f;   // +50%

    [Tooltip("Множитель выплаты при плохой репутации (<= thresholdPenalty)")]
    [Range(0f, 1f)]
    public float penaltyMultiplier = 0.4f;  // -60%

    [Header("Наличные у НПС")]
    [SerializeField] private float _cash;

    // ── Диапазоны ─────────────────────────────────────────────────
    private static readonly float[,] PayRange = new float[,]
    {
        { 500f, 2000f },   // SuperRich
        { 200f,  700f },   // Rich
        {  50f,  200f },   // Medium
        {  15f,   60f },   // PoorPlus
        {   3f,   20f }    // Poor
    };

    private static readonly float[,] CashRange = new float[,]
    {
        { 3000f, 8000f },
        { 1000f, 3500f },
        {  150f,  700f },
        {   40f,  180f },
        {    5f,   50f }
    };

    // ── API ───────────────────────────────────────────────────────

    public void Init()
    {
        int i = (int)wealthClass;
        _cash = Random.Range(CashRange[i, 0], CashRange[i, 1]);
    }

    public float RollJobPayment()
    {
        int i = (int)wealthClass;
        return Random.Range(PayRange[i, 0], PayRange[i, 1]);
    }

    // Вернуть множитель выплаты на основе репутации игрока
    // Используй это чтобы просто узнать отношение НПС без транзакции
    public float GetPaymentMultiplier(int playerReputation)
    {
        if (playerReputation >= reputationThresholdBonus)  return bonusMultiplier;
        if (playerReputation <= reputationThresholdPenalty) return penaltyMultiplier;
        return 1f; // норма
    }

    // Отношение НПС к игроку: строка для UI/диалогов
    public string GetRelationLabel(int playerReputation)
    {
        if (playerReputation >= reputationThresholdBonus)  return "Уважает";
        if (playerReputation <= reputationThresholdPenalty) return "Не доверяет";
        return "Нейтральный";
    }

    /// Выплатить игроку с учётом его репутации
    /// Возвращает фактически выплаченную сумму
    public float PayPlayer(PlayerScore player)
    {
        float basePayment = RollJobPayment();
        float multiplier  = GetPaymentMultiplier(player.Reputation);
        float payment     = basePayment * multiplier;
        float actual      = Mathf.Min(payment, _cash);

        _cash -= actual;
        player.AddMoney(actual);

        // Репутационный бонус только при нормальном/хорошем отношении
        if (multiplier >= 1f)
            player.AddReputation(2);

        return actual;
    }

    public float Cash => _cash;

    // ── Debug ─────────────────────────────────────────────────────
    public override string ToString() =>
        $"[NPCWallet] Class={wealthClass} Cash={_cash:F1} " +
        $"PayRange=[{PayRange[(int)wealthClass, 0]}-{PayRange[(int)wealthClass, 1]}] " +
        $"RepBonus>={reputationThresholdBonus}(x{bonusMultiplier}) " +
        $"RepPenalty<={reputationThresholdPenalty}(x{penaltyMultiplier})";
}
