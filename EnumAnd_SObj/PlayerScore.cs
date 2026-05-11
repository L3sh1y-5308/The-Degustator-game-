using UnityEngine;
using UnityEngine.Events;

// SB // Score — Счёт игрока: деньги, репутация, опыт
[CreateAssetMenu(fileName = "PlayerScore", menuName = "SB/PlayerScore")]
public class PlayerScore : ScriptableObject
{
    [Header("Баланс")]
    [SerializeField] private float _money = 100f;
    [SerializeField] private int   _reputation = 0;   // -100..+100
    [SerializeField] private int   _xp = 0;

    // Публичные readonly свойства
    public float Money      => _money;
    public int   Reputation => _reputation;
    public int   XP         => _xp;

    // События — подписывай UI/логику снаружи
    public UnityAction<float> OnMoneyChanged;
    public UnityAction<int>   OnReputationChanged;
    public UnityAction<int>   OnXPChanged;

    // ── Деньги ──────────────────────────────────────────
    public bool SpendMoney(float amount)
    {
        if (_money < amount) return false;   // недостаточно средств
        _money -= amount;
        OnMoneyChanged?.Invoke(_money);
        return true;
    }

    public void AddMoney(float amount)
    {
        _money += amount;
        OnMoneyChanged?.Invoke(_money);
    }

    // ── Репутация ────────────────────────────────────────
    // репутация зажата в [-100, +100]
    public void AddReputation(int delta)
    {
        _reputation = Mathf.Clamp(_reputation + delta, -100, 100);
        OnReputationChanged?.Invoke(_reputation);
    }

    // ── Опыт ─────────────────────────────────────────────
    public void AddXP(int amount)
    {
        _xp += amount;
        OnXPChanged?.Invoke(_xp);
    }

    // ── Утилита ──────────────────────────────────────────
    /// Сбросить всё (например при новой игре)
    public void Reset(float startMoney = 100f)
    {
        _money = startMoney;
        _reputation = 0;
        _xp = 0;
    }
}
