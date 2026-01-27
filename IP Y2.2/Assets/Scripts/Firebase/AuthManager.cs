using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Auth;

namespace BentoBoss.FirebaseManagers
{
    /// <summary>
    /// Handles user registration and login
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        private global::Firebase.Auth.FirebaseAuth _auth;
        public FirebaseUser CurrentUser => _auth?.CurrentUser;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            FirebaseManager.Instance.OnFirebaseReady += () =>
            {
                _auth = global::Firebase.Auth.FirebaseAuth.DefaultInstance;
                Debug.Log("[Auth] Ready");
            };
        }

        /// <summary>
        /// Register new user with email and password
        /// Minimum 6 characters for password (Firebase requirement)
        /// </summary>
        public async Task<FirebaseResult<FirebaseUser>> Register(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return new FirebaseResult<FirebaseUser>(false, null, "Email/password cannot be empty");

            if (password.Length < 6)
                return new FirebaseResult<FirebaseUser>(false, null, "Password must be at least 6 characters");

            try
            {
                var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password);
                Debug.Log($"[Auth] User registered: {result.User.Email}");
                return new FirebaseResult<FirebaseUser>(true, result.User);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Register failed: {ex.Message}");
                return new FirebaseResult<FirebaseUser>(false, null, ex.Message);
            }
        }

        /// <summary>
        /// Login existing user
        /// </summary>
        public async Task<FirebaseResult<FirebaseUser>> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return new FirebaseResult<FirebaseUser>(false, null, "Email/password cannot be empty");

            try
            {
                var result = await _auth.SignInWithEmailAndPasswordAsync(email, password);
                Debug.Log($"[Auth] User logged in: {result.User.Email}");
                return new FirebaseResult<FirebaseUser>(true, result.User);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Login failed: {ex.Message}");
                return new FirebaseResult<FirebaseUser>(false, null, ex.Message);
            }
        }

        public void SignOut()
        {
            _auth?.SignOut();
            Debug.Log("[Auth] User signed out");
        }
    }
}