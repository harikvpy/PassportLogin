using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassportLogin.AuthService
{
    public class PassportDevice
    {
        // These are the new variables that will need to be added to the existing UserAccount in the Database
        // The DeviceName is used to support multiple devices for the one user.
        // This way the correct public key is easier to find as a new public key is made for each device.
        // The KeyAttestationResult is only used if the User device has a TPM (Trusted Platform Module) chip, 
        // in most cases it will not. So will be left out for this hands on lab.
        public Guid DeviceId { get; set; }
        public byte[] PublicKey { get; set; }
        // public KeyCredentialAttestationResult KeyAttestationResult { get; set; }
    }
}
