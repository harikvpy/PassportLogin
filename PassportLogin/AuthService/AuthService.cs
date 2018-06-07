using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassportLogin.AuthService
{
    public class AuthService
    {
        //Singleton instance of the AuthService
        //The AuthService is a mock of what a real world server and database implementation would be
        private static AuthService _instance;
        public static AuthService Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new AuthService();
                }
                return _instance;
            }
        }

        private MockStore _mockStore = new MockStore();

        public Guid GetUserId(string username)
        {
            return _mockStore.GetUserId(username);
        }

        public UserAccount GetUserAccount(Guid userId)
        {
            return _mockStore.GetUserAccount(userId);
        }

        public List<UserAccount> GetUserAccountsForDevice(Guid deviceId)
        {
            return _mockStore.GetUserAccountsForDevice(deviceId);
        }

        public void Register(string username)
        {
            _mockStore.AddAccount(username);
        }

        public bool PassportRemoveUser(Guid userId)
        {
            return _mockStore.RemoveAccount(userId);
        }

        public bool PassportRemoveDevice(Guid userId, Guid deviceId)
        {
            return _mockStore.RemoveDevice(userId, deviceId);
        }

        public void PassportUpdateDetails(Guid userId, Guid deviceId, byte[] publicKey,
            KeyCredentialAttestationResult keyAttestationResult)
        {
            _mockStore.PassportUpdateDetails(userId, deviceId, publicKey, keyAttestationResult);
        }

        public bool ValidateCredentials(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // This would be used for existing accounts migrating to use Passport
                Guid userId = GetUserId(username);
                if (userId != Guid.Empty)
                {
                    UserAccount account = GetUserAccount(userId);
                    if (account != null)
                    {
                        if (string.Equals(password, account.Password))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public IBuffer PassportRequestChallenge()
        {
            return CryptographicBuffer.ConvertStringToBinary("ServerChallenge", BinaryStringEncoding.Utf8);
        }

        public bool SendServerSignedChallenge(Guid userId, Guid deviceId, byte[] signedChallenge)
        {
            // Depending on your company polices and procedures this step will be different
            // It is at this point you will need to validate the signedChallenge that is sent back from the client.
            // Validation is used to ensure the correct user is trying to access this account. 
            // The validation process will use the signedChallenge and the stored PublicKey 
            // for the username and the specific device signin is called from.
            // Based on the validation result you will return a bool value to allow access to continue or to block the account.

            // For this sample validation will not happen as a best practice solution does not apply and will need to 
            // be configured for each company.
            // Simply just return true.

            // You could get the User's Public Key with something similar to the following:
            byte[] userPublicKey = _mockStore.GetPublicKey(userId, deviceId);
            return true;
        }
    }
}
