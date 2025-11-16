#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public class CurvePopulatorEditor : EditorWindow
{
    // Campos para configurar a curva
    private AnimationCurve targetCurve = new AnimationCurve();
    private int numberOfLevels = 10;
    private float initialValue = 0;
    private float xpIncreasePerLevel = 100f;
    private bool cumulativeMode = true;

    [MenuItem("Tools/Curve Populator")]
    public static void ShowWindow()
    {
        GetWindow<CurvePopulatorEditor>("Curve Populator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configurações do Preenchedor de Curvas", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Curva Alvo:");
        targetCurve = EditorGUILayout.CurveField(targetCurve);

        EditorGUILayout.Space(10);

        numberOfLevels = EditorGUILayout.IntSlider("Número de Níveis", numberOfLevels, 2, 100);
        xpIncreasePerLevel = EditorGUILayout.FloatField("Aumento Fixo por Nível", xpIncreasePerLevel);
        initialValue = EditorGUILayout.FloatField("Valor inicial", initialValue);

        cumulativeMode = EditorGUILayout.Toggle(
            new GUIContent("Modo Acumulativo", "Se ativado, cada ponto de controle será a soma dos aumentos anteriores."),
            cumulativeMode
        );

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Gerar Curva Incremental"))
        {
            GenerateCurve();
        }

        EditorGUILayout.HelpBox(
            "Use o modo Acumulativo para Curvas de XP Total (Nível 1 = 0). Use o modo não-acumulativo para curvas de Dano por Nível.",
            MessageType.Info
        );
    }

    private void GenerateCurve()
    {
        targetCurve.keys = new Keyframe[0];
        float accumulatedValue = initialValue;

        targetCurve.AddKey(1f, initialValue);

        for (int level = 2; level <= numberOfLevels; level++)
        {
            float currentValue;

            if (cumulativeMode)
            {
                accumulatedValue += xpIncreasePerLevel;
                currentValue = accumulatedValue;
            }
            else
            {
                currentValue = initialValue + xpIncreasePerLevel * (level - 1);
            }

            targetCurve.AddKey(level, currentValue);
        }

        for (int i = 0; i < targetCurve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(targetCurve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(targetCurve, i, AnimationUtility.TangentMode.Linear);
        }

    }
}
#endif