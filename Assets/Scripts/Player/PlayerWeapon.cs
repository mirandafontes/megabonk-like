using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using Weaponry;

namespace Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("- Dependencies -")]
        [SerializeField] private PlayerInput playerInput;

        [Header("- Setup -")]
        //Para projectiles
        [SerializeField] private Transform firePoint;

        private readonly List<WeaponData> automaticWeapons = new List<WeaponData>();
        private readonly List<WeaponData> manualWeapons = new List<WeaponData>();

        #region Unity
        private void Awake()
        {
            BindInputs();
        }

        private void Update()
        {
            Quaternion fireRotation = transform.rotation;
            foreach (var weapon in automaticWeapons)
            {
                if (!weapon.IsAutomatic)
                {
                    return;
                }
                weapon.TryAttack(firePoint.position, fireRotation);
            }
        }
        #endregion

        public void EquipWeapon(WeaponData newWeapon)
        {
            if (newWeapon.IsAutomatic)
            {
                automaticWeapons.Add(newWeapon);
            }
            else
            {
                manualWeapons.Add(newWeapon);
            }
        }

        public bool LevelUpWeapon(WeaponBlueprint targetBlueprint)
        {
            WeaponData weaponToLevelUp =
                automaticWeapons.Find(w => w.Blueprint == targetBlueprint) ??
                manualWeapons.Find(w => w.Blueprint == targetBlueprint);

            if (weaponToLevelUp == null)
            {
                Debug.LogError($"[PlayerWeapon] Attempt to level up unequipped weapon: {targetBlueprint.WeaponName}");
                return false;
            }

            return weaponToLevelUp.TryLevelUp();
        }

        public WeaponData GetWeaponData(WeaponBlueprint targetBlueprint)
        {
            WeaponData automaticMatch = automaticWeapons.FirstOrDefault(w => w.Blueprint == targetBlueprint);
            if (automaticMatch != null)
            {
                return automaticMatch;
            }

            WeaponData manualMatch = manualWeapons.FirstOrDefault(w => w.Blueprint == targetBlueprint);
            if (manualMatch != null)
            {
                return manualMatch;
            }

            return null;
        }

        private void BindInputs()
        {
            InputAction attackAction = playerInput.actions.FindAction("Attack");

            if (attackAction == null)
            {
                Debug.LogError("[PlayerWeapon] = MoveAction is null. Review player actions scriptable.");
                return;
            }

            attackAction.performed += OnPressed;
        }

        private void OnPressed(InputAction.CallbackContext context)
        {
            Quaternion fireRotation = transform.rotation;
            foreach (var weapon in manualWeapons)
            {
                if (weapon.IsAutomatic)
                {
                    continue;
                }
                weapon.TryAttack(firePoint.position, fireRotation);
            }
        }
    }
}