using Player;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Weaponry;
using Event;
using System;

namespace UI
{
    //Essa classe deveria ser mais elástica
    //Não possuindo referências diretas ao itens
    //mas obtendo por meio de uma lista dinamica
    //para sugerir o level up.
    //Entretanto, estou sem tempo para implementar adequadamente
    public class PlayerLevelUpController : MonoBehaviour
    {
        [Header("- UI -")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("- Dependencies -")]
        [SerializeField] private PlayerItems playerItems;
        [SerializeField] private PlayerWeapon playerWeapon;

        [Header("- Weapon 1 -")]
        [SerializeField] private WeaponBlueprint swordBlueprint;
        [SerializeField] private Button weaponOneButtonLevelUp;
        [SerializeField] private Image weaponOneIcon;
        [SerializeField] private TextMeshProUGUI weaponOneTitle;
        [SerializeField] private TextMeshProUGUI weaponOneDMGNext;
        [SerializeField] private TextMeshProUGUI weaponOneCooldownNext;
        [SerializeField] private TextMeshProUGUI weaponOneAmountNext;

        [Header("- Weapon 2 -")]
        [SerializeField] private WeaponBlueprint bowBlueprint;
        [SerializeField] private Button weaponTwoButtonLevelUp;
        [SerializeField] private Image weaponTwoIcon;
        [SerializeField] private TextMeshProUGUI weaponTwoTitle;
        [SerializeField] private TextMeshProUGUI weaponTwoDMGNext;
        [SerializeField] private TextMeshProUGUI weaponTwoCooldownNext;
        [SerializeField] private TextMeshProUGUI weaponTwoAmountNext;

        [Header("- Item -")]
        [SerializeField] private ItemBlueprint itemBlueprint;
        [SerializeField] private Button selectItem;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemTitle;
        [SerializeField] private TextMeshProUGUI itemDescription;


        #region Unity
        private void Awake()
        {
            weaponOneButtonLevelUp.onClick.AddListener(() => LevelUpWeapon(swordBlueprint));
            weaponTwoButtonLevelUp.onClick.AddListener(() => LevelUpWeapon(bowBlueprint));
            selectItem.onClick.AddListener(() => ApplyItem());

            EventBus.Subscribe<OnPlayerLevelUp>(OnPlayerLevelUp);

            RefreshUI();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnPlayerLevelUp>(OnPlayerLevelUp);
        }
        #endregion

        private void OnPlayerLevelUp(OnPlayerLevelUp onPlayerLevelUp)
        {
            RefreshUI();
            canvasGroup.SetGroupState(true);
        }

        private void LevelUpWeapon(WeaponBlueprint weapon)
        {
            playerWeapon.LevelUpWeapon(weapon);
            canvasGroup.SetGroupState(false);
            RefreshUI();
        }

        //Está triste isso...
        private void ApplyItem()
        {
            playerItems.ApplyItem(itemBlueprint);
        }

        public void RefreshUI()
        {
            PopulateWeaponOption(
                swordBlueprint,
                weaponOneIcon,
                weaponOneTitle,
                weaponOneDMGNext,
                weaponOneCooldownNext,
                weaponOneAmountNext
            );

            PopulateWeaponOption(
                bowBlueprint,
                weaponTwoIcon,
                weaponTwoTitle,
                weaponTwoDMGNext,
                weaponTwoCooldownNext,
                weaponTwoAmountNext
            );

            PopulateItemOption(
                itemBlueprint,
                itemIcon,
                itemTitle,
                itemDescription
            );
        }

        private void PopulateWeaponOption(WeaponBlueprint blueprint, Image icon, TextMeshProUGUI title, TextMeshProUGUI dmgNext, TextMeshProUGUI cooldownNext, TextMeshProUGUI amountNext)
        {
            WeaponData currentData = playerWeapon.GetWeaponData(blueprint);

            if (currentData == null)
            {
                title.text = blueprint.WeaponName;
                icon.sprite = blueprint.Icon;
                dmgNext.text = "New: " + blueprint.BaseDamage.ToString();
                cooldownNext.text = "New: " + blueprint.BaseCooldown.ToString("F1");
                amountNext.text = "New: " + blueprint.BaseAmount.ToString();
            }
            else
            {
                int nextLevel = currentData.CurrentLevel + 1;
                //dmg, cooldown, amount
                (float, float, float)? nextLevelData = currentData.GetNextLevelData();

                if (nextLevelData != null)
                {
                    title.text = $"{blueprint.WeaponName} (Lv. {nextLevel})";
                    icon.sprite = blueprint.Icon;

                    dmgNext.text = $"DMG: {currentData.CurrentDamage} -> **{nextLevelData.Value.Item1}**";
                    cooldownNext.text = $"CD: {currentData.CurrentCooldown:F1}s -> **{nextLevelData.Value.Item2:F1}s**";
                    amountNext.text = $"AMOUNT: {currentData.CurrentAmount} -> **{nextLevelData.Value.Item3}**";
                }
                else
                {
                    title.text = $"{blueprint.WeaponName} (MAX)";
                    icon.sprite = blueprint.Icon;
                    dmgNext.text = "DMG: MAX";
                    cooldownNext.text = "CD: MAX";
                    amountNext.text = "AMOUNT: MAX";
                }
            }
        }

        private void PopulateItemOption(ItemBlueprint blueprint, Image icon, TextMeshProUGUI title, TextMeshProUGUI description)
        {
            title.text = blueprint.ItemName;
            icon.sprite = blueprint.ItemIcon;

            description.text = blueprint.ItemDescription;
        }
    }

}