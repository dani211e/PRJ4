using UnityEngine;
using TMPro;

public class UIPopup : MonoBehaviour
{
    public static UIPopup Instance;

    public GameObject popupPanel;
    public TMP_Text messageText;

    private void Awake()
    {
        Debug.Log("UIPopup Awake called. Instance is null: " + (Instance == null));
    
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    
        Debug.Log("UIPopup Instance set: " + (Instance != null));
        popupPanel.SetActive(false);
    }

    public void Show(string message)
    {
        Debug.Log("UIPopup.Show called with: " + message);
        messageText.text = message;
        popupPanel.SetActive(true);
    }

    public void Close()
    {
        popupPanel.SetActive(false);
    }
}