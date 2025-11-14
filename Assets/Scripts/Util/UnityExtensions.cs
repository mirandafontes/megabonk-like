using UnityEngine;

namespace Util
{
    public static class UnityExtensions
    {
        public static void SetGroupState(this CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.alpha = state ? 1f : 0f;
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }
    }
}