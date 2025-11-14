using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI
{
    public class UIStartGameController : MonoBehaviour
    {
        [Header("- UI -")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button startButton;

        #region Unity
        private void Awake()
        {
            startButton.onClick.AddListener(StartGame);
        }
        #endregion

        private void StartGame()
        {
            canvasGroup.SetGroupState(false);
        }
    }
}