using System.Text.RegularExpressions;

namespace SHOP_RUNNER.Common
{
    public class filter_characters
    {


        public static string cleanInput(string input)
        {
            try
            {
                return Regex.Replace(input, @"[^\w\.@-\s]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (Exception ex)
            {
                return input;
            }
        }

    }
}


/*
 private string CleanInput(string strIn)
        {
            // thay thế các ký tự ko hợp lệ bằng một chuỗi rỗng:
            try
            {

                return Regex.Replace(strIn, @"[^\w\.@-\s]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));

                // nếu xảy ra lỗi trả ra giá trị ban đầu:
            }catch (Exception ex)
            {
                return strIn;
            }
        }
 */