using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MashingMiniGame : MonoBehaviour
{
    [Header("UI")]
    public Image fillBar;
    public TextMeshProUGUI timerText;

    [Header("Game Settings")]
    public float timeLimit = 4f;       // Сколько секунд длится игра
    public float clickPower = 0.1f;    // Сколько % добавляет один клик (0.1 = 10%)
    public float decayRate = 0.3f;     // Как быстро убывает шкала в секунду (0.3 = 30% в сек)

    private float currentProgress = 0f;
    private float timeLeft;
    private bool isPlaying = true;

    void Start()
    {
        timeLeft = timeLimit;
        fillBar.fillAmount = currentProgress;
    }

    void Update()
    {
        if (!isPlaying) return;

        // Обновляем таймер
        timeLeft -= Time.deltaTime;
        timerText.text = timeLeft.ToString("F1"); // Формат с одной цифрой после запятой

        // Шкала постоянно падает вниз
        currentProgress -= decayRate * Time.deltaTime;

        // При нажатии на Пробел увеличиваем прогресс
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentProgress += clickPower;
        }

        // Ограничиваем значения от 0 до 1
        currentProgress = Mathf.Clamp01(currentProgress);
        fillBar.fillAmount = currentProgress;

        // Проверка условий победы/поражения
        if (currentProgress >= 1f)
        {
            WinGame();
        }
        else if (timeLeft <= 0f)
        {
            LoseGame();
        }
    }

    private void WinGame()
    {
        isPlaying = false;
        timerText.text = "WIN!";
        Debug.Log("<color=green>Вы выжили!</color>");
    }

    private void LoseGame()
    {
        isPlaying = false;
        timerText.text = "DEAD!";
        Debug.Log("<color=red>Время вышло! Вы погибли.</color>");
    }
}