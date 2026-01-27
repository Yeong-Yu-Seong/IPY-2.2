using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BentoBoss.FirebaseManagers;
using UnityEngine.SceneManagement;

public class AuthUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject forgotPasswordPanel;

    [Header("Login Panel")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI loginErrorText;
    [SerializeField] private TextMeshProUGUI goToSignUpText;
    [SerializeField] private TextMeshProUGUI forgotPasswordText;
    
    [Header("Sign Up Panel")]
    [SerializeField] private TMP_InputField signUpEmailInput;
    [SerializeField] private TMP_InputField signUpPasswordInput;
    [SerializeField] private Button signUpButton;
    [SerializeField] private TextMeshProUGUI signUpErrorText;
    [SerializeField] private TextMeshProUGUI goToLoginText;
    
    [Header("Forgot Password Panel")]
    [SerializeField] private TMP_InputField forgotPasswordEmailInput;
    [SerializeField] private Button sendResetButton;
    [SerializeField] private TextMeshProUGUI forgotPasswordErrorText;
    [SerializeField] private TextMeshProUGUI backToLoginText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    void Awake()
    {
        // Check critical assignments
        if (loginPanel == null) Debug.LogError("[AuthUI] LoginPanel not assigned!");
        if (signUpPanel == null) Debug.LogError("[AuthUI] SignUpPanel not assigned!");
        if (forgotPasswordPanel == null) Debug.LogWarning("[AuthUI] ForgotPasswordPanel not assigned!");
    }

    void Start()
    {
        // Setup main buttons
        loginButton.onClick.AddListener(OnLoginClicked);
        signUpButton.onClick.AddListener(OnSignUpClicked);
        if (sendResetButton != null) sendResetButton.onClick.AddListener(OnResetClicked);

        // Make text clickable by adding Button component automatically
        MakeTextClickableWithButton(goToSignUpText, ShowSignUpPanel);
        MakeTextClickableWithButton(goToLoginText, ShowLoginPanel);
        MakeTextClickableWithButton(forgotPasswordText, ShowForgotPasswordPanel);
        MakeTextClickableWithButton(backToLoginText, ShowLoginPanel);

        // Force show login panel
        ShowLoginPanel();
        Debug.Log("[AuthUI] Started - Login panel active");
    }

    /// <summary>
    /// Makes text clickable by adding Button component instead of EventTrigger
    /// This is more reliable and works better with Unity's UI system
    /// </summary>
    void MakeTextClickableWithButton(TextMeshProUGUI text, UnityEngine.Events.UnityAction action)
    {
        if (text == null)
        {
            Debug.LogWarning("[AuthUI] Text is null, cannot make clickable");
            return;
        }

        // Get or add Button component
        Button button = text.GetComponent<Button>();
        if (button == null)
        {
            button = text.gameObject.AddComponent<Button>();
            Debug.Log($"[AuthUI] Added Button to '{text.gameObject.name}'");
        }

        // Clear old listeners and add new one
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        // Make button transparent (we only see the text)
        button.transition = Selectable.Transition.None;

        Debug.Log($"[AuthUI] Made '{text.gameObject.name}' clickable");
    }

    void ShowLoginPanel()
    {
        Debug.Log("[AuthUI] Showing Login Panel");
        
        if (loginPanel != null) loginPanel.SetActive(true);
        if (signUpPanel != null) signUpPanel.SetActive(false);
        if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(false);
        
        ClearAll();
    }

    void ShowSignUpPanel()
    {
        Debug.Log("[AuthUI] Showing Sign Up Panel");
        
        if (loginPanel != null) loginPanel.SetActive(false);
        if (signUpPanel != null) signUpPanel.SetActive(true);
        if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(false);
        
        ClearAll();
    }

    void ShowForgotPasswordPanel()
    {
        Debug.Log("[AuthUI] Showing Forgot Password Panel");
        
        if (forgotPasswordPanel == null)
        {
            Debug.LogWarning("[AuthUI] Forgot Password Panel not assigned!");
            return;
        }
        
        if (loginPanel != null) loginPanel.SetActive(false);
        if (signUpPanel != null) signUpPanel.SetActive(false);
        forgotPasswordPanel.SetActive(true);
        
        ClearAll();
    }

    async void OnLoginClicked()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (!ValidateInput(email, password, loginErrorText)) return;

        loginErrorText.text = "Logging in...";
        loginButton.interactable = false;

        var result = await AuthManager.Instance.Login(email, password);

        if (result.Success)
        {
            loginErrorText.text = "Success!";
            await System.Threading.Tasks.Task.Delay(300);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            loginErrorText.text = SimplifyError(result.ErrorMessage);
            loginButton.interactable = true;
        }
    }

    async void OnSignUpClicked()
    {
        string email = signUpEmailInput.text.Trim();
        string password = signUpPasswordInput.text;

        if (!ValidateInput(email, password, signUpErrorText)) return;

        signUpErrorText.text = "Creating account...";
        signUpButton.interactable = false;

        var result = await AuthManager.Instance.Register(email, password);

        if (result.Success)
        {
            signUpErrorText.text = "Saving...";
            await DatabaseManager.Instance.SaveUserEmail(result.Data.UserId, result.Data.Email);
            
            signUpErrorText.text = "Success!";
            await System.Threading.Tasks.Task.Delay(300);
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            signUpErrorText.text = SimplifyError(result.ErrorMessage);
            signUpButton.interactable = true;
        }
    }

    async void OnResetClicked()
    {
        if (forgotPasswordEmailInput == null)
        {
            Debug.LogError("[AuthUI] Forgot password email input not assigned!");
            return;
        }

        string email = forgotPasswordEmailInput.text.Trim();

        if (!ValidateEmail(email, forgotPasswordErrorText)) return;

        forgotPasswordErrorText.text = "Sending...";
        sendResetButton.interactable = false;

        try
        {
            await global::Firebase.Auth.FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email);
            forgotPasswordErrorText.text = "Email sent! Check inbox.";
            await System.Threading.Tasks.Task.Delay(2000);
            ShowLoginPanel();
        }
        catch (System.Exception ex)
        {
            forgotPasswordErrorText.text = SimplifyError(ex.Message);
        }
        finally
        {
            sendResetButton.interactable = true;
        }
    }

    bool ValidateInput(string email, string password, TextMeshProUGUI errorText)
    {
        if (!ValidateEmail(email, errorText)) return false;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorText.text = "Enter password";
            return false;
        }

        if (password.Length < 6)
        {
            errorText.text = "Password too short (min 6)";
            return false;
        }

        return true;
    }

    bool ValidateEmail(string email, TextMeshProUGUI errorText)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            errorText.text = "Enter email";
            return false;
        }

        if (!email.Contains("@"))
        {
            errorText.text = "Invalid email (missing @)";
            return false;
        }

        int atIndex = email.IndexOf("@");
        if (!email.Substring(atIndex).Contains("."))
        {
            errorText.text = "Invalid email (missing domain)";
            return false;
        }

        return true;
    }

    string SimplifyError(string error)
    {
        if (string.IsNullOrEmpty(error)) return "Error occurred";

        string e = error.ToLower();

        if (e.Contains("already") || e.Contains("in use")) return "Email already registered";
        if (e.Contains("user not found") || e.Contains("no user")) return "No account found";
        if (e.Contains("wrong password")) return "Wrong password";
        if (e.Contains("invalid email")) return "Invalid email";
        if (e.Contains("weak password")) return "Password too weak";
        if (e.Contains("network")) return "Connection error";
        if (e.Contains("too many")) return "Too many attempts";

        return "Error occurred";
    }

    void ClearAll()
    {
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (signUpEmailInput != null) signUpEmailInput.text = "";
        if (signUpPasswordInput != null) signUpPasswordInput.text = "";
        if (forgotPasswordEmailInput != null) forgotPasswordEmailInput.text = "";
        
        if (loginErrorText != null) loginErrorText.text = "";
        if (signUpErrorText != null) signUpErrorText.text = "";
        if (forgotPasswordErrorText != null) forgotPasswordErrorText.text = "";
    }

    void OnDestroy()
    {
        if (loginButton != null) loginButton.onClick.RemoveAllListeners();
        if (signUpButton != null) signUpButton.onClick.RemoveAllListeners();
        if (sendResetButton != null) sendResetButton.onClick.RemoveAllListeners();
    }
}