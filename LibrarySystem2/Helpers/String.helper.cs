using System.Globalization;

namespace Library2.Core.Helpers;

public static class StringHelper
{
    public static string ToTitleCase(this string input)
    {
        if ( string.IsNullOrWhiteSpace( input ) )
            return input;

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase( input.ToLower() );
    }
}