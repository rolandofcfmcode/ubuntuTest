using ApiNuevasTecnologias.DataAccess;
using ApiNuevasTecnologias.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
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
        public string Get(string userName, string password)
        {
            string token = "";
            var userInDB = dataContext.Users.Where(w => w.UserName == userName).FirstOrDefault();

            if (userInDB == null)
                return token;

            var hashPassword = userInDB.PasswordHash;
            var salt = userInDB.Salt;

            var decryptedPassword = Decrypt(hashPassword, salt);

            if (decryptedPassword == password)
            {
                token =  CreateJwtToken(userInDB);
            }
            return token;

        }

        private string CreateJwtToken(User userInDB)
        {

            try
            {
                //HEADER
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PrivateKeys.jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                //PAYLOAD
                var payload = new List<Claim>();
                payload.Add(new Claim("userName", userInDB.UserName));
                payload.Add(new Claim("userId", userInDB.UserId.ToString()));


                //ENCRIPTADO
                var token = new JwtSecurityToken("API_NUEVAS_TEC", "FCFM", claims: payload, null,
                    DateTime.Now.AddHours(1), signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                return "";
            
            }
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
