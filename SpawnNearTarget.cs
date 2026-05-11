using UnityEngine;
using UnityEngine.UI;

public class SpawnNearTarget : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject targetObject;
    public Transform canvasTransform;
    public float offsetRight = 1.5f;
    public Button spawnButton;

    private GameObject _spawnedInstance;

    private void Start()
    {
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(SpawnNextToTarget);
        }
    }

    public void SpawnNextToTarget()
    {
        // Если объект уже создан, удаляем его (закрываем)
        if (_spawnedInstance != null)
        {
            Destroy(_spawnedInstance);
            _spawnedInstance = null;
            return;
        }

        if (prefabToSpawn != null && targetObject != null && canvasTransform != null)
        {
            // Рассчитываем позицию относительно целевого объекта
            Vector3 spawnPos = targetObject.transform.position + new Vector3(offsetRight, 0, 0);

            // Спавн и сохранение ссылки
            _spawnedInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, canvasTransform);
        }
        else
        {
            Debug.LogWarning("Убедитесь, что prefabToSpawn, targetObject и canvasTransform назначены!");
        }



    }
}
