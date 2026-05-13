using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class LoginUI : MonoBehaviour
    {

        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField passwordField;


        public void OnClickLogin()
        {
            string username = usernameField.text.Trim();
            string password = passwordField.text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Debug.Log("Username or password is empty");
                return;
            }

            if (APIManager.Instance == null)
            {
                Debug.Log("API Manager instance is null");
                return;
            }

            StartCoroutine(APIManager.Instance.CreateProfile(
                username,
                password,
                SuccessMessage =>
                {
                    Debug.Log(SuccessMessage);
                },
                error =>
                {
                    Debug.LogError(error);
                }
                ));
        }
    }

