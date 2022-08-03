using System.Text.RegularExpressions;

namespace DoDo.Open.AgingRole
{
    public static class TextUtil
    {
        /// <summary>
        /// 取文本中间
        /// </summary>
        /// <param name="str">原文</param>
        /// <param name="preStr">前文</param>
        /// <param name="nextStr">后文</param>
        /// <returns></returns>
        public static string GetMiddle(this string str, string preStr, string nextStr)
        {
            var regex = Regex.Match(str, $"{preStr}(.*?){nextStr}");
            return regex.Groups[1].Value;
        }

        /// <summary>
        /// 取文本左边
        /// </summary>
        /// <param name="str">原文</param>
        /// <param name="keyStr">关键文本</param>
        /// <returns></returns>
        public static string GetLeft(this string str, string keyStr)
        {
            var regex = Regex.Match(str, $"^(.*?){keyStr}");
            return regex.Groups[1].Value;
        }

        /// <summary>
        /// 取文本右边
        /// </summary>
        /// <param name="str">原文</param>
        /// <param name="keyStr">关键文本</param>
        /// <returns></returns>
        public static string GetRight(this string str, string keyStr)
        {
            var regex = Regex.Match(str, $"{keyStr}(.*?)$");
            return regex.Groups[1].Value;
        }
    }
}
