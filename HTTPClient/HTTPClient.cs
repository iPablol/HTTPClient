using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Command = HTTPClient.CLI.Command;
using CLI = HTTPClient.CLI.CLI;

namespace HTTPClient
{
    public class HTTPClient
    {
        public bool connected
        {
            get
            {
                return connection?.connected ?? false;
            }
        }

        public TCPConnection? connection { get; private set; } = null;
        public HTTPClient()
        {
        }


        public void Run()
        {
            try
            {
                Connect:
                connection = null;
                Task<bool> connect = WaitForConnection();
                connect.Wait();
                if (!connect.Result) goto Connect;
                
                while (true)
                {
                    switch (CLI.CLI.HandleCommand(new(Console.ReadLine() ?? "")))
                    {
                        default: break;
                    }
                    //if (command == "read")
                    //{
                    //    connection.Read();
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                connection.Disconnect();
            }
        }

        private async Task<bool> WaitForConnection()
        {
            Console.WriteLine("Waiting for connection");
            while (connection is null)
            {
                
                switch (CLI.CLI.HandleCommand(new(Console.ReadLine() ?? "")))
                {
                    case TCPConnection connection:
                        this.connection = connection;
                        break;
                    default: break;
                }  
            }
            bool connected = await connection.Connect();
            if (connected)
            {
                Console.WriteLine("Connection established!");
            }
            else
            {
                Console.WriteLine("Connection failed.");
            }
            return connected;
        }
    }
}
