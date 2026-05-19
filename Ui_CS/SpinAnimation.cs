// SpinAnimation.cs
// Медленное вращение спрайта через DOTween.
// Вешается на любой GameObject с SpriteRenderer или UI Image.

using UnityEngine;
using DG.Tweening;

public class SpinAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 3f;      // секунд на 1 оборот
    [SerializeField] private bool  clockwise = true;   // направление

    private void Start()
    {
        float angle = clockwise ? -360f : 360f;

        transform
            .DORotate(new Vector3(0, 0, angle), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)       // равномерная скорость без ускорений
            .SetLoops(-1, LoopType.Restart) // -1 = бесконечно
            .SetUpdate(true);           // работает даже при Time.timeScale = 0
    }

    private void OnDestroy()
    {
        transform.DOKill(); // чистим tween при уничтожении объекта
    }
}
