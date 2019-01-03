using System;
using System.Collections.Generic;

namespace DecryptBackup
{
    public static class ExtendMethod
    {
        public static T GetValue<T>(this Dictionary<string, dynamic> sources, string key, T defaultValue)
        {
            return sources.ContainsKey(key) ? (T)sources[key] : defaultValue;
        }

        public static string GetValueString(this Dictionary<string, dynamic> sources, string key)
        {
            string strTarget = string.Empty;
            if (sources.ContainsKey(key))
            {
                dynamic target = sources[key];
                switch (target)
                {
                    case int intValue:
                        strTarget = intValue.ToString();
                        break;
                    case string str:
                        strTarget = str;
                        break;
                    case byte byteValue:
                        strTarget = byteValue.ToString("X2");
                        break;
                    case byte[] byteArray:
                        strTarget = byteArray.ShowString();
                        break;
                    default:
                        break;
                }
            }
            return strTarget;
        }

        public static int ToInt32(this byte[] array, bool reversed = false)
        {
            if (array == null) return 0;
            if (!reversed)
            {
                Array.Reverse(array);
            }
            switch (array.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return array[0];
                case 4:
                    return BitConverter.ToInt32(array, 0);
                default:
                    Array.Resize(ref array, 4);
                    return BitConverter.ToInt32(array, 0);
            }
        }

        public static ulong ToUInt64(this byte[] array)
        {
            if (array == null) return 0;
            Array.Reverse(array);
            switch (array.Length)
            {
                case 0:
                    return 0;
                case 1:
                    return array[0];
                case 2:
                    return BitConverter.ToUInt16(array, 0);
                case 4:
                    return BitConverter.ToUInt32(array, 0);
                case 8:
                    return BitConverter.ToUInt64(array, 0);
                default:
                    Array.Resize(ref array, 8);
                    return BitConverter.ToUInt64(array, 0);
            }
        }

        public static string ShowString(this byte[] array, bool needReverse = false)
        {
            if (array == null) return string.Empty;
            if (needReverse) Array.Reverse(array);
            return BitConverter.ToString(array).Replace("-", "");
        }
    }
}
