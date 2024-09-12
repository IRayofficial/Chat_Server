using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server
{
    internal class ClientInfo
    {
        public TcpClient Client { get; set; }
        public string UserName { get; set; }
        public string PublicKey { get; set; }
    }
}
