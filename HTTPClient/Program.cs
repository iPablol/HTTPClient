
using System.Text;

namespace HTTPClient
{
    internal class Program
    {
        public static HTTPClient client = new();

        static void Main(string[] args)
        {
            client.Run();
        }


    }
}
