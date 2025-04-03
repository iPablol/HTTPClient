using HTTPClient.CLI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HTTPClient
{
    internal static class Utils
    {
        public static IPAddress? ResolveHost(string host)
        {
            if (!IPAddress.TryParse(host, out IPAddress? address))
                address = Dns.GetHostEntry(host).AddressList.FirstOrDefault();
            return address;
        }
    }
}
