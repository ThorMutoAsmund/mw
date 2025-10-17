using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MW.Helpers
{
    public static class HashingTool
    {
        public static string GenerateHash(IEnumerable<object> dependecies)
        {
            var concatenation = string.Concat(dependecies);
            return Sha1Hex16(concatenation);
        }

        private static string Sha1Hex16(string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] hash = SHA1.HashData(bytes);                 // 20 bytes
            return Convert.ToHexString(hash).ToLowerInvariant().Substring(0, 16);
        }
    }
}
