using System.Runtime.InteropServices;
using System.Text;

namespace DoDo.Open.ChatGPT
{
    public class DataHelper
    {
        // Windows API 参考手册
        // http://www.office-cn.net/t/api/index.html?writeprivateprofilestring.htm

        // https://docs.microsoft.com/zh-cn/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringa
        [DllImport("kernel32")]
        private static extern byte WritePrivateProfileString(string section, string key, string val, string filePath);

        // https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, uint size, string filePath);

        // https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestringa
        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileStringA(string section, string key, string defVal, byte[] retVal, uint size, string filePath);

        /// <summary>
        /// 写值
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns>true表示写入成功，false表示写入失败</returns>
        public static bool WriteValue<T>(string filePath, string section, string key, T val)
        {
            try
            {
                var value = WritePrivateProfileString(section, key, val.ToString(), filePath);

                return value != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 删键
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns>true表示写入成功，false表示写入失败</returns>
        public static bool DeleteKey(string filePath, string section, string key)
        {
            var value = WritePrivateProfileString(section, key, null, filePath);

            return value != 0;
        }

        /// <summary>
        /// 删段
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <param name="section"></param>
        /// <returns>true表示写入成功，false表示写入失败</returns>
        public static bool DeleteSection(string filePath, string section)
        {
            var value = WritePrivateProfileString(section, null, null, filePath);

            return value != 0;
        }

        /// <summary>
        /// 读值
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? ReadValue<T>(string filePath, string section, string key)
        {
            try
            {
                var sb = new StringBuilder(256);
                GetPrivateProfileString(section, key, "", sb, 256, filePath);
                var retVal = (T)Convert.ChangeType(sb.ToString(), typeof(T));
                return retVal;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// 读段
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <returns></returns>
        public static List<string> ReadSections(string filePath)
        {
            List<string> sections = new List<string>();
            byte[] buf = new byte[65535];
            var charLength = GetPrivateProfileStringA(null, null, "", buf, 65535, filePath);

            int j = 0;
            for (int i = 0; i < charLength; i++)
            {
                if (buf[i] == 0)
                {
                    sections.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            }

            return sections;
        }

        /// <summary>
        /// 读键
        /// </summary>
        /// <param name="filePath">ini文件的路径。示例：H:\学习\C#\示例\Demo.ini</param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static List<string> ReadKeys(string filePath, string section)
        {
            List<string> keys = new List<string>();
            byte[] buf = new byte[65535];
            var charLength = GetPrivateProfileStringA(section, null, "", buf, 65535, filePath);

            int j = 0;
            for (int i = 0; i < charLength; i++)
            {
                if (buf[i] == 0)
                {
                    keys.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            }

            return keys;
        }
    }
}
