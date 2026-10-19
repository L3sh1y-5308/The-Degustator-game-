using System.Collections.Generic;
using UnityEngine;

namespace Degustation
{
    public class FoodServant : MonoBehaviour
    {
        [Header("Настройки Меню")]
       // [SerializeField] public FoodMenu foodMenu; 

        [Header("Настройки Стола и Спавна")]
        [SerializeField] private Transform spawnPoint; 
        [SerializeField] private Transform targetTable; 
        [SerializeField] private Animator tableAnimator; 


    }
}