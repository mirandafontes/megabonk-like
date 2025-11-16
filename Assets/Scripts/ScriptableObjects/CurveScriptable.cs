using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "CurveScriptable", menuName = "Data/Simple curve")]
    public class CurveScriptable : ScriptableObject
    {
        [Header("- Label -")]
        [SerializeField]
        [Tooltip("Descrição do eixo X")]
        private string XAxis;

        [SerializeField]
        [Tooltip("Descrição do eixo Y")]
        private string YAxis;

        [Header("- Data -")]
        public AnimationCurve Curve;

        /// <summary>
        /// Retorna (x, y), respectivamente (time, value) da curva.
        /// </summary>
        public (float, float) GetLastPoint()
        {
            if (Curve.length <= 0)
            {
                return (0, 0);
            }

            int totalKeys = Curve.length;

            Keyframe lastKey = Curve.keys[totalKeys - 1];

            float lastTime = lastKey.time;
            float lastValue = lastKey.value;

            return (lastTime, lastValue);
        }

        public float GetValueAtLevel(float level)
        {
            float maxLevel = GetLastPoint().Item1;

            float clampedLevel = Mathf.Clamp(level, 1, maxLevel);
            return Curve.Evaluate(clampedLevel);
        }
    }
}