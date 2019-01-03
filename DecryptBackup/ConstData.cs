using System.Collections.Generic;

namespace DecryptBackup
{
    public class ConstData
    {
        public static readonly IList<string> CLASSKEY_TAGS
            = new string[5] { "CLAS", "WRAP", "WPKY", "KTYP", "PBKY" };//UUID
        public static readonly IList<string> KEYBAG_TYPES
            = new string[] { "System", "Backup", "Escrow", "OTA (icloud)" };
        public static readonly IList<string> KEY_TYPES = new string[] { "AES", "Curve25519" };
        public static readonly Dictionary<int, string> PROTECTION_CLASSES
            = new Dictionary<int, string>()
            {
                [1] = "NSFileProtectionComplete",
                [2] = "NSFileProtectionCompleteUnlessOpen",
                [3] = "NSFileProtectionCompleteUntilFirstUserAuthentication",
                [4] = "NSFileProtectionNone",
                [5] = "NSFileProtectionRecovery?",
                [6] = "kSecAttrAccessibleWhenUnlocked",
                [7] = "kSecAttrAccessibleAfterFirstUnlock",
                [8] = "kSecAttrAccessibleAlways",
                [9] = "kSecAttrAccessibleWhenUnlockedThisDeviceOnly",
                [10] = "kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly",
                [11] = "kSecAttrAccessibleAlwaysThisDeviceOnly"
            };

        public const int WRAP_DEVICE = 1;
        public const int WRAP_PASSCODE = 2;


    }
}
