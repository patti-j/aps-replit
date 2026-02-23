using System.Security.Cryptography;

namespace WebAPI.Models
{
	public static class StringHasher
	{
		public static byte[] Hash(string a_key, byte[] a_salt)
		{
			var pbkdf2 = new Rfc2898DeriveBytes(a_key, a_salt, 10000);
			byte[] hash = pbkdf2.GetBytes(20);
			return hash;
		}

		public static byte[] GenerateSalt()
		{
			byte[] salt = new byte[16];
			new RNGCryptoServiceProvider().GetBytes(salt);
			return salt;
		}
	}
}
