using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using fsbackend.Data;
using fsbackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Runtime.Serialization;
using System.Text;
using MySql.Data.MySqlClient;
//using Microsoft.AspNetCore.Cors;

namespace fsbackend.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]

    [Route("api/v1")]
    [ApiController]

    public class AutenticationsController : ControllerBase
    {
        private MySqlDatabase _MySqlDatabase { get; set; }
        public AutenticationsController(MySqlDatabase mySqlDatabase)
        {
            this._MySqlDatabase = mySqlDatabase;
        }

        [HttpGet("autenticacion/saludo")]
        public async Task<ActionResult<string>> logoutAsync()
        {
            string userstr;
            var flag = HttpContext.Request.Cookies.Keys.Count;
            var enu = HttpContext.Request.Cookies.Keys.GetEnumerator();
            var cookie = HttpContext.Request.Cookies.TryGetValue("FSCOOKIE", out userstr);
            while (enu.MoveNext())
            {
                Console.WriteLine(enu.Current);
            }
            Console.WriteLine(userstr);

            var em = HttpContext.Request.Cookies["NOMBRE"];
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine(em);


            return Ok(new { Message = "You are logged out" });

            /* var keys = HttpContext.Session.Keys.GetEnumerator();
            while (keys.MoveNext())
            {
                Console.WriteLine(keys.Current);
                Console.WriteLine(HttpContext.Session.GetString(keys.Current));

            } */

            /*  User user = null;
             using (MemoryStream ms = new MemoryStream(userbin))
             {
                 IFormatter br = new BinaryFormatter();
                 user = (User)br.Deserialize(ms);
             }
  */
            //return Ok();



            //return Ok(new { Message = "dsdsdsdsdsds" });
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //return Ok(new { Message = "You are logged out" });
        }
        [AllowAnonymous]
        [HttpGet("noauthorize")]
        public ActionResult<string> noauthorized()
        {
            return Unauthorized();
        }
        [AllowAnonymous]
        [HttpPost("autenticacion")]
        public async Task<ActionResult<User>> Login([FromBody] User body)
        {
            Console.WriteLine("LOGINNN::");
            Console.WriteLine(body.Email);
            Console.WriteLine(body.Nombre);
            Console.WriteLine(body.Pass);


            HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            HttpContext.Response.Cookies.Append("FSCOOKIE", body.Email + "__" + body.Nombre);
            //var props = new AuthenticationProperties();

            User t = null;
            var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT email,name,pass FROM user WHERE email = @user";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@user",
                DbType = System.Data.DbType.String,
                Value = body.Email,
            });
            using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    t = new User();
                    {
                        t.Email = reader.GetFieldValue<string>(0);
                        t.Nombre = reader.GetFieldValue<string>(1);
                        t.Pass = reader.GetFieldValue<string>(2);

                    };
                }

            if (t.Pass == body.Pass)
            {
                return Ok(t);
            }
            else
            {
                return StatusCode(500);
            }


            /* var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,usuario.Nombre),
                new Claim(ClaimTypes.Role,"ADMIN")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); */
            /*  BinaryFormatter bf = new BinaryFormatter();
             using (var ms = new MemoryStream())
             {
                 bf.Serialize(ms, usuario.Nombre);
                 HttpContext.Session.Set("usuario", ms.ToArray());
                 return Ok(new { Message = "You are logged in" });
             } */


            /* var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, usuario.Nombre),
                new Claim(ClaimTypes.Name, usuario.Nombre)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);
            
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal); */












        }

        public User desSerialize(string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            using (MemoryStream ms = new MemoryStream(b))
            {
                IFormatter br = new BinaryFormatter();
                return (User)br.Deserialize(ms);
            }

        }
    }


}