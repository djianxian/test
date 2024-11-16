using UnityEngine;

namespace TestGame.Utils
{
    public static class UIUtil
    {
        public static void SetButtonAction(UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
        
        public static  bool AreVectorsEqual(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }
    }
}