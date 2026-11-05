using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace BusinessKit.Domain;

/// <summary>
/// Unguessable identifiers for records that are reachable from public (unauthenticated)
/// endpoints, so callers cannot enumerate other customers' records by sequential id.
/// 128 bits of cryptographic randomness, encoded as 32 lowercase hex characters.
/// </summary>
public static partial class PublicTokens
{
    public const int Length = 32;

    public static string New() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();

    /// <summary>Cheap format check before hitting the database.</summary>
    public static bool IsWellFormed(string? token) => token != null && TokenFormat().IsMatch(token);

    [GeneratedRegex("^[0-9a-f]{32}$")]
    private static partial Regex TokenFormat();
}
