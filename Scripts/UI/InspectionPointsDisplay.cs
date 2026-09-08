using UnityEngine;
using TMPro;
using Degustation;

public class InspectionPointsDisplay : MonoBehaviour
{
    [SerializeField] private InspectionPoints points;
    [SerializeField] private TMP_Text label; // например "3 / 5"

    private void OnEnable() => points.OnChanged += Refresh;
    private void OnDisable() => points.OnChanged -= Refresh;

    private void Start() => Refresh();

    private void Refresh() => label.text = $"{points.Free} / {points.MaxPoints}";
}