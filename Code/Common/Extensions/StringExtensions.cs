
namespace GrabCoin.Code.Common.Extensions
{
    public static class StringExtensions
    {
        public static string ApplyColorTag(this string str, string color)
        {
            return $"<color={color}>{str}</color>";
        }
    }
}