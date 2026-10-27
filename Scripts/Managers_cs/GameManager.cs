using System.Collections.Generic;
using UnityEngine;
using static NPCclasses;


public class GameManager : MonoBehaviour
{

    public void Awake()
    {
        //Generate everyRound the npc, location >> food, items in shop

        //GenNPCs()
        //GenMapOfDays()
    }




    // Id  // serialize TypeLevel  

    //Pick the random Enum of LevelType >> Send the option to GenNPCs ; GenFoodList ; GenShopItems ; HomeType
    public void GenMapOfDays ()
    {
        // Implementation for generating the map of days
    }


    //Get setting for round >> Get NpcType  >> Send to NPc Manager
    public void GenNPCs()
    {
        // Implementation for generating NPCs
    }


    //Get setting for round >> Get FoodList >> Send to NPc Manager
    public void GenFoodList()
    {
        // Implementation for generating the food list
    }





    void Update()
    {
        
    }
}
