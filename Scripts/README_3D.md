# Структура скриптов — 3D-версия «Дегустатора»

## Что где лежит

```
Scripts/
├── Core/          GameManager, SceneSwitcher, SceneButton
├── Data/          все ScriptableObject: FoodData, SenseStatsData, PlayerScore,
│                  InspectionPoints, InspectionDamageTable, ShopInventory, NPC…
├── Enums/         SenseActions, PoisonosActions, SupliesEm
├── Gameplay/      RuntimeFood, ActionUnlockManager, InspectionProcessor, TasteHandler
├── World/         ◄ НОВОЕ, 3D-сцена
│   ├── FoodItem.cs            блюдо на столе (данные + IInteractable)
│   ├── FoodPlacementPoint.cs  точка на столе под одно блюдо
│   ├── FoodSpawner3D.cs       спавн 3D-моделей по точкам
│   └── FoodHighlight.cs       подсветка при наведении
├── Interaction/   ◄ НОВОЕ, режим осмотра
│   ├── IInteractable.cs
│   ├── PlayerInteractor.cs      клик мышью по 3D-объекту
│   ├── InspectionController.cs  «как в Skyrim»: блюдо едет в руки и обратно
│   ├── Manipulation.cs          вращение + зум в руках (переписан)
│   └── InputCompat.cs           обёртка над старым/новым Input System
├── UI/            SlidePanel, SenseRow, OrganView, HoverLabel,
│                  InspectionPointsDisplay, EndOfRoundWindow, Shop/Inventory, ProgressBar
├── Minigames/     CircularSkillCheck, MashingMiniGame
├── Editor/        Degustator3DPrefabFactory (меню Degustation → 3D)
└── _Legacy2D/     старый 2D-интерфейс — удалить, когда 3D-сцена заработает
```

`.asset`-файлы (SO-инстансы) намеренно **не** трогались — остались в
`Ui_CS/`, `ForSOFood/`, `NPCfolder/`, `ForShopItems/`, `EnumFolder/`.
Скрипты переносились вместе с `.meta`, поэтому GUID сохранены и ссылки
в префабах/сценах не отвалились.

---

## Поток игры в 3D

```
FoodSpawner3D            → расставляет FoodData.prefab3D по FoodPlacementPoint
      ↓
PlayerInteractor         → луч из камеры в курсор, подсветка, ЛКМ
      ↓
InspectionController     → блюдо летит к HoldAnchor перед камерой (DOTween)
      ↓
Manipulation             → ЛКМ-drag крутит, колесо приближает
SlidePanel.Open(food)    → панель чувств для этого блюда
      ↓  ПКМ / Esc
InspectionController     → блюдо возвращается на свою точку
      ↓  кнопка «Проверить»
InspectionProcessor      → сверяет выбор с FoodData, урон по органам
      ↓
GameManager              → очки, EndOfRoundWindow, следующая волна
```

---

## Настройка сцены (по шагам)

1. **Стол.** Пустышка `FoodSpawner` на столе → компонент `FoodSpawner3D`.
   Меню `Degustation → 3D → Create Placement Points` создаст 5 точек — раскидай
   их по столешнице, гизмо показывает, где встанет блюдо.

2. **Блюда.** У каждого `FoodData` заполни `prefab3D`. Компонент `FoodItem`
   вешать на префаб не обязательно — спавнер добавит его сам, как и коллайдер
   (строится по границам мешей, если своего нет).

3. **Риг осмотра.** Меню `Degustation → 3D → Setup Inspection Rig` создаст
   `HoldAnchor` ребёнком камеры на (0, 0, 0.55) и объект `InspectionController`
   с `PlayerInteractor`. Дальше в инспекторе назначь:
   - `targetCamera`, `holdAnchor`
   - `slidePanel` — панель чувств (можно оставить пустым)
   - `disableWhileInspecting` — скрипты движения игрока и поворота камеры

4. **Тонкая настройка блюда в руках.** На префабе в `FoodItem`:
   `inspectRotationEuler` (развернуть «лицом»), `inspectPositionOffset`,
   `inspectScale`.

5. **Органы (2D).** На Canvas — `Image` + `OrganView`, укажи `organSense`
   и `SenseStatsData`. Спрайт меняется сам по HP, есть опциональные
   полоска HP и вспышка при уроне.

6. **Слой Inspect (опционально).** Создай слой `Inspect`, впиши его имя в
   `InspectionController.inspectLayerName` — тогда блюдо в руках не будет
   резаться стенами (если добавишь отдельную near-камеру для этого слоя).

---

## Что исправлено по дороге

- **Manipulation**: вращение стало относительно камеры (мышь вправо →
  блюдо крутится вправо на экране всегда, а не в зависимости от его поворота).
  Зум больше не двигает объект по прямой к камере (объект мог пролететь
  сквозь неё) — теперь это дистанция от камеры с жёстким клампом.
  Проверка `zoneCenter`/`zoneRadius` убрана: компонент включает
  `InspectionController`, лишних `Update` нет.
- **SlidePanel**: `Input.GetKeyDown` выбрасывал исключение, если в проекте
  включён только новый Input System. Заменено на `InputCompat`.
  Добавлены явные `Open(food)` / `Close()` — `ToggleWithFood` при клике по
  второму блюду закрывал бы панель вместо смены содержимого.
- **InspectionProcessor**: урон считался по **повторному** `Roll()`, то есть
  по другим эффектам, чем те, что были у блюда на столе. Теперь берётся
  `FoodItem.Runtime`. `FindObjectOfType` убран из цикла.
- **GameManager**: `CollectResults` был заглушкой (`playerAnswer = correctAnswer`,
  `score = 10` всем). Теперь результаты берутся из `InspectionProcessor.ProcessAll()`.
- `FindObjectOfType` → `FindFirstObjectByType` под `#if UNITY_2023_1_OR_NEWER`.

---

## _Legacy2D — что там и почему

Всё это работает с Canvas/спрайтами и заменено 3D-аналогами.
Компилируется, но в новой сцене не используется. Удаляй, когда 3D заработает.

| Файл | Заменён на |
|---|---|
| `ItemSlot.cs` | `FoodItem` (еда) + `OrganView` (органы) |
| `TastedItem.cs` | `FoodItem` |
| `TastedItemSpawner.cs` | `FoodSpawner3D` |
| `SceneEventHand.cs`, `SceneLayoutSO.cs` | `FoodPlacementPoint` + `FoodSpawner3D` |
| `FoodSlotButton.cs` | `PlayerInteractor` + `IInteractable` |
| `FoodDragHandler.cs` | не нужен — блюда берутся кликом, не тащатся |
| `FoodSlot.cs` | `FoodPlacementPoint` |
| `SpawnNearTarget.cs` | UI-панель теперь открывает `InspectionController` |
| `SpinAnimation.cs` | вращение делает `Manipulation` |
| `ProgressBar.cs` (корневой) | дубликат `UI/ProgressBar.cs` (`MyGame.UI`) |
| `DropDownManager.cs`, `ReadDropDownINP.cs` | старые заглушки дропдаунов |
| `DegustatorPrefabFactory.cs` | `Degustator3DPrefabFactory` |

**Требуют отдельной переделки под 3D (пока просто лежат):**

- `CheckingPanel.cs` — карусель результатов на `SpriteRenderer` + OutlineFx.
  В 3D логичнее подсвечивать сами модели на столе по очереди.
- `IntroSequence.cs` — интро целиком на `RectTransform` (стол «выезжает»
  как UI-элемент). Диалог переиспользуем, переезд к столу — нет.
