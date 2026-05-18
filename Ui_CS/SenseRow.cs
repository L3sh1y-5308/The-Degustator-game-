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

    private void Awake()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        dropdown.interactable = false;
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

    public void Setup(SenseType sense, bool isOn, int savedAction, InspectionPoints points)
    {
        _points = points;
        senseType = sense;

        toggle.onValueChanged.RemoveListener(OnToggleChanged);

        var names = sense switch
        {
            SenseType.Taste => System.Enum.GetNames(typeof(TasteAction)),
            SenseType.Vision => System.Enum.GetNames(typeof(VisionAction)),
            SenseType.Touch => System.Enum.GetNames(typeof(TouchAction)),
            SenseType.Smell => System.Enum.GetNames(typeof(SmellAction)),
            SenseType.Hearing => System.Enum.GetNames(typeof(HearingAction)),
            _ => new string[] { "—" }
        };

        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>(names));

        toggle.SetIsOnWithoutNotify(isOn);
        dropdown.value = savedAction;
        dropdown.interactable = isOn;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    public bool IsActive() => toggle.isOn;
    public int GetAction() => dropdown.value;
}