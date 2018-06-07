using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

using PassportLogin.AuthService;

namespace PassportLogin.Utils
{
    public static class MicrosoftPassportHelper
    {
        /// <summary>
        /// Checks to see if Passport is ready to be used.
        /// 
        /// Passport has dependencies on:
        ///     1. Having a connected Microsoft Account
        ///     2. Having a Windows PIN set up for that _account on the local machine
        /// </summary>
        public static async Task<bool> MicrosoftPassportAvailableCheckAsync()
        {
            bool keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();
            if (keyCredentialAvailable == false)
            {
                // Key credential is not enabled yet as user 
                // needs to connect to a Microsoft Account and select a PIN in the connecting flow.
                Debug.WriteLine("Microsoft Passport is not setup!\nPlease go to Windows Settings and set up a PIN to use it.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a Passport key on the machine using the _account id passed.
        /// </summary>
        /// <param name="accountId">The _account id associated with the _account that we are enrolling into Passport</param>
        /// <returns>Boolean representing if creating the Passport key succeeded</returns>
        public static async Task<bool> CreatePassportKeyAsync(Guid userId, string username)
        {
            Debug.WriteLine("Before calling KeyCredentialManager.RequestCreateAsync");

            KeyCredentialRetrievalResult keyCreationResult = await KeyCredentialManager.RequestCreateAsync(username, KeyCredentialCreationOption.ReplaceExisting);

            Debug.WriteLine("After calling KeyCredentialManager.RequestCreateAsync");

            switch (keyCreationResult.Status)
            {
                case KeyCredentialStatus.Success:
                    Debug.WriteLine("Successfully made key");

                    // In the real world authentication would take place on a server.
                    // So every time a user migrates or creates a new Microsoft Passport account Passport details should be pushed to the server.
                    // The details that would be pushed to the server include:
                    // The public key, keyAttesation if available, 
                    // certificate chain for attestation endorsement key if available,  
                    // status code of key attestation result: keyAttestationIncluded or 
                    // keyAttestationCanBeRetrievedLater and keyAttestationRetryType
                    // As this sample has no concept of a server it will be skipped for now
                    // for information on how to do this refer to the second Passport sample

                    //For this sample just return true
                    await GetKeyAttestationAsync(userId, keyCreationResult);
                    return true;
                case KeyCredentialStatus.UserCanceled:
                    Debug.WriteLine("User cancelled sign-in process.");
                    break;
                case KeyCredentialStatus.NotFound:
                    // User needs to setup Microsoft Passport
                    Debug.WriteLine("Microsoft Passport is not setup!\nPlease go to Windows Settings and set up a PIN to use it.");
                    break;
                case KeyCredentialStatus.CredentialAlreadyExists:
                    Debug.WriteLine("Credential already exists!");
                    break;
                case KeyCredentialStatus.UnknownError:
                    Debug.WriteLine("Unknown error generating credentials key");
                    break;
                default:
                    Debug.WriteLine("Some error making the credentials key!");
                    break;
            }

            return false;
        }

        /// <summary>
        /// Function to be called when user requests deleting their account.
        /// Checks the KeyCredentialManager to see if there is a Passport for the current user
        /// Then deletes the local key associated with the Passport.
        /// </summary>
        public static async void RemovePassportAccountAsync(UserAccount account)
        {
            // Open the account with Passport
            KeyCredentialRetrievalResult keyOpenResult = await KeyCredentialManager.OpenAsync(account.Username);

            if (keyOpenResult.Status == KeyCredentialStatus.Success)
            {
                // In the real world you would send key information to server to unregister
                //e.g. RemovePassportAccountOnServer(account);
                AuthService.AuthService.Instance.PassportRemoveUser(account.UserId);
            }

            // Then delete the account from the machines list of Passport Accounts
            await KeyCredentialManager.DeleteAsync(account.Username);
        }

        public static void RemovePassportDevice(UserAccount account, Guid deviceId)
        {
            AuthService.AuthService.Instance.PassportRemoveDevice(account.UserId, deviceId);
        }

        /// <summary>
        /// Attempts to sign a message using the Passport key on the system for the accountId passed.
        /// </summary>
        /// <returns>Boolean representing if creating the Passport authentication message succeeded</returns>
        public static async Task<bool> GetPassportAuthenticationMessageAsync(UserAccount account)
        {
            KeyCredentialRetrievalResult openKeyResult = await KeyCredentialManager.OpenAsync(account.Username);
            // Calling OpenAsync will allow the user access to what is available in the app and will not require user credentials again.
            // If you wanted to force the user to sign in again you can use the following:
            // var consentResult = await Windows.Security.Credentials.UI.UserConsentVerifier.RequestVerificationAsync(account.Username);
            // This will ask for the either the password of the currently signed in Microsoft Account or the PIN used for Microsoft Passport.

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                // If OpenAsync has succeeded, the next thing to think about is whether the client application requires access to backend services.
                // If it does here you would Request a challenge from the Server. The client would sign this challenge and the server
                // would check the signed challenge. If it is correct it would allow the user access to the backend.
                // You would likely make a new method called RequestSignAsync to handle all this
                // e.g. RequestSignAsync(openKeyResult);
                // Refer to the second Microsoft Passport sample for information on how to do this.

                return await RequestSignAsync(account.UserId, openKeyResult);
            }
            else if (openKeyResult.Status == KeyCredentialStatus.NotFound)
            {
                // If the _account is not found at this stage. It could be one of two errors. 
                // 1. Microsoft Passport has been disabled
                // 2. Microsoft Passport has been disabled and re-enabled cause the Microsoft Passport Key to change.
                // Calling CreatePassportKey and passing through the account will attempt to replace the existing Microsoft Passport Key for that account.
                // If the error really is that Microsoft Passport is disabled then the CreatePassportKey method will output that error.
                if (await CreatePassportKeyAsync(account.UserId, account.Username))
                {
                    // If the Passport Key was again successfully created, Microsoft Passport has just been reset.
                    // Now that the Passport Key has been reset for the _account retry sign in.
                    return await GetPassportAuthenticationMessageAsync(account);
                }
            }

            // Can't use Passport right now, try again later
            return false;
        }

        private static async Task GetKeyAttestationAsync(Guid userId, KeyCredentialRetrievalResult keyCreationResult)
        {
            KeyCredential userKey = keyCreationResult.Credential;
            IBuffer publicKey = userKey.RetrievePublicKey();
            KeyCredentialAttestationResult keyAttestationResult = await userKey.GetAttestationAsync();
            IBuffer keyAttestation = null;
            IBuffer certificateChain = null;
            bool keyAttestationIncluded = false;
            bool keyAttestationCanBeRetrievedLater = false;
            KeyCredentialAttestationStatus keyAttestationRetryType = 0;

            if (keyAttestationResult.Status == KeyCredentialAttestationStatus.Success)
            {
                keyAttestationIncluded = true;
                keyAttestation = keyAttestationResult.AttestationBuffer;
                certificateChain = keyAttestationResult.CertificateChainBuffer;
                Debug.WriteLine("Successfully made key and attestation");
            }
            else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.TemporaryFailure)
            {
                keyAttestationRetryType = KeyCredentialAttestationStatus.TemporaryFailure;
                keyAttestationCanBeRetrievedLater = true;
                Debug.WriteLine("Successfully made key but not attestation");
            }
            else if (keyAttestationResult.Status == KeyCredentialAttestationStatus.NotSupported)
            {
                keyAttestationRetryType = KeyCredentialAttestationStatus.NotSupported;
                keyAttestationCanBeRetrievedLater = false;
                Debug.WriteLine("Key created, but key attestation not supported");
            }

            Guid deviceId = Helpers.GetDeviceId();
            //Update the Pasport details with the information we have just gotten above.
            UpdatePassportDetails(userId, deviceId, publicKey.ToArray(), keyAttestationResult);
        }

        public static bool UpdatePassportDetails(Guid userId, Guid deviceId, byte[] publicKey, KeyCredentialAttestationResult keyAttestationResult)
        {
            //In the real world you would use an API to add Passport signing info to server for the signed in _account.
            //For this tutorial we do not implement a WebAPI for our server and simply mock the server locally 
            //The CreatePassportKey method handles adding the Windows Hello account locally to the device using the KeyCredential Manager

            //Using the userId the existing account should be found and updated.
            AuthService.AuthService.Instance.PassportUpdateDetails(userId, deviceId, publicKey, keyAttestationResult);
            return true;
        }

        private static async Task<bool> RequestSignAsync(Guid userId, KeyCredentialRetrievalResult openKeyResult)
        {
            // Calling userKey.RequestSignAsync() prompts the uses to enter the PIN or use Biometrics (Windows Hello).
            // The app would use the private key from the user account to sign the sign-in request (challenge)
            // The client would then send it back to the server and await the servers response.
            IBuffer challengeMessage = AuthService.AuthService.Instance.PassportRequestChallenge();
            KeyCredential userKey = openKeyResult.Credential;
            KeyCredentialOperationResult signResult = await userKey.RequestSignAsync(challengeMessage);

            if (signResult.Status == KeyCredentialStatus.Success)
            {
                // If the challenge from the server is signed successfully
                // send the signed challenge back to the server and await the servers response
                return AuthService.AuthService.Instance.SendServerSignedChallenge(
                    userId, Helpers.GetDeviceId(), signResult.Result.ToArray());
            }
            else if (signResult.Status == KeyCredentialStatus.UserCanceled)
            {
                // User cancelled the Windows Hello PIN entry.
            }
            else if (signResult.Status == KeyCredentialStatus.NotFound)
            {
                // Must recreate Windows Hello key
            }
            else if (signResult.Status == KeyCredentialStatus.SecurityDeviceLocked)
            {
                // Can't use Windows Hello right now, remember that hardware failed and suggest restart
            }
            else if (signResult.Status == KeyCredentialStatus.UnknownError)
            {
                // Can't use Windows Hello right now, try again later
            }

            return false;
        }

    }
}
