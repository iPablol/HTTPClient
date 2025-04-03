using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTTPClient.CLI
{
    internal static class CLI
    {
        public static object? HandleCommand(Command command)
        {
            if (commandDictionary.TryGetValue(command.keyword, out Delegate? function))
            {
                return function.DynamicInvoke(command);
            }
            Console.WriteLine("Invalid command");
            return null;
        }

        private static TCPConnection? Connect(Command command)
        {
            try
            {
                IPAddress? address;
                if (!IPAddress.TryParse(command.args[0], out address))
                    address = Dns.GetHostEntry(command.args[0]).AddressList[0];
                int port = int.Parse(command.args[1]);
                return address is null ? null : new TCPConnection(address, port);
            }
            catch
            {
                return null;
            }
        }

        private static void Quit(Command command) => Environment.Exit(0);

        // Send a generic message
        private static void Send(Command command) => Program.client.connection?.Write(command.BuildArgs());

        // Might be interesting to add aliases
        private static readonly Dictionary<string, Delegate> commandDictionary = new()
        {
            { "connect", Connect },
            { "quit", Quit },
            { "write", Send }
        };
    }
}
