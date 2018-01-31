using System;
using System.Threading;
using System.Threading.Tasks;
using DiscordRpc;

namespace DemoApp
{
    public static class Program
    {
        private static RichPresence _presence;
        private static bool _cancel;

        static void Main(string[] args)
        {
            _presence = new RichPresence();
            _presence.Initialize("407040311819370497");
            _presence.Ready += Rpc_Ready;
            _presence.Disconnected += Rpc_Disconnected;
            _presence.Errored += Rpc_Errored;

            var callbackTask = RunCallbacks();

            while (true)
            {
                var presence = Console.ReadLine();

                if (presence == "q")
                    break;

                _presence.Update(new RichPresenceBuilder().WithState("Testing discord-rpc.net", presence));
            }

            Console.WriteLine("Disposing Discord RPC");
            _presence.Shutdown();
            callbackTask.Wait(3000);
            _presence.Dispose();
            Console.WriteLine("Disposed.");
            Console.ReadLine();
        }

        private static void Rpc_Ready()
        {
            Console.WriteLine("Discord RPC is ready, type a message to update the game state. Type 'q' to exit.");
            _presence.Update(new RichPresenceBuilder().WithState("Ready to go!"));
        }

        private static void Rpc_Disconnected(int errorCode, string message)
        {
            Console.WriteLine($"Disconnected: {errorCode} {message}");
        }

        private static void Rpc_Errored(int errorCode, string message)
        {
            Console.WriteLine($"Errored: {errorCode} {message}");
        }

        private static async Task RunCallbacks()
        {
            await Task.Run(() =>
            {
                while (!_cancel)
                {
                    _presence.RunCallbacks();
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
