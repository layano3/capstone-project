using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Supabase;

public class LoginController : MonoBehaviour
{
    [Header("Refs")]
    public SupabaseConfig supabaseConfig;    // <- drag the ScriptableObject here
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public TMP_Text loginButtonLabel;
    public TMP_Text errorText;               // optional

    [Header("Flow")]
    public string nextSceneName = "MainMenu";
    public bool disableUntilFilled = true;

    bool _busy;
    string _accessToken;
    string _userId;

    void Awake()
    {
        if (errorText) errorText.gameObject.SetActive(false);

        if (disableUntilFilled)
        {
            usernameField.onValueChanged.AddListener(_ => ValidateReady());
            passwordField.onValueChanged.AddListener(_ => ValidateReady());
            ValidateReady();
        }
    }

    void ValidateReady()
    {
        bool ready = !string.IsNullOrWhiteSpace(usernameField.text)
                  && !string.IsNullOrWhiteSpace(passwordField.text);
        loginButton.interactable = ready && !_busy;
    }

    public void OnClickLogin()
    {
        if (_busy) return;
        if (string.IsNullOrWhiteSpace(usernameField.text) || string.IsNullOrWhiteSpace(passwordField.text))
        { ShowError("Enter email and passcode."); return; }

        StartCoroutine(LoginFlow());
    }

    IEnumerator LoginFlow()
    {
        AuthSession session = null; string err = null;
        yield return SupabaseAuth.SignIn(supabaseConfig,
                                         usernameField.text.Trim(),
                                         passwordField.text,
                                         (s, e) => { session = s; err = e; });

        if (err != null || session == null || string.IsNullOrEmpty(session.access_token))
        {
            ShowError("Arrr… login failed. " + (err ?? ""));
            _busy = false; SetInteractable(true); SetButton("Set Sail"); yield break;
        }

        _accessToken = session.access_token;
        _userId = SupabaseAuth.GetUserIdFromJwt(_accessToken);

        if (!string.IsNullOrEmpty(_userId))
        {
            PlayerPrefs.SetString("CurrentUserId", _userId);
            PlayerPrefs.Save();
            Debug.Log($"Logged in! User ID saved: {_userId}");
        }
        else
        {
            Debug.LogWarning("Could not extract user ID from token.");
        }

        SceneManager.LoadScene(nextSceneName);
    }

    void SetInteractable(bool v)
    {
        usernameField.interactable = v;
        passwordField.interactable = v;
        loginButton.interactable   = disableUntilFilled ? v && loginButton.interactable : v;
    }
    void SetButton(string s){ if (loginButtonLabel) loginButtonLabel.text = s; }
    void ShowError(string s){ if (errorText){ errorText.text = s; errorText.gameObject.SetActive(true);} }
}
