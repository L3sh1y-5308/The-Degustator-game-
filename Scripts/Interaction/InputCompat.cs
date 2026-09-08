// InputCompat.cs
// Единая обёртка над старым и новым Input System.
// Нужна чтобы код не падал независимо от того, что стоит в
// Project Settings → Player → Active Input Handling.
//
// Если нужна новая клавиша — добавь её в switch в KeyDown().

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Degustation
{
    public static class InputCompat
    {
        public static Vector2 MousePosition
        {
#if ENABLE_INPUT_SYSTEM
            get => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            get => Input.mousePosition;
#endif
        }

        public static bool LeftClickDown =>
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            Input.GetMouseButtonDown(0);
#endif

        public static bool RightClickDown =>
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            Input.GetMouseButtonDown(1);
#endif

        public static bool KeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return false;

            return key switch
            {
                KeyCode.Escape => kb.escapeKey.wasPressedThisFrame,
                KeyCode.Space  => kb.spaceKey.wasPressedThisFrame,
                KeyCode.Tab    => kb.tabKey.wasPressedThisFrame,
                KeyCode.E      => kb.eKey.wasPressedThisFrame,
                KeyCode.F      => kb.fKey.wasPressedThisFrame,
                KeyCode.Q      => kb.qKey.wasPressedThisFrame,
                KeyCode.R      => kb.rKey.wasPressedThisFrame,
                _              => false
            };
#else
            return Input.GetKeyDown(key);
#endif
        }
    }
}
