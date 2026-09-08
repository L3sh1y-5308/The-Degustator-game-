// OrganView.cs
// 2D-иконка органа (глаз / рот / рука / нос / ухо) на Canvas.
// Спрайт меняется автоматически по HP из SenseStatsData.
//
// Это вынутая рабочая часть старого ItemSlot (SlotType = Organ).
// Органы остаются 2D-графикой поверх 3D-сцены.
//
// НАСТРОЙКА: Image + этот компонент, укажи organSense и playerStats.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Degustation
{
    [RequireComponent(typeof(Image))]
    public class OrganView : MonoBehaviour
    {
        [Header("Какой орган показываем")]
        [SerializeField] private SenseType organSense = SenseType.Vision;

        [Header("Данные игрока")]
        [SerializeField] private SenseStatsData playerStats;

        [Header("Опционально")]
        [Tooltip("Полоска HP (Image.type = Filled)")]
        [SerializeField] private Image hpFill;

        [Tooltip("Текст HP, например «72»")]
        [SerializeField] private TMP_Text hpLabel;

        [Tooltip("Короткая вспышка красным при получении урона")]
        [SerializeField] private bool flashOnDamage = true;
        [SerializeField] private Color flashColor   = new Color(1f, 0.35f, 0.35f);
        [SerializeField] private float flashTime    = 0.15f;

        public SenseType Organ => organSense;

        private Image _image;
        private float _flashLeft;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.preserveAspect = true;
        }

        private void OnEnable()
        {
            if (playerStats == null)
            {
                Debug.LogWarning($"[OrganView] {name}: не назначен SenseStatsData.");
                return;
            }
            playerStats.OnOrganDamaged += OnOrganDamaged;
            Refresh();
        }

        private void OnDisable()
        {
            if (playerStats != null) playerStats.OnOrganDamaged -= OnOrganDamaged;
        }

        private void OnOrganDamaged(DamageEvent evt)
        {
            if (evt.Organ != organSense) return;
            Refresh();
            if (flashOnDamage) _flashLeft = flashTime;
        }

        public void Refresh()
        {
            if (playerStats == null) return;

            var sprite = playerStats.GetCurrentSprite(organSense);
            if (sprite != null) _image.sprite = sprite;
            _image.color = Color.white;

            int hp = playerStats.GetHp(organSense);
            if (hpFill  != null) hpFill.fillAmount = Mathf.Clamp01(hp / 100f);
            if (hpLabel != null) hpLabel.text      = hp.ToString();
        }

        private void Update()
        {
            if (_flashLeft <= 0f) return;

            _flashLeft -= Time.unscaledDeltaTime;
            _image.color = _flashLeft > 0f
                ? Color.Lerp(Color.white, flashColor, _flashLeft / flashTime)
                : Color.white;
        }
    }
}
