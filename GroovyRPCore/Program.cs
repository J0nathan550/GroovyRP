using System;
using System.Diagnostics;
using DiscordRPC;
using DiscordRPC.Message;

namespace GroovyRP
{
    class Program
    {
        private const string appDetails = "GroovyRP\nhttps://github.com/dsdude123/GroovyRP\n";
        private static readonly DiscordRpcClient _client = new DiscordRpcClient("1036403671212245172", autoEvents: false);
        private static readonly GrooveInfoFetcher _grooveInfoFetcher = new GrooveInfoFetcher();
        private static string pressenceDetails = string.Empty;
        private static Timestamps timeStamp;

        private static void Main()
        {
            timeStamp = null;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(appDetails);
            Console.WriteLine("\nNothing Playing");

            _client.Initialize();
            _client.OnError += _client_OnError;
            _client.OnPresenceUpdate += _client_OnPresenceUpdate;

            TrackInfo currentTrack = new TrackInfo();
            TrackInfo oldTrack = new TrackInfo();

            bool isPresenceActive = false;

            if (timeStamp == null)
            {
                timeStamp = Timestamps.Now;
            }

            while (_client.IsInitialized)
            {
                if (_grooveInfoFetcher.IsUsingAudio())
                {
                    try
                    {
                        currentTrack = _grooveInfoFetcher.GetTrackInfo();
                        if (oldTrack.Title != currentTrack.Title)
                        {
                            var details = $"Название: {currentTrack.Title}";
                            var state = $"Автор: {currentTrack.Artist}";

                            _client.SetPresence(new RichPresence
                            {
                                Details = details,
                                State = state,
                                Assets = new Assets
                                {
                                    LargeImageKey = "groove",
                                    LargeImageText = "Groove Music",
                                    SmallImageKey = "groove_small",
                                    SmallImageText = $"{details}"
                                },
                                Timestamps = timeStamp
                            });
                            isPresenceActive = true;
                            _client.Invoke();
                        }
                    }
                    catch (Exception)
                    {
                        isPresenceActive = true;
                        _client.SetPresence(new RichPresence()
                        {
                            Details = "Неизвестный трек",
                            Timestamps = timeStamp,
                        });
                        Console.Clear();
                        Console.WriteLine(appDetails);
                        Console.WriteLine("\nFailed to get track info");
                    }
                }
                else
                {
                    _client.ClearPresence();
                    oldTrack = new TrackInfo();
                    if(isPresenceActive)
                    {
                        Console.Clear();
                        Console.WriteLine(appDetails);
                        Console.WriteLine("\nNothing Playing");
                        isPresenceActive = false;
                    }
                }
            }
        }

        private static void _client_OnPresenceUpdate(object sender, PresenceMessage args)
        {
            if (args.Presence != null)
            {
                if (pressenceDetails != args.Presence.Details)
                {
                    pressenceDetails = _client.CurrentPresence?.Details;
                    Console.Clear();
                    Console.WriteLine(appDetails);
                    Console.WriteLine($"{args.Presence.Details}, {args.Presence.State}");
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine(appDetails);
                Console.WriteLine("\nNothing Playing");
                pressenceDetails = string.Empty;
            }
        }

        private static void _client_OnError(object sender, ErrorMessage args)
        {
            Console.WriteLine(args.Message);
        }
    }
}
