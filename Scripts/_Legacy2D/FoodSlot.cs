using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public struct PlacementPointData
{
    public Transform point;
    public bool isOccupied; 
    public bool isManipulated;
}

public class FoodSlot : MonoBehaviour
{
    [SerializeField] private List<PlacementPointData> placementPoints;



    // Update is called once per frame
    void Update()
    {
        
    }
}
