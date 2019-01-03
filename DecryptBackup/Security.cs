using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DecryptBackup
{
    internal class Security
    {
        public static byte[] DecryptECB(byte[] toDecryptArray, byte[] keyArray)
        {
            byte[] result;
            RijndaelManaged rijndael = new RijndaelManaged();
            rijndael.Mode = CipherMode.ECB;
            rijndael.Padding = PaddingMode.Zeros;
            rijndael.Key = keyArray;
            using (ICryptoTransform cTransform = rijndael.CreateDecryptor())
            {
                result = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
            }
            rijndael.Dispose();
            return result;
        }

        private static readonly byte[] ZEROIV = new byte[16];

        public static byte[] DecrypteCBC(byte[] toDecryptArray, byte[] keyArray, byte[] ivArray, bool isPadding = false)
        {
            if (toDecryptArray.Length % 16 > 0)
            {
                Trace.WriteLine("AESdecryptCBC: data length not /16, truncating");
                toDecryptArray = new byte[toDecryptArray.Length / 16 * 16];
            }
            RijndaelManaged rijndael = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.Zeros,
                Key = keyArray,
                IV = ivArray ?? ZEROIV
            };
            byte[] result;
            using (ICryptoTransform cTransform = rijndael.CreateDecryptor())
            {
                result = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
            }
            rijndael.Dispose();
            if (isPadding)
            {
                return RemovePadding(16, result);
            }
            return result;
        }

        private static byte[] RemovePadding(int blockSize, byte[] array)
        {
            byte n = array[array.Length - 1];
            if (n > blockSize || n > array.Length)
            {
                throw new System.Exception("invalid padding");
            }
            byte[] newArray = new byte[array.Length - n];
            Array.ConstrainedCopy(array, 0, newArray, 0, newArray.Length);
            return newArray;
        }
    }
}
