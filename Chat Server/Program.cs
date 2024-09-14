using Chat_Server;
using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

/*
    Chatserver Client for the SimpleChat Client
    This Server and Client is free to user
    for the ChatClient just get the repository on Github: https://github.com/IRayofficial/Simple_Chat.git
 */
class ChatServer
{
    private static List<ClientInfo> connectedClients = new List<ClientInfo>();
    private static string SignUpPattern = @"<username>(.*?)</username>|<public-key>(.*?)</public-key>";
    private static string MessagePattern = @"<from>(.*?)</from>|<content>(.*?)</content>|<send-to>(.*?)</send-to>";
    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("__________Simple Chat Server created________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("__________made by Raycraft Studios__________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("_____________Start Chat Server______________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("________Ready for client connections________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("____________________________________________");
        Console.WriteLine("____________________________________________");
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Task.Run(() => HandleClient(client));
            Console.WriteLine("__________Connected Clients: " + connectedClients.Count + 1 + "_______________");
        }
    }

    private static void HandleClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            string connectionMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var matches = Regex.Matches(connectionMessage, SignUpPattern);
            string userName = "";
            string publicKey = "";
            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) { userName = match.Groups[1].Value; }
                if (match.Groups[2].Success) { publicKey = match.Groups[2].Value; }   
            }

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(publicKey)){
                ClientInfo newClient = new ClientInfo
                {
                    Client = client,
                    UserName = userName,
                    PublicKey = publicKey
                };
                connectedClients.Add(newClient);

                Console.WriteLine(connectionMessage);
                BroadcastUserList();

                // Client Listener
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);

                    var getMessage = Regex.Matches(message, MessagePattern);
                    string from = "";
                    string sendMessage = "";
                    string sendToIndex = "";
                    foreach (Match match in getMessage)
                    {
                        if (match.Groups[1].Success) { from = match.Groups[1].Value; }
                        if (match.Groups[2].Success) { sendMessage = match.Groups[2].Value; }
                        if (match.Groups[3].Success) { sendToIndex = match.Groups[3].Value; }
                    }

                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(sendMessage) 
                        && !string.IsNullOrEmpty(sendToIndex) && int.TryParse(sendToIndex, out int id))
                    {
                        BroadcastMessage($"{sendMessage}", client, from, id);
                    }
                }
            }            
        }
        catch (Exception ex) { Console.WriteLine("--Connection-Error--"); }
        finally { DisconnectClient(client); }
    }

    // Send message to destination
    private static void BroadcastMessage(string message, TcpClient sender, string from, int id)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(BuildMessageBlock(from, message));
        TcpClient destination = connectedClients[id].Client;
        if (destination != sender)
        {
            try
            {
                NetworkStream stream = destination.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("--Send-Error:-" + ex.Message);
            }
        }
    }

    //Builder for Message 
    private static string BuildMessageBlock(string senderName, string message)
    {
        return "<type>MESSAGE</type><content><username>" + senderName + "</username><message>" + message + "</message></content>";
    }

    //Builder for sendable user list 
    private static string BuildUserListBlock()
    {
        StringBuilder userListMessage = new StringBuilder("<type>USERLIST</type><content>");
        foreach (var client in connectedClients)
        {
            userListMessage.Append($"<client><username>{client.UserName}</username><public-key>{client.PublicKey}</public-key></client>");
        }
        userListMessage.Append("</content>");
        return userListMessage.ToString();
    }

    //Send ALL active user list
    private static void BroadcastUserList()
    {
        byte[] buffer = Encoding.UTF8.GetBytes(BuildUserListBlock());
        foreach (var clientInfo in connectedClients)
        {
            try
            {
                NetworkStream stream = clientInfo.Client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("--Error:-" + ex.Message);
            }
        }
    }

    private static void DisconnectClient(TcpClient client)
    {
        var clientInfo = connectedClients.Find(c => c.Client == client);
        if (clientInfo != null)
        {
            connectedClients.Remove(clientInfo);
            Console.WriteLine($"Client-{clientInfo.UserName}-disconnected.");
            BroadcastUserList();
        }
        client.Close();
    }
}
