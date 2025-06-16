using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EmailPassLogin : MonoBehaviour
{
  

    #region variables

    [Header("Login")]
    public TMP_InputField LoginEmail;
    public TMP_InputField LoginPassword;
    
    [Header("Register")]
    public TMP_InputField SignUpEmail;
    public TMP_InputField SignUpPassword;
    public TMP_InputField SignUpConfirmPassword;

    [Header("Extra")]
    public GameObject loadingScreen;
    public TextMeshProUGUI logText;
    public GameObject loginUi, signUpUi, successUi;
    
    #endregion

    #region signup

    public void SignUp()
    {
       

        loadingScreen.SetActive(true);
        
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = SignUpEmail.text;
        string password = SignUpPassword.text;
        
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an exception: " + task.Exception);
                return;
            }
            // Firebase user has been created.
            
            loadingScreen.SetActive(false); FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            // AuthResult result = task.Result;
            Debug.Log("Firebase user created successfully" + user.DisplayName + user.UserId);
            
            SignUpEmail.text = "";
            SignUpPassword.text = "";
            SignUpConfirmPassword.text = "";

            if (user.IsEmailVerified)
            {
               showLogMsg("Email verified");
            }
            else
            {
                showLogMsg("Verification mail has been sent");
                SendEmailVerification();
            }
        });
    }



    public void SendEmailVerification()
    {
        Debug.Log("1");
        StartCoroutine(SendEmailForVerificationAsync());
    }

    IEnumerator SendEmailForVerificationAsync()
    {
        Debug.Log("2");
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
    
        if (user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                Debug.LogWarning("Failed to send verification email.");
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;

                if (firebaseException != null)
                {
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    Debug.LogError($"Error Code: {error}, Message: {firebaseException.Message}");

                    switch (error)
                    {
                        case AuthError.InvalidEmail:
                            Debug.LogError("Invalid email format.");
                            break;
                        case AuthError.UserNotFound:
                            Debug.LogError("User not found.");
                            break;
                        case AuthError.NetworkRequestFailed:
                            Debug.LogError("Network error. Check your internet connection.");
                            break;
                        case AuthError.TooManyRequests:
                            Debug.LogError("Too many requests. Try again later.");
                            break;
                        default:
                            Debug.LogError("An unknown error occurred.");
                            break;
                    }
                }
            }
            else
            {
                Debug.Log($"Verification email sent to: {user.Email}");
            }
        }
        else
        {
            Debug.LogWarning("No user is currently signed in.");
        }
    }

    
    #endregion
    
    #region login

    public void Login()
    {
        loadingScreen.SetActive(true);
        
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = LoginEmail.text;
        string password = LoginPassword.text;
        
        Credential credential= EmailAuthProvider.GetCredential(email, password);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an exception: " + task.Exception);
                return;
            }
            loadingScreen.SetActive(false);
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            
            if (result.User.IsEmailVerified)
            { 
                showLogMsg("Log in successful ");
                // loginUi.SetActive(false);
                GameManager.Instance.LoadScenarioScene();
                // successUi.SetActive(true);
                // successUi.transform.Find("Desc").GetComponent<TextMeshProUGUI>().text ="Id: " + result.User.UserId;
            }
            else
            {
                showLogMsg("Email not verified:");
               
            }
        });
    }
    
    #endregion

    #region extra

  public  void AlreadyHaveAcc()
    {
        signUpUi.SetActive(false);
        loginUi.SetActive(true);
    }
   public void DontHaveAcc()
    {      signUpUi.SetActive(true);
        loginUi.SetActive(false); 
  
    }
    void showLogMsg(string msg)
    {
        logText.text = msg;
        
    }

    #endregion
    
}
