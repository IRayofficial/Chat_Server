using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

class ChatServer
{
    // Liste aller verbundenen Clients
    private static List<TcpClient> connectedClients = new List<TcpClient>();

    static void Main(string[] args)
    {
        // Erstelle einen TCP-Listener, der Verbindungen auf Port 8888 akzeptiert
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Chat-Server gestartet...");

        // Endlos-Schleife, um auf neue Client-Verbindungen zu warten
        while (true)
        {
            TcpClient client = server.AcceptTcpClient(); // Warten auf neuen Client
            connectedClients.Add(client); // Füge neuen Client zur Liste hinzu
            Console.WriteLine("Neuer Client verbunden!");

            // Starte einen Task, um den neuen Client zu handhaben
            Task.Run(() => HandleClient(client));
        }
    }

    // Verarbeite die Nachrichten eines verbundenen Clients
    private static void HandleClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            // Endlos-Schleife, um Nachrichten vom Client zu empfangen
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Nachricht empfangen: " + message);

                // Sende die Nachricht an alle verbundenen Clients
                BroadcastMessage(message, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler: " + ex.Message);
        }
        finally
        {
            client.Close();
            connectedClients.Remove(client);
            Console.WriteLine("Client getrennt.");
        }
    }

    // Sende eine Nachricht an alle Clients außer dem Sender
    private static void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        foreach (var client in connectedClients)
        {
            if (client != sender)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Senden an Client: " + ex.Message);
                }
            }
        }
    }
}
