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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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

            NORTHWNDContext dataContext = new NORTHWNDContext();
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
