using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Command = HTTPClient.CLI.Command;
using CLI = HTTPClient.CLI.CLI;
using HTTPClient.CLI;

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
                    CommandResult result = CLI.CLI.HandleCommand(new(Console.ReadLine() ?? ""));
                    switch (result.result)
                    {
                        default:
                            Task<string> response = AwaitResponse();
                            response.Wait();
                            HandleResponse(response);
                            goto Connect;
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
                CommandResult result = CLI.CLI.HandleCommand(new(Console.ReadLine() ?? ""));
                switch (result.result)
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

        private async Task<string> AwaitResponse()
        {
            string responseMessage = await connection.Read();
            return responseMessage;
        }

        private void HandleResponse(Task<string> response)
        {
            Console.WriteLine(response.Result);
            connection?.Disconnect();
        }
    }
}
