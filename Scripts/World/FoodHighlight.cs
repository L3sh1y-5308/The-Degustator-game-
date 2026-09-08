// FoodHighlight.cs
// Подсветка 3D-блюда при наведении курсора.
// Работает через MaterialPropertyBlock — материалы НЕ инстансятся,
// то есть не течёт память и не ломается батчинг.
//
// ВАЖНО: свечение (_EmissionColor) видно только если в материале
// включён Emission. Если у тебя URP/Lit без эмиссии — оставь
// tintBaseColor = true, будет работать подсветка цветом.
//
// Если позже подключишь OutlineFx — просто замени тело SetHighlighted.

using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    [DisallowMultipleComponent]
    public class FoodHighlight : MonoBehaviour
    {
        [Header("Цвет подсветки")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.45f);

        [SerializeField, Range(0f, 4f)] private float emissionIntensity = 1.4f;

        [Tooltip("Дополнительно подкрашивать базовый цвет (работает без Emission)")]
        [SerializeField] private bool tintBaseColor = true;

        [SerializeField, Range(0f, 1f)] private float tintStrength = 0.35f;

        private static readonly int EmissionId  = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP/HDRP
        private static readonly int ColorId     = Shader.PropertyToID("_Color");     // Built-in

        private Renderer[] _renderers;
        private readonly List<Color> _originalColors = new();
        private MaterialPropertyBlock _mpb;
        private bool _isOn;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _mpb = new MaterialPropertyBlock();

            foreach (var r in _renderers)
            {
                Color c = Color.white;
                var mat = r != null ? r.sharedMaterial : null;
                if (mat != null)
                {
                    if (mat.HasProperty(BaseColorId))  c = mat.GetColor(BaseColorId);
                    else if (mat.HasProperty(ColorId)) c = mat.GetColor(ColorId);
                }
                _originalColors.Add(c);
            }
        }

        public void SetHighlighted(bool on)
        {
            if (_isOn == on || _renderers == null) return;
            _isOn = on;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;

                r.GetPropertyBlock(_mpb);

                _mpb.SetColor(EmissionId, on ? highlightColor * emissionIntensity : Color.black);

                if (tintBaseColor)
                {
                    Color target = on
                        ? Color.Lerp(_originalColors[i], highlightColor, tintStrength)
                        : _originalColors[i];
                    _mpb.SetColor(BaseColorId, target);
                    _mpb.SetColor(ColorId, target);
                }

                r.SetPropertyBlock(_mpb);
            }
        }
    }
}
