using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using fsbackend.Data;
//using fsbackend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using MySql.Data.MySqlClient;

namespace fsbackend.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private MySqlDatabase _MySqlDatabase { get; set; }
        public FilesController(MySqlDatabase mySqlDatabase)
        {
            this._MySqlDatabase = mySqlDatabase;
        }



        /*
        PARA DESCARGAR UN ARCHIVO SEGUN EL ID
        @param id -> id del archivo
        */
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> getfile(int id)
        {
            //SE RECUPERA DE LA BASE DE DATOS EL PATH DEL ARCHIVO..
            try
            {
                var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
                cmd.CommandText = @"SELECT path,name from files where id = @param1";
                cmd.Parameters.AddWithValue("@param1", id);
                string path = "";
                string fname = "";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();
                    path = reader.GetFieldValue<string>(0);
                    fname = reader.GetFieldValue<string>(1);

                }
                var mimeType = "application/Content-Disposition";
                var fileBytes = Encoding.UTF8.GetBytes(path);

                using (var ms = new MemoryStream())
                {

                }
                var memory = new MemoryStream();
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, mimeType, Path.GetFileName(path));



            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR???--------> " + ex.Message);
                return NotFound();
            }



        }


        /*
        PARA OBTENER UN LISTADO DE TODOS LOS ARCHIVOS SEGUN EL USUARIO
        @param id -> id del usuario
        */

        [AllowAnonymous]
        [HttpGet("all/{id}")]
        public async Task<ActionResult<List<fsbackend.Models.File>>> getfiles(string id)
        {
            List<fsbackend.Models.File> lista = new List<fsbackend.Models.File>();

            var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT id,name from files where id_user = @param1";
            cmd.Parameters.AddWithValue("@param1", id);

            using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    var f = new fsbackend.Models.File();
                    f.ID = reader.GetFieldValue<int>(0);
                    f.Name = reader.GetFieldValue<string>(1);
                    f.Path = $"http://localhost:5000/api/v1/files/{f.ID}";
                    lista.Add(f);
                }
            return Ok(lista);




        }




        /*
        METODO PARA ELIMINAR UN ARCHIVO
        @param id = id del archivo
        */
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deletefile(int id)
        {


            var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT path FROM files where id = @param1";
            cmd.Parameters.AddWithValue("@param1", id);
            string oldpath = "";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                oldpath = reader.GetFieldValue<string>(0);

            }
            System.IO.File.Delete(oldpath);
            cmd.CommandText = @"DELETE from files where id = @param1";
            cmd.Parameters.AddWithValue("@param1", id);
            cmd.ExecuteNonQuery();


            return Ok(new { msg = "Borrado satisfactoriamente" });




        }


        /*
        METODO PARA MODIFICAR UN ARCHIVO
        */
        [AllowAnonymous]
        [HttpPut("{user}")]
        public async Task<ActionResult<List<fsbackend.Models.File>>> putfile([FromBody] fsbackend.Models.File body, string user)
        {
            List<fsbackend.Models.File> lista = new List<fsbackend.Models.File>();
            Console.WriteLine("PUT::");
            Console.WriteLine(body.ID);
            Console.WriteLine(body.Name);
            Console.WriteLine(user);

            var folderName = Path.Combine("Resources", user);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var newpath = Path.Combine(pathToSave, body.Name);

            var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT path FROM files where id = @param1";
            cmd.Parameters.AddWithValue("@param1", body.ID);
            string oldpath = "";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                oldpath = reader.GetFieldValue<string>(0);

            }
            System.IO.File.Move(oldpath, newpath);
            cmd.Parameters.Clear();
            cmd.CommandText = @"UPDATE files SET name = @param1, path = @param3 where id = @param2";
            cmd.Parameters.AddWithValue("@param1", body.Name);
            cmd.Parameters.AddWithValue("@param2", body.ID);
            cmd.Parameters.AddWithValue("@param3", newpath);
            cmd.ExecuteNonQuery();

            body.Path = $"http://localhost:5000/api/v1/files/{body.ID}";
            return Ok(body);




        }



        /*
        METODO PARA CREAR UN NUEVO ARCHIVO
        */
        [AllowAnonymous]
        [HttpPost("{user}"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadAsync(string user)
        {
            Console.WriteLine("SE VA A SUBIR F de:: ");
            Console.WriteLine(user);
            HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            try
            {

                var file = Request.Form.Files[0];


                //Console.WriteLine("Nombre: " + nombre.ToString());
                var folderName = Path.Combine("Resources", user);
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);


                if (file.Length > 0)
                {
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }

                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    Microsoft.Extensions.Primitives.StringValues nombre;
                    Request.Form.TryGetValue("nombre", out nombre);
                    if (nombre != "")
                    {
                        fileName = nombre.ToString();
                    }
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    System.UInt64 idfile = 0;
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                        var cmd = this._MySqlDatabase.Connection.CreateCommand() as MySqlCommand;
                        cmd.CommandText = @"INSERT INTO files(name,path,id_user) values(@para1,@para2,@para3)";
                        cmd.Parameters.AddWithValue("@para1", fileName);
                        cmd.Parameters.AddWithValue("@para2", fullPath);
                        cmd.Parameters.AddWithValue("@para3", user);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = @"SELECT LAST_INSERT_ID()";
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            idfile = reader.GetFieldValue<System.UInt64>(0);

                        }
                        //var rawdata = new Byte(file.Length){ }

                        //cmd.Parameters.AddWithValue("@para2", file.OpenReadStream().R);

                    }

                    fsbackend.Models.File resp = new Models.File();
                    resp.Name = fileName;
                    resp.ID = (int)idfile;
                    resp.Path = $"http://localhost:5000/api/v1/files/{idfile}";
                    return Ok(resp);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        /* public async Task<IActionResult> OnPostUploadAsync(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();
                    Console.WriteLine("PATH::", filePath);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size });
        } */

    }
}