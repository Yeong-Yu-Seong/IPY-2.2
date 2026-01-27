using System;
using System.Collections.Generic;

namespace BentoBoss.FirebaseManagers
{
    /// <summary>
    /// Simple result wrapper - tells if operation succeeded or failed
    /// </summary>
    public class FirebaseResult<T>
    {
        public bool Success;
        public string ErrorMessage;
        public T Data;

        public FirebaseResult(bool success, T data = default, string error = "")
        {
            Success = success;
            Data = data;
            ErrorMessage = error;
        }
    }

    /// <summary>
    /// Basic user data - just email for now
    /// Stored in database at: users/{userId}
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string userId;
        public string email;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { "userId", userId },
                { "email", email }
            };
        }
    }
}