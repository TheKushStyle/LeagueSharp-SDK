using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.UI.INotifications;

namespace KappaSeriesSDK
{
    internal class Program
    {
        private static void Main(string[] args)
        {
           Load.OnLoad += Load_OnLoad;
        }

        private static void Load_OnLoad(object sender, EventArgs e)
        {
            try
            {
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
