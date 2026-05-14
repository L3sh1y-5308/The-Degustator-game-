using UnityEngine;
using System.Collections.Generic;
using BayatGames.SaveGameFree;

namespace Degustation
{

    public class TasteHandler : MonoBehaviour
    {

        [SerializeField] private SubActionData actionData; // Назначь в инспекторе

        public void ReadEnumOfTaste()
        {
            SenseType senseType = actionData.senseType;
            UnlockType unlockType = actionData.unlockType;


            if (senseType == SenseType.Taste)
            {
                Debug.Log("Обрабатываем вкус...");
            }
            /*
            SenseType taste = SenseType.Taste;
            TasteAction action = TasteAction.Lick;
            UnlockType unlockType = UnlockType.Starter;


            */

        }
    }
}
