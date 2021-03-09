using ApiNuevasTecnologias.DataAccess;
using ApiNuevasTecnologias.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiNuevasTecnologias.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/<UserController>
        private static string secretKey = "0406b130-bd65-11ea-b3de-0242ac130004-ab0660bb-a138-45e9-bdf1-9ff1ac62ae5e-0242ac130004-ab0660bb-a138-45e9-b";
        private static NORTHWNDContext dataContext = new NORTHWNDContext();
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/login?userName="rolando"?password="123456"
        [HttpGet("login")]
        public bool Get(string userName, string password)
        {

            var userInDB = dataContext.Users.Where(w => w.UserName == userName).FirstOrDefault();

            if (userInDB == null)
                return false;

            var hashPassword = userInDB.PasswordHash;
            var salt = userInDB.Salt;

            var decryptedPassword = Decrypt(hashPassword, salt);

            var result = decryptedPassword == password;

            return result;

        }

        private string Decrypt(string cipherText, string saltValue)
        {
            var mySecretKey = secretKey;
            var saltBuffer = Encoding.UTF8.GetBytes(saltValue);
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(mySecretKey, saltBuffer, 1000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            return cipherText;

        }

        // POST api/<UserController>
        [HttpPost]
        public void Post([FromBody] UserModel newUser)
        {

            var userInDB = new User();
            userInDB.UserName = newUser.UserName;
            userInDB.CreationDate = DateTime.Now;
            userInDB.UserId = Guid.NewGuid();
            userInDB.Salt = Guid.NewGuid().ToString();

            userInDB.PasswordHash = Encrypt(userInDB.Salt, newUser.Password);

            dataContext.Users.Add(userInDB);
            dataContext.SaveChanges();
        }

        private string Encrypt(string salt, string password)
        {
            string mySecretKey = secretKey;
            var saltBuffer = Encoding.UTF8.GetBytes(salt);
            var clearBytes = Encoding.UTF8.GetBytes(password);

            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(mySecretKey, saltBuffer, 1000, HashAlgorithmName.SHA256);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    password = Convert.ToBase64String(ms.ToArray());
                }
            }

            return password;
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
