using System.Runtime.InteropServices;
using System.Text;

namespace DoDo.Open.Sign
{
    public class DataHelper
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal,
            int size, string filePath);

        public static void SetValue<T>(string filePath, string section, string key, T val)
        {
            try
            {
                WritePrivateProfileString(section, key, val.ToString(), filePath);
            }
            catch
            {
                // ignored
            }
        }

        public static T GetValue<T>(string filePath, string section, string key)
        {
            try
            {
                var sb = new StringBuilder(255);
                GetPrivateProfileString(section, key, "", sb, 255, filePath);
                var retVal = (T)Convert.ChangeType(sb.ToString(), typeof(T));
                return retVal;
            }
            catch
            {
                return default;
            }

        }
    }
}
