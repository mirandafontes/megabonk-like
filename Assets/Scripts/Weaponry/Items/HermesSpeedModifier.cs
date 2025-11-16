using UnityEngine;
using Player;

namespace Weaponry.Items
{
    //Classe POCO + campos para scriptable
    //A classe POCO permite a criação de algoritmos específicos
    //ao mesmo tempo que o uso congregado com o SO
    //permite rápidas alterações e balanceamentos em parametros.
    //Ou seja, ao termos um item mais complexo, com mais valores
    //para configurar, o algortimo de atuação dele é todo encapsulado no POCO,
    //mas as variáveis que alteram seu comportamento são todas disponíveis via [SerializeField]
    //para o SO, que é criado derivando do ItemBlueprint
    [System.Serializable]
    public class HermesSpeedModifier : IItemModifier
    {
        [SerializeField] private float modifierValue = 0.1f;
        private PlayerStats _playerStats;
        public float ModifierValue { get; private set; }

        //Postergamos a inicialização para o uso
        public void Initialize(PlayerStats stats)
        {
            _playerStats = stats;
            ModifierValue = modifierValue;
        }

        public void Apply()
        {
            _playerStats?.AddSpeedModifier(ModifierValue);
        }
        
        public void Remove()
        {
            _playerStats?.RemoveSpeedModifier(ModifierValue);
        }
    }
}