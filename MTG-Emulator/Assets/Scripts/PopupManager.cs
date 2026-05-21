using UnityEngine;

namespace DefaultNamespace
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPopup;
        [SerializeField] private GameObject gameResultPopup;
        
        
        public void CloseAllPopups()
        {
            if (settingsPopup != null)
                settingsPopup.SetActive(false);

            if (gameResultPopup != null)
                gameResultPopup.SetActive(false);

        }
        
        public void OpenSettingsPopup()
        {
            if (settingsPopup != null)
                settingsPopup.SetActive(true);
        }
        
        public void OpenGameResultPopup()
        {
            if (gameResultPopup != null)
                gameResultPopup.SetActive(true);
        }
        
    }
}