using System;
using LeagueSharp;
using LeagueSharp.SDK.Core.UI.INotifications;

namespace KappaSeriesSDK
{
    internal class Program
    {
        private static Notification Load = new Notification("Kappa Series SDK Loaded", "Please remember this is still BETA");
        private static void Main(string[] args)
        {
           LeagueSharp.SDK.Core.Events.Load.OnLoad += Load_OnLoad;
        }

        private static void Load_OnLoad(object sender, EventArgs e)
        {
            try
            {
                Notifications.Add(Load);
                var cs = ObjectManager.Player.ChampionName;
                //var say = ("Kappa Series Loaded : ");
                //var def = ("Kappa Series Doesn't Support : ");
                switch (cs)
                {
                    case "Volibear":
                        new Volibear();
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
