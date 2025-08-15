using System.Text;
using System.Text.RegularExpressions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal static class StringExtensions
{
    public static string CleanVersionDetails(this string content)
    {
        return Regex.Replace(content, @"(Version|Culture|PublicKeyToken)=.*?(\s*,|\s*}|\s*$)", string.Empty);
    }

    public static string UnescapeMulti(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        var prev = s;

        while (true)
        {
            var curr = UnescapeOnce(prev);
            if (curr == prev) // artık değişmiyorsa dur
                break;
            prev = curr;
        }

        return prev.Trim('"');
    }

    private static string UnescapeOnce(string s)
    {
        var sb = new StringBuilder(s.Length);
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\' && i + 1 < s.Length)
            {
                i++;
                switch (s[i])
                {
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': // \uXXXX
                        if (i + 4 < s.Length && TryParseHex4(s, i + 1, out var code))
                        {
                            sb.Append((char)code);
                            i += 4;
                        }
                        else
                        {
                            sb.Append("\\u");
                        }
                        break;
                    default:
                        sb.Append(s[i]); // bilinmeyen kaçış → aynen ekle
                        break;
                }
            }
            else
            {
                sb.Append(s[i]);
            }
        }
        return sb.ToString();
    }

    private static bool TryParseHex4(string s, int start, out int value)
    {
        value = 0;
        if (start + 4 > s.Length) return false;
        for (var k = 0; k < 4; k++)
        {
            var digit = HexValue(s[start + k]);
            if (digit < 0) return false;
            value = (value << 4) | digit;
        }
        return true;
    }

    private static int HexValue(char ch) =>
        ch switch
        {
            >= '0' and <= '9' => ch - '0',
            >= 'A' and <= 'F' => ch - 'A' + 10,
            >= 'a' and <= 'f' => ch - 'a' + 10,
            _ => -1
        };
}