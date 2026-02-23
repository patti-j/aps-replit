using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebAppImportService.Models
{
	/// <summary>
	/// Manages an asynchronous encryption for initial client server connections.
	/// Useful to pass a synchronous encryption key securely across the network.
	/// </summary>
	public class EncryptionHandshake : IDisposable
	{
		readonly RSACryptoServiceProvider m_rsa;
		//private RSAEncryptionPadding m_padding = RSAEncryptionPadding.OaepSHA512;

		public EncryptionHandshake()
		{
			m_rsa = new RSACryptoServiceProvider(2048);
		}

		public EncryptionHandshake(string a_publicKey)
		{
			m_rsa = new RSACryptoServiceProvider(2048);
			m_rsa.FromXmlString(a_publicKey);
		}

		public string GetEncryptionKey()
		{
			return m_rsa.ToXmlString(false);
		}

		public byte[] Encrypt(byte[] a_symmetricKey)
		{
			return m_rsa.Encrypt(a_symmetricKey, true);
		}

		public void Dispose()
		{
			m_rsa?.Dispose();
		}

		public byte[] Decrypt(byte[] a_encryptedData)
		{
			byte[] bytes = m_rsa.Decrypt(a_encryptedData, true);
			return bytes;
		}
	}
}
