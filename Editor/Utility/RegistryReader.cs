#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

namespace hackebein.objecttracking.utility
{
    public static class RegistryReader
    {
        // --- Constants & Types ---
        
        // Predefined registry root handles
        public static readonly UIntPtr HKEY_CLASSES_ROOT   = (UIntPtr)0x80000000;
        public static readonly UIntPtr HKEY_CURRENT_USER   = (UIntPtr)0x80000001;
        public static readonly UIntPtr HKEY_LOCAL_MACHINE  = (UIntPtr)0x80000002;
        public static readonly UIntPtr HKEY_USERS          = (UIntPtr)0x80000003;
        public static readonly UIntPtr HKEY_CURRENT_CONFIG = (UIntPtr)0x80000005;

        // Access mask for reading keys
        private const int KEY_READ = 0x20019;

        // Possible registry value types (not exhaustively listed)
        private const int REG_NONE      = 0;  // No value type
        private const int REG_SZ        = 1;  // Unicode nul terminated string
        private const int REG_EXPAND_SZ = 2;  // Unicode nul terminated string
        private const int REG_BINARY    = 3;  // Free form binary
        private const int REG_DWORD     = 4;  // 32-bit number
        private const int REG_QWORD     = 11; // 64-bit number

        // --- P/Invoke Signatures ---

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RegOpenKeyEx(
            UIntPtr hKey,
            string lpSubKey,
            int ulOptions,
            int samDesired,
            out IntPtr phkResult);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int RegQueryValueEx(
            IntPtr hKey,
            string lpValueName,
            IntPtr lpReserved,
            out int lpType,
            byte[] lpData,
            ref int lpcbData);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(IntPtr hKey);

        // --- Public Methods ---

        /// <summary>
        /// Reads (up to) 8 bytes from a Windows registry value and interprets them as a 64-bit (double).
        /// This mimics the Python function that always treats the read bytes as a double,
        /// even if the registry value is incorrectly marked as a DWORD.
        /// </summary>
        /// <param name="hive">One of the predefined registry root constants (e.g. HKEY_LOCAL_MACHINE).</param>
        /// <param name="subkey">The path to the registry key (e.g. "SOFTWARE\\MyKey").</param>
        /// <param name="valueName">The name of the registry value to read.</param>
        /// <param name="defaultValue">Default value if the read fails.</param>
        /// <returns>The registry value as a double, or the default value if the read fails.</returns>
        public static double ReadRegistryRawQword(UIntPtr hive, string subkey, string valueName, double defaultValue)
        {
            IntPtr hKey = IntPtr.Zero;
            try
            {
                // 1) Open the registry key
                int result = RegOpenKeyEx(hive, subkey, 0, KEY_READ, out hKey);
                if (result != 0 || hKey == IntPtr.Zero)
                {
                    // Could throw, but here we just return the default if open fails
                    throw new InvalidOperationException($"RegOpenKeyEx failed with error code {result}.");
                }

                // Prepare variables for the query
                int type;
                byte[] rawData = new byte[8];   // up to 8 bytes
                int dataSize = rawData.Length;  // initially 8

                // 2) Query the registry value
                result = RegQueryValueEx(hKey, valueName, IntPtr.Zero, out type, rawData, ref dataSize);
                if (result != 0)
                {
                    throw new InvalidOperationException($"RegQueryValueEx failed with error code {result}.");
                }

                // If fewer than 8 bytes were read, the rest remain zero; interpret as double
                // If more than 8 bytes exist in the registry, only the first 8 will be used
                if (dataSize < 8)
                {
                    // Zero-pad or just trust that the array is already zeroed
                    // so effectively only dataSize bytes are relevant
                }

                // 3) Interpret the raw bytes as a 64-bit double (little-endian)
                return BitConverter.ToDouble(rawData, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return defaultValue;
            }
            finally
            {
                // 4) Close the key
                if (hKey != IntPtr.Zero)
                {
                    RegCloseKey(hKey);
                }
            }
        }

        /// <summary>
        /// Converts up to 8 bytes of little-endian data into a 64-bit integer (signed or unsigned).
        /// This mirrors the Python function <c>bytes_to_qword_le</c>.
        /// </summary>
        /// <param name="raw">Raw bytes from the registry.</param>
        /// <param name="signed">Interpret as signed if true, otherwise unsigned.</param>
        /// <returns>A 64-bit integer (long in C#).</returns>
        public static long BytesToQwordLe(byte[] raw, bool signed = false)
        {
            if (raw.Length > 8)
                throw new ArgumentException("Data is longer than 8 bytes; unexpected for a QWORD.");

            // Create an 8-byte buffer; pad the remainder with zeros if less than 8
            byte[] data8 = new byte[8];
            Array.Copy(raw, data8, raw.Length);

            if (signed)
            {
                return BitConverter.ToInt64(data8, 0);
            }
            else
            {
                // For an "unsigned" interpretation, cast BitConverter.ToUInt64 to long
                // if you need it as a long. You could also return a ulong instead.
                ulong val = BitConverter.ToUInt64(data8, 0);
                return unchecked((long)val);
            }
        }
    }
}
#endif
