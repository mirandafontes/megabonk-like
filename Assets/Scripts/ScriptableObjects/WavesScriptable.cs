using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wave;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Waves", menuName = "Data/Waves setup")]
    public class WavesScriptable : ScriptableObject
    {
        //Short-cut para indexação
        public WaveSetup this[int index]
        {
            get
            {
                if (index >= 0 && index < Waves.Count)
                {
                    return Waves[index];
                }

                return null;
            }
        }

        public List<WaveSetup> Waves;

        public int WavesTotalTime
        {
            get
            {
                if (Waves == null || Waves.Count <= 0)
                {
                    return 0;
                }

                return Waves.Sum(group => group.TotalTime);
            }
        }
    }
}