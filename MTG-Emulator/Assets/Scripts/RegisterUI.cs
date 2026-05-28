using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;

public class RegisterUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;

    public void OnClickRegister()
    {
        string username = usernameField.text.Trim();
        string email = emailField.text.Trim();
        string password = passwordField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("All fields are required.");
            return;
        }

        RegisterDto dto = new RegisterDto
        {
            Username = username,
            Email = email,
            Password = password,
            Role = ""
        };

        StartCoroutine(APIManager.Instance.Register(
            dto,
            result =>
            {
                SceneManager.LoadScene("Login screen");
            },
            error => UIPopup.Instance.Show("Register failed: " + error)
        ));
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("Login screen");
    }
}