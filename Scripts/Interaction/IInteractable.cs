// IInteractable.cs
// Контракт для всего, на что игрок может навести курсор и кликнуть в 3D.
// Сейчас реализуется FoodItem, дальше — двери, NPC, инструменты дегустатора.

namespace Degustation
{
    public interface IInteractable
    {
        /// Что показать в подсказке под курсором
        string DisplayName { get; }

        /// Доступен ли объект прямо сейчас (например, блюдо уже в руках → false)
        bool CanInteract { get; }

        void OnHoverEnter();
        void OnHoverExit();
        void OnInteract();
    }
}
