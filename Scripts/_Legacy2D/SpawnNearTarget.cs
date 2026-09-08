// SpawnNearTarget.cs
// Спавн UI-панели рядом с целевым объектом на Canvas.
// FIX: использует RectTransform.anchoredPosition вместо world position.

using UnityEngine;
using UnityEngine.UI;

public class SpawnNearTarget : MonoBehaviour
{
    [Tooltip("Префаб UI-панели для спавна")]
    public GameObject prefabToSpawn;

    [Tooltip("RectTransform целевого объекта (рядом с которым появится панель)")]
    public RectTransform targetRect;

    [Tooltip("Корневой RectTransform Canvas")]
    public RectTransform canvasRect;

    [Tooltip("Смещение в Canvas-пикселях (вправо от цели)")]
    public float offsetRight = 200f;

    public Button spawnButton;

    private GameObject _spawnedInstance;

    private void Start()
    {
        if (spawnButton != null)
            spawnButton.onClick.AddListener(SpawnNextToTarget);
    }

    public void SpawnNextToTarget()
    {
        // Тогл: если уже есть — закрыть
        if (_spawnedInstance != null)
        {
            Destroy(_spawnedInstance);
            _spawnedInstance = null;
            return;
        }

        if (prefabToSpawn == null || targetRect == null || canvasRect == null)
        {
            Debug.LogWarning("[SpawnNearTarget] Не назначены поля!");
            return;
        }

        _spawnedInstance = Instantiate(prefabToSpawn, canvasRect);

        var spawnedRect = _spawnedInstance.GetComponent<RectTransform>();
        if (spawnedRect != null)
            spawnedRect.anchoredPosition = targetRect.anchoredPosition + new Vector2(offsetRight, 0f);
    }
}
