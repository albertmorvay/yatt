using HashidsNet;
using System;
using System.Security.Cryptography;
using System.Text;

namespace YattWpf.Models
{
    class UserModel
    {
        public string ID
        {
            get {

                string userName = GetSHA256HashAsString(Environment.UserName);
                byte[] byteArray = Encoding.Default.GetBytes(userName);
                var integerRepresentingUsername = BitConverter.ToInt32(byteArray,0);

                var hashids = new Hashids(GetSHA256HashAsString(Environment.MachineName));
                return hashids.Encode(integerRepresentingUsername).ToUpper();
            }
        }

        private String GetSHA256HashAsString(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString().Replace(" ", "").Replace("-", "").ToUpper();
        }
    }
}
