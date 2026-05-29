using UnityEngine;
using UnityEngine.UI;

public class CircularSkillCheck : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform needle;
    public Image successZoneImage;

    [Header("Settings")]
    public float rotationSpeed = 200f; // Градусов в секунду
    [Range(0f, 360f)]
    public float targetStartAngle = 45f; // Где начинается зона (в градусах от верха)
    [Range(0f, 1f)]
    public float successZoneSize = 0.15f; // Размер зоны (15% от круга)

    private float currentAngle = 0f;
    private bool isActive = true;

    void Start()
    {
        // Настраиваем визуализацию зоны успеха
        successZoneImage.fillAmount = successZoneSize;
        successZoneImage.rectTransform.localEulerAngles = new Vector3(0, 0, -targetStartAngle);
    }

    void Update()
    {
        if (!isActive) return;

        // Вращаем стрелку по часовой стрелке
        currentAngle += rotationSpeed * Time.deltaTime;
        currentAngle %= 360f; // Держим угол в пределах 0-360
        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Evaluate();
        }
    }

    private void Evaluate()
    {
        isActive = false;

        // Вычисляем границы зоны успеха
        float targetEndAngle = targetStartAngle + (successZoneSize * 360f);

        // Проверяем попадание (с учетом того, что зона может пересекать нулевую отметку)
        bool isHit = false;
        if (targetEndAngle > 360f)
        {
            isHit = (currentAngle >= targetStartAngle && currentAngle <= 360f) ||
                    (currentAngle >= 0f && currentAngle <= targetEndAngle % 360f);
        }
        else
        {
            isHit = (currentAngle >= targetStartAngle && currentAngle <= targetEndAngle);
        }

        if (isHit)
        {
            Debug.Log("<color=green>УСПЕХ!</color> Вы попали в зону.");
        }
        else
        {
            Debug.Log("<color=red>ПРОВАЛ!</color> Вы промахнулись.");
        }
    }
}