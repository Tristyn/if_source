using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Hasher
{
    /// <summary>
    /// Size of salt.
    /// </summary>
    const int SaltSize = 16;

    /// <summary>
    /// Size of hash.
    /// </summary>
    const int HashSize = 20;

    const int Iterations = 100000;

    /// <summary>
    /// Creates a hash from a password.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="iterations">Number of iterations.</param>
    /// <returns>The hash.</returns>
    public static string Hash(string password, byte[] salt)
    {
        // Create hash
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hash;
        using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, Iterations))
        {
            hash = pbkdf2.GetBytes(HashSize);
        }

        // Combine salt and hash
        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to base64
        var base64Hash = Convert.ToBase64String(hashBytes);

        // Format hash with extra information
        return string.Format("$HASH$V1${0}${1}", Iterations, base64Hash);
    }

    /// <summary>
    /// Creates a hash from a password
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The hash.</returns>
    public static string Hash(string password)
    {
        byte[] salt;
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt = new byte[SaltSize]);
        }
        return Hash(password, salt);
    }

    /// <summary>
    /// Creates a hash from a password and salt
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The hash.</returns>
    public static string Hash(string password, string salt)
    {
        // Create salt
        byte[] saltBytes = new byte[SaltSize];
        byte[] saltBytesUncapped = Encoding.UTF8.GetBytes(salt);
        Array.Copy(saltBytesUncapped, saltBytes, Mathf.Min(SaltSize, saltBytesUncapped.Length));

        return Hash(password, saltBytes);
    }

    /// <summary>
    /// Checks if hash is supported.
    /// </summary>
    /// <param name="hashString">The hash.</param>
    /// <returns>Is supported?</returns>
    public static bool IsHashSupported(string hashString)
    {
        return hashString.StartsWith("$HASH$V1$");
    }

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <param name="hashedPassword">The hash.</param>
    /// <returns>Could be verified?</returns>
    public static bool Verify(string password, string hashedPassword)
    {
        // Check hash
        if (!IsHashSupported(hashedPassword))
        {
            throw new NotSupportedException("The hashtype is not supported");
        }

        // Extract iteration and Base64 string
        var splittedHashString = hashedPassword.Replace("$HASH$V1$", "").Split('$');
        var iterations = int.Parse(splittedHashString[0]);
        var base64Hash = splittedHashString[1];

        // Get hash bytes
        var hashBytes = Convert.FromBase64String(base64Hash);

        // Get salt
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Create hash with given salt
        byte[] hash;
        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            hash = pbkdf2.GetBytes(HashSize);
        }

        // Get result
        for (var i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
            {
                return false;
            }
        }
        return true;
    }
}