using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace DecryptBackup
{
    internal class Keybag
    {
        private bool Unlocked = false;
        private dynamic Type { get; set; } = null;
        private byte[] UUID { get; set; }
        private dynamic Wrap { get; set; } = null;
        private dynamic DeviceKey { get; set; } = null;
        private Dictionary<string, dynamic> Attrs { get; set; } = new Dictionary<string, dynamic>();
        private Dictionary<int, Dictionary<string, dynamic>> ClassKeys { get; set; }
            = new Dictionary<int, Dictionary<string, dynamic>>();
        private dynamic KeyBagKeys { get; set; } = null;

        internal Keybag(byte[] data)
        {
            ParseBinaryBlob(data);
        }

        private void ParseBinaryBlob(byte[] data)
        {
            Dictionary<string, dynamic> currentClassKey = new Dictionary<string, dynamic>();
            foreach (KeyValuePair<string, byte[]> item in LoopTLVBlocks(data))
            {
                dynamic target = item.Value;
                if (item.Value.Length == 4)
                {
                    target = item.Value.ToInt32();
                }
                switch (item.Key)
                {
                    case "TYPE":
                        Type = target;
                        if (Type > 3)
                        {
                            Trace.WriteLine($"FAIL: keybag type > 3 :{Type}");
                        }
                        break;
                    case "UUID":
                        if (UUID == null)
                        {
                            UUID = target;
                        }
                        else
                        {
                            if (currentClassKey.ContainsKey("CLAS"))
                                ClassKeys[currentClassKey["CLAS"]] = currentClassKey;
                            currentClassKey = new Dictionary<string, dynamic>() { ["UUID"] = target };
                        }
                        break;
                    case "WRAP":
                        if (Wrap == null)
                        {
                            Wrap = target;
                        }
                        else if (ConstData.CLASSKEY_TAGS.Contains(item.Key))
                        {
                            currentClassKey[item.Key] = target;
                        }
                        break;
                    default:
                        if (ConstData.CLASSKEY_TAGS.Contains(item.Key))
                        {
                            currentClassKey[item.Key] = target;
                        }
                        else
                        {
                            Attrs[item.Key] = target;
                        }
                        break;
                }
            }
            if (currentClassKey.Count > 0)
            {
                ClassKeys[currentClassKey["CLAS"]] = currentClassKey;
            }
        }

        private IEnumerable<KeyValuePair<string, byte[]>> LoopTLVBlocks(byte[] data)
        {
            Dictionary<string, byte[]> blocks = new Dictionary<string, byte[]>();
            int i = 0;
            int countLength = data.Length;
            int dataLength = 0;
            while (i + 8 <= countLength)
            {
                byte[] byteTag = new byte[4];
                Array.Copy(data, i, byteTag, 0, 4);
                string tag = Encoding.UTF8.GetString(byteTag);
                byte[] byteLength = new byte[4];
                Array.ConstrainedCopy(data, i + 4, byteLength, 0, 4);
                dataLength = byteLength.ToInt32();
                byte[] blockData = new byte[dataLength];
                Array.ConstrainedCopy(data, i + 8, blockData, 0, dataLength);
                yield return new KeyValuePair<string, byte[]>(tag, blockData);
                i += 8 + dataLength;
            }
        }

        private readonly byte[] EmptyByteArray = new byte[1];

        internal string PrintClassKeys()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("== Keybag");
            sb.AppendLine($"Keybag type:{ConstData.KEYBAG_TYPES[Type]} keybag {Type}");
            sb.AppendLine($"Keybag version:{Attrs.GetValueString("VERS")}");
            //sb.AppendLine($"Keybag iterations: {Attrs["ITER"]} iv={Attrs.GetValueString("SALT")}");
            sb.AppendLine($"Keybag UUID: {UUID.ShowString()}");
            sb.AppendLine(GetLineString());
            sb.AppendLine($"{"Class".PadRight(53)}{"WRAP".PadRight(5)}{"Type".PadRight(11)}{"Key".PadRight(65)}{"WPKY".PadRight(80)}Public key");
            sb.AppendLine(GetLineString());
            foreach (KeyValuePair<int, Dictionary<string, dynamic>> item in ClassKeys)
            {
                if (item.Key == 6)
                {
                    sb.AppendLine();
                }
                sb.AppendLine($"{ConstData.PROTECTION_CLASSES[item.Key].PadRight(53)}{item.Value.GetValue("WRAP", 0).ToString().PadRight(5)}{ConstData.KEY_TYPES[item.Value.GetValue("KTYP", 0)].PadRight(11)}{item.Value.GetValueString("KEY").PadRight(65)}{item.Value.GetValueString("WPKY").PadRight(80)}{item.Value.GetValueString("PBKY")}");
            }
            return sb.ToString();
        }

        private string GetLineString()
        {
            StringBuilder line = new StringBuilder();
            for (int i = 0; i < 256; i++)
            {
                line.Append("-");
            }
            line.Append(Environment.NewLine);
            return line.ToString();
        }

        internal bool UnlockWithPassword(string password)
        {
            PBKDF2 password1 = new PBKDF2(password, Attrs["DPSL"], Attrs["DPIC"], "HMACSHA256");
            byte[] byteArray1 = password1.GetBytes(32);
            PBKDF2 passCodeKey = new PBKDF2(byteArray1, Attrs["SALT"], Attrs["ITER"], "HMACSHA1");
            byte[] byteArray2 = passCodeKey.GetBytes(32);
            foreach (Dictionary<string, dynamic> item in ClassKeys.Values)
            {
                if (!item.ContainsKey("WPKY"))
                {
                    continue;
                }
                dynamic k = item["WPKY"];
                if ((item["WRAP"] & ConstData.WRAP_PASSCODE) != 0)
                {
                    k = AESUnwrap(item["WPKY"], byteArray2);
                    if (k == null)
                    {
                        Unlocked = false;
                        return false;
                    }
                    item["KEY"] = k;
                }
            }
            Unlocked = true;
            return Unlocked;
        }

        private byte[] AESUnwrap(byte[] toDecryptArray, byte[] passCodeKey)
        {
            List<ulong> C = new List<ulong>();

            for (int i = 0; i < toDecryptArray.Length / 8; i++)
            {
                byte[] c = new byte[8];
                Array.ConstrainedCopy(toDecryptArray, i * 8, c, 0, 8);
                ulong cValue = c.ToUInt64();
                C.Add(cValue);
            }
            int n = C.Count - 1;
            ulong[] R = new ulong[C.Count];
            ulong A = C[0];

            for (int i = 1; i < C.Count; i++)
            {
                R[i] = C[i];
            }

            for (int i = 5; i >= 0; i--)
            {
                for (int j = n; j > 0; j--)
                {
                    byte[] byteDecrypt = new byte[16];
                    ulong temp = A ^ (ulong)(n * i + j);
                    FillData(ref byteDecrypt, 0, temp);
                    FillData(ref byteDecrypt, 8, R[j]);
                    byte[] B = Security.DecryptECB(byteDecrypt, passCodeKey);
                    A = Getulong(B, 0);
                    R[j] = Getulong(B, 8);
                }
            }
            if (A != 0xa6a6a6a6a6a6a6a6)
            {
                Trace.WriteLine($"A值检查失败:{A}");
                return null;
            }
            byte[] byteResult = new byte[n * 8];
            for (int i = 1; i < R.Length; i++)
            {
                FillData(ref byteResult, (i - 1) * 8, R[i]);
            }
            return byteResult;
        }

        private void FillData(ref byte[] targetArray, int index, ulong value)
        {
            byte[] tempArray = BitConverter.GetBytes(value);
            Array.Reverse(tempArray);
            Array.ConstrainedCopy(tempArray, 0, targetArray, index, 8);
        }

        private ulong Getulong(byte[] sourceArray, int index, int length = 8)
        {
            byte[] tempArray = new byte[length];
            Array.ConstrainedCopy(sourceArray, index, tempArray, 0, length);
            return tempArray.ToUInt64();
        }

        internal byte[] UnwrapKeyForClass(int intManifestClass, byte[] arrayManifestKey)
        {
            dynamic ck = ClassKeys[intManifestClass]["KEY"];
            if (arrayManifestKey.Length != 0x28)
            {
                throw new Exception($"Invalid key length on {nameof(UnwrapKeyForClass)}");
            }
            byte[] result = AESUnwrap(arrayManifestKey, ck);
            return result;
        }
    }
}
