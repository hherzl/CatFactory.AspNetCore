namespace CatFactory.AspNetCore
{
    public class AspNetCoreProjectNamespaces
    {
        public AspNetCoreProjectNamespaces()
        {
            Controllers = "Controllers";
            Requests = "Requests";
            Responses = "Responses";
        }

        public string Controllers { get; set; }

        public string Requests { get; set; }

        public string Responses { get; set; }
    }
}
