using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace fsbackend.Models
{
    [System.Serializable]
    public class User
    {
        private string email;
        public string Email
        {
            get { return email; }
            set { email = value; }
        }


        private string nombre;
        public string Nombre
        {
            get { return nombre; }
            set { nombre = value; }
        }

        private string pass;
        public string Pass
        {
            get { return pass; }
            set { pass = value; }
        }

        public string serializeToString()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                var ar = ms.ToArray();
                return ar.ToString();

            }
        }

    }
}