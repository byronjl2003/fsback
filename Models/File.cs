namespace fsbackend.Models
{
    public class File
    {
        private int id;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }



    }
}