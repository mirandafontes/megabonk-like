using UnityEngine.SceneManagement;
using Event;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI
{
    //Não queria construir o restart dessa maneira
    //Dando um loading na scene
    //Temos infraestrutura suficiente para dar um soft-reboot
    //em todos os mecanismos facilmente
    public class UIGameOverController : MonoBehaviour
    {
        [Header("- UI -")]
        [SerializeField] private Button restartGame;
        [SerializeField] private CanvasGroup canvasGroup;

        #region UNITY
        private void Awake()
        {
            EventBus.Subscribe<OnGameEnd>(_ => OnGameEnd());
            restartGame.onClick.AddListener(() => Restart());
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnGameEnd>(_ => OnGameEnd());
        }
        #endregion

        private void OnGameEnd()
        {
            canvasGroup.SetGroupState(true);
        }

        private void Restart()
        {
            canvasGroup.SetGroupState(false);
            SceneManager.LoadScene(0);
        }
    }
}