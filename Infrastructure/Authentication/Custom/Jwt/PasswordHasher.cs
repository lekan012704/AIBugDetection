//using Application.Abstractions.Authentication.Custom;
//using Domain.Application.Entities.Users;
//using Microsoft.AspNetCore.Identity;
//using System.Security.Cryptography;

//namespace Infrastructure.Authentication.Custom.Jwt;

//internal sealed class PasswordHasher : IPasswordHasher
//{
//    private const int SaltSize = 16;
//    private const int HashSize = 32;
//    private const int Iterations = 500000;

//    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

//    public string Hash(string password)
//    {
//        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
//        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

//        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
//    }

//    public bool Verify1(string password, string passwordHash)
//    {
//        string[] parts = passwordHash.Split('-');
//        byte[] hash = Convert.FromHexString(parts[0]);
//        byte[] salt = Convert.FromHexString(parts[1]);

//        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

//        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
//    }
//    public bool Verify(string password, string passwordHash)
//    {
//        // Decode the Base64 string to get the combined hash and salt
//        byte[] hashBytes = Convert.FromBase64String(passwordHash);

//        // Extract the salt and hash from the byte array
//        // Adjust these sizes if your SaltSize or HashSize are different
//        const int SaltSize = 16;
//        const int HashSize = 32;

//        byte[] salt = new byte[SaltSize];
//        byte[] hash = new byte[HashSize];

//        Array.Copy(hashBytes, 0, hash, 0, HashSize);
//        Array.Copy(hashBytes, HashSize, salt, 0, SaltSize);

//        // Hash the input password with the extracted salt
//        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 500000, HashAlgorithmName.SHA512, HashSize);

//        // Compare the hashes
//        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
//    }

//    public bool VerifyPassword(User user, string password)
//    {
//        var passwordHasher = new PasswordHasher<User>();
//        if (user.PasswordHash == null)
//        {
//            return false;
//        }
//        var hashedPassword = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
//        if (hashedPassword == PasswordVerificationResult.Failed)
//        {
//            return false;
//        }
//        // Return true if the password verification was successful or if a rehash is needed
//        return hashedPassword == PasswordVerificationResult.Success || hashedPassword == PasswordVerificationResult.SuccessRehashNeeded;
//    }

//    public string HashPassword(User user, string password)
//    {
//        var passwordHasher = new PasswordHasher<User>();
//        //var hashedPassword = passwordHasher.HashPassword(user, "123Pa$$word.");
//        var hashedPassword = passwordHasher.HashPassword(user, password);
//        if (string.IsNullOrWhiteSpace(hashedPassword))
//        {
//            //unable to generate a password hash
//            return "Unable to generate a password hash.";
//        }

//        return hashedPassword;
//    }

//}
