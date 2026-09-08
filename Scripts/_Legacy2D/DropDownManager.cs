using UnityEngine;
using TMPro;

public class DropDownManager : MonoBehaviour
{
    [SerializeField] public TMP_Dropdown dropdown;
    [SerializeField] public GameObject[] panels;

    void Start()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(HandleDropdownChanged);
            // Initialize the state based on the current dropdown value
            HandleDropdownChanged(dropdown.value);
        }
    }

    public void HandleDropdownChanged(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                // Only the panel matching the current index will be active
                panels[i].SetActive(i == index);
            }
        }
    }

    void OnDestroy()
    {
        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
        }
    }
}
