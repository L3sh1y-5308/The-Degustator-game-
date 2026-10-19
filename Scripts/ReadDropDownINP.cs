using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadDropDownINP : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropDownINP;






    public void ReadDropDown()
    {
        int selectedValue = dropDownINP.value;
        string selectedText = dropDownINP.options[selectedValue].text;



        Debug.Log("Selected value: " + selectedValue + ", Selected text: " + selectedText);
    }



    public void TranslateEnterValue(string selectedText)
    {
    
        selectedText.GetHashCode();


    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
