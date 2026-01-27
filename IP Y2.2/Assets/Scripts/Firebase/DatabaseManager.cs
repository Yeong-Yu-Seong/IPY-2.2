using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;

namespace BentoBoss.FirebaseManagers
{
    /// <summary>
    /// Handles database operations - saves user email when they register
    /// Database path: users/{userId}/email
    /// </summary>
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        private DatabaseReference _database;

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
                var firebaseDb = global::Firebase.Database.FirebaseDatabase.DefaultInstance;
                _database = firebaseDb.RootReference;
                Debug.Log("[Database] Ready");
            };
        }

        /// <summary>
        /// Save user email to database after registration
        /// Path: users/{userId}
        /// </summary>
        public async Task<FirebaseResult<bool>> SaveUserEmail(string userId, string email)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return new FirebaseResult<bool>(false, false, "Invalid userId or email");

            try
            {
                UserData userData = new UserData
                {
                    userId = userId,
                    email = email
                };

                await _database.Child("users").Child(userId).SetValueAsync(userData.ToDictionary());
                
                Debug.Log($"[Database] User email saved: {email}");
                return new FirebaseResult<bool>(true, true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Database] Save failed: {ex.Message}");
                return new FirebaseResult<bool>(false, false, ex.Message);
            }
        }
    }
}