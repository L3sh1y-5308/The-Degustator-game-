using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode()]
public class ProgressBar : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/ProgressBar")]
    public static void AddProgressBar()
    {
        // 1. Пытаемся загрузить префаб
        GameObject prefab = Resources.Load<GameObject>("UI/ProgressBar");
        GameObject obj;

        if (prefab != null)
        {
            obj = Instantiate(prefab);
        }
        else
        {
            // 2. Если префаба нет, создаем иерархию программно
            obj = new GameObject("ProgressBar", typeof(RectTransform), typeof(ProgressBar));
            
            // Создаем фон
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(obj.transform, false);
            bg.GetComponent<Image>().color = Color.gray;

            // Создаем маску (заполняемую часть)
            GameObject maskObj = new GameObject("Mask", typeof(RectTransform), typeof(Image));
            maskObj.transform.SetParent(obj.transform, false);
            
            Image maskImg = maskObj.GetComponent<Image>();
            maskImg.type = Image.Type.Filled;
            maskImg.fillMethod = Image.FillMethod.Horizontal;
            maskImg.fillOrigin = 0; // Left

            // Привязываем ссылку к скрипту
            obj.GetComponent<ProgressBar>().mask = maskImg;

            Debug.LogWarning("Префаб не найден в Resources/UI/ProgressBar. Создан стандартный объект.");
        }

        // Автоматический поиск Canvas, если ничего не выбрано
        GameObject parent = Selection.activeGameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            parent = canvas.gameObject;
        }

        obj.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(obj, "Create ProgressBar");
        Selection.activeGameObject = obj;
    }
#endif

    public int minimum = 0; 
    public int maximum = 100; 
    public Image mask; 
    public int current = 0; 

    void Update()
    {
        GetCurrentFill();
    }

    void GetCurrentFill()
    {
        // Prevent NullReferenceException if mask is not assigned
        if (mask == null) return;

        float currentOfSet = current - minimum;
        float maximumOfSet = maximum - minimum;

        // Prevent division by zero
        if (maximumOfSet <= 0) 
        {
            mask.fillAmount = 0;
            return;
        }

        float fillAmount = currentOfSet / maximumOfSet;
        mask.fillAmount = Mathf.Clamp01(fillAmount);
    }
}
