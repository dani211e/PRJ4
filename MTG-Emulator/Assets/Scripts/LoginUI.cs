using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class LoginUI : MonoBehaviour
    {

        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private TMP_InputField passwordField;


        public void OnClickLogin()
        {
            string email = usernameField.text.Trim();
            string password = passwordField.text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                UIPopup.Instance.Show("Username or password is empty.");
                return;
            }

            if (APIManager.Instance == null)
            {
                UIPopup.Instance.Show("Connection error. Please try again.");
                return;
            }

            StartCoroutine(APIManager.Instance.Login(
                email,
                password,
                successMessage =>
                {
                    StartCoroutine(APIManager.Instance.GetPlayerProfile(
                        player => SceneManager.LoadScene("0"),
                        error =>
                        {
                            if (error.Contains("404") || error.Contains("Not Found"))
                            {
                                UIPopup.Instance.Show("Account no longer exists.");
                                PlayerPrefs.DeleteAll();
                                PlayerPrefs.Save();
                            }
                        }
                    ));
                },
                error => UIPopup.Instance.Show("Login failed: " + error)
            ));
        }
        
        public void OnClickRegister()
        {
            SceneManager.LoadScene("Register");
        }
    }

