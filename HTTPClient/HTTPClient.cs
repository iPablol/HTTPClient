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

        private const string httpVersion = "HTTP/1.1";

        public TCPConnection? connection { get; private set; } = null;


        public void Run()
        {
            try
            {
                
                while (true)
                {
                    CommandResult result = CLI.CLI.HandleCommand(new(Console.ReadLine() ?? ""));
                    Task<bool>? successful = null;
                    switch (result.result)
                    {
                        case (string method, Uri url, string head):
                            successful = Request(method, url, head, "");
                            break;
                        case (string method, Uri url, string head, string body):
                            successful = Request(method, url, head, body);
                            break;
                        default:
                            
                            break;
                    }
                    if (successful is null) continue;
                    successful.Wait();
                    if (successful.Result)
                    {
                        Task<string> response = AwaitResponse();
                        response.Wait();
                        HandleResponse(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                connection?.Disconnect();
            }
        }

        private async Task<bool> Request(string method, Uri url, string head, string body)
        {
            if (url.Scheme != "http")
            {
                Console.WriteLine("Unsuported protocol");
                return false;
            }
            if (await Connect(url))
            {
                await connection.Write($"{method} / {httpVersion}\r\n{head}\r\n\r\n{body}");
                return true;
            }
            return false;
        }

        private async Task<bool> Connect(Uri url)
        {
            IPAddress? address = Utils.ResolveHost(url.Host);
            if (address is null) { Console.WriteLine("Error connecting to host"); return false; }
            int port = url.Port;
            connection = new TCPConnection(address, port);
            return await connection.Connect();
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
            // Handle error codes here
            Console.WriteLine(response.Result);
            connection?.Disconnect();
        }
    }
}
