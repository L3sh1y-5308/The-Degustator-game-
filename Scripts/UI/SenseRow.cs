// SenseRow.cs
// Вешается на каждую строку в сайдпанели (Toggle + Dropdown).
// Показывает только разблокированные навыки через ActionUnlockManager.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Degustation;

public class SenseRow : MonoBehaviour
{
    [SerializeField] public SenseType senseType;
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Dropdown dropdown;

    private InspectionPoints _points;
    private ActionUnlockManager _unlockManager;

    // Карта индекс дропдауна → SubActionData (для сохранения)
    private List<SubActionData> _currentActions = new();

    private void Awake()
    {
        dropdown.interactable = false;
    }

    public void Setup(SenseType sense, bool isOn, int savedActionIndex,
                      InspectionPoints points, ActionUnlockManager unlockManager)
    {
        _points        = points;
        _unlockManager = unlockManager;
        senseType      = sense;

        // отписываемся перед установкой значений
        toggle.onValueChanged.RemoveListener(OnToggleChanged);

        // заполняем дропдаун только разблокированными навыками
        _currentActions = _unlockManager.GetUnlockedForSense(sense);
        var options = new List<string>();
        foreach (var action in _currentActions)
            options.Add(action.displayName);

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        toggle.SetIsOnWithoutNotify(isOn);
        dropdown.value        = Mathf.Clamp(savedActionIndex, 0, Mathf.Max(0, options.Count - 1));
        dropdown.interactable = isOn;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            bool ok = _points.Spend();
            if (!ok)
            {
                toggle.SetIsOnWithoutNotify(false);
                return;
            }
        }
        else
        {
            _points.Refund();
        }
        dropdown.interactable = isOn;
    }

    public bool IsActive()       => toggle.isOn;
    public int  GetActionIndex() => dropdown.value;

    // Возвращает выбранный SubActionData или null
    public SubActionData GetSelectedAction()
    {
        if (!toggle.isOn || _currentActions.Count == 0) return null;
        int idx = dropdown.value;
        return idx < _currentActions.Count ? _currentActions[idx] : null;
    }
}
