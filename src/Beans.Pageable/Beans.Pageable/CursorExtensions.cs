using System.Globalization;
using System.Text;

namespace Beans.Pageable;

/// <summary>Extension methods for cursor encoding and decoding.</summary>
public static class CursorExtensions
{
    /// <summary>Encodes a given cursor as a user-facing opaque string.</summary>
    /// <param name="input">The cursor to encode.</param>
    /// <returns>The encoded cursor, or null if `input` is null.</returns>
    public static string? EncodeCursor(this long? input)
        => input?.EncodeCursor();

    /// <summary>Encodes a given cursor as a user-facing opaque string.</summary>
    /// <param name="input">The cursor to encode.</param>
    /// <returns>The encoded cursor.</returns>
    public static string EncodeCursor(this long input)
    {
        var bytes = Encoding.UTF8.GetBytes(input.ToString(CultureInfo.InvariantCulture));
        return Convert.ToBase64String(bytes, Base64FormattingOptions.None);
    }

    /// <summary>Tries to convert an encoded cursor string back into a number.</summary>
    /// <param name="input">The encoded string.</param>
    /// <param name="cursor">The decoded cursor.  Null if input is null or cursor is invalid.</param>
    /// <returns>True when input is successfully decoded, otherwise false.</returns>
    public static bool TryDecodeCursor(this string? input, out long? cursor)
    {
        cursor = null;
        if (input is null)
            return true;
        try
        {
            var bytes = Convert.FromBase64String(input);
            var utf8 = Encoding.UTF8.GetString(bytes);

            cursor = long.TryParse(utf8, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;

            return cursor is not null;
        }
        catch
        {
            return false;
        }
    }
}