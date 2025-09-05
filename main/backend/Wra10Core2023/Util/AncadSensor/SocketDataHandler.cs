using System.Net;
using System.Net.Sockets;

namespace Wra10Core2023.Util.AncadSensor;

public class SocketDataHandler
{
    private int TimeoutMs;
    private TcpListener? listener;
    private ThreadSafeSet<TcpClient> clients = new();
    private bool ShouldStop = false;

    public void StartListening()
    {
        this.TimeoutMs = 30_000;

        var thread = new Thread(() =>
        {
            while (!ShouldStop)
            {
                Exceptions.PrintIfFailed(Listen);
                Thread.Sleep(500);
            }
        });

        thread.IsBackground = true;
        thread.Start();
    }

    private void StopAllClients()
    {
        while (clients.Count > 0)
        {
            var (success, c) = clients.Peek();
            if (!success) break;

            if (clients.TryRemove(c))
            {
                Exceptions.IgnoreException(() =>
                {
                    c.Close();
                    c.Dispose();
                });
            }
        }
    }

    public void Stop()
    {
        ShouldStop = true;

        Exceptions.IgnoreException(() =>
        {
            listener?.Stop();
            listener = null;
            StopAllClients();
        });
    }

    private void Listen()
    {
        var port = SiteUtil.AncadSensorPort;
        listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"Started ANCAD sensor listener");

            while (true)
            {
                var client = listener.AcceptTcpClient();

                // Handle each client
                var thread = CreateThreadForClient(client);
                thread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"socket listen error \n {e}");
        }
        finally
        {
            Exceptions.IgnoreException(() => listener?.Stop());
            listener = null;
        }
    }

    private Thread CreateThreadForClient(TcpClient client)
    {
        return new Thread(async () =>
        {
            try
            {
                if (!client.Connected) return;
                client.ReceiveTimeout = TimeoutMs;
                client.SendTimeout = TimeoutMs;
                clients.TryAdd(client);
                var stream = client.GetStream();
                var receiver = new SocketDataReceiver(stream)
                {
                    OnDataUpdateAsync = OnDataUpdateAsync
                };

                await receiver.StartAsync();
            }
            catch (EOLException)
            {
                Console.WriteLine($"disconnecting from client (no more data)");
            }
            catch (Exception e)
            {
                Console.WriteLine($"socket receive from client failed \n {e}");
            }
            finally
            {
                clients.TryRemove(client);
                client.Close();
                client.Dispose();
            }
        })
        { IsBackground = true };
    }

    private async Task OnDataUpdateAsync(InterfaceUpdateInfo info)
    {
        Console.WriteLine($"[ANCAD SENSOR] received: {info.InterfaceName} {info.Msg.Time}");
    }
}