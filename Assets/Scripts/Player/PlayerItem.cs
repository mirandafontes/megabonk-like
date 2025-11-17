using System;
using ScriptableObjects;
using UnityEngine;

namespace Player
{
    //Essa classe merecia tratamento mais adequado.
    //Está crua devido a falta de tempo hábil.
    public class PlayerItems : MonoBehaviour
    {
        [Header("- Dependencies -")]
        //Debug apenas
        [SerializeField] private PlayerStats playerStats;

        public void Initialize(PlayerStats stats)
        {
            playerStats = stats;
        }

        // Apenas aplica o modificador,
        // mas não gerencia o estado do item (como nível/dados),
        // o que seria necessário para um sistema completo.
        public void ApplyItem(ItemBlueprint newItem)
        {
            if (playerStats == null)
            {
                Debug.LogError("[PlayerItems] PlayerStats dependency is missing!");
                return;
            }

            Action<float> modifierAction = newItem.ItemType switch
            {
                ItemBlueprint.ModifierType.Speed => playerStats.AddSpeedModifier,
                ItemBlueprint.ModifierType.Health => playerStats.AddHealthModifier,
                _ => (value) => Debug.LogError($"[PlayerItems] Unhandled ModifierType: {newItem.ItemType}")
            };

            modifierAction.Invoke(newItem.GetItemModifier().ModifierValue);
        }

    }
}