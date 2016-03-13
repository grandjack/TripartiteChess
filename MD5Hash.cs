using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WpfApplication2
{
    class MD5Hash
    {
        private MD5 md5Hash = null;
        private string hashCode = null;
        private string inputSource = null;

        public MD5Hash()
        {
            md5Hash = MD5.Create();
        }

        public MD5Hash(string inputSource)
        {
            this.inputSource = inputSource;
            md5Hash = MD5.Create();
            hashCode = GetMd5Hash(inputSource);
        }

        public string GetMD5HashCode()
        {
            if ((hashCode == null) && (inputSource != null))
            {
                hashCode = GetMd5Hash(inputSource);
            }

            return hashCode;
        }

        public string GetMd5Hash(byte []input)
        {
            if (input == null)
            {
                return "";
            }

            byte[] data = md5Hash.ComputeHash(input);
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public string GetMd5Hash(string input)
        {
            if (input == null)
            {
                return "";
            }
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public string GetMd5Hash(Stream input)
        {
            if (input == null)
            {
                return "";
            }
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(input);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public bool VerifyMd5Hash(string inputSource, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(inputSource);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Verify a hash against a string.
        public bool VerifyMd5Hash(byte[] inputSource, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(inputSource);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
