using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;

namespace KappaSeriesSDK
{
   class Volibear
    {
        private static List<Spell> _spellList = new List<Spell>();
        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;
        private static Menu _cfg;
        /*private static Items.Item _rdo;
        private static Items.Item _yoy;
        private static Items.Item _botk;
        private static Items.Item _hyd;
        private static Items.Item _rg;
        private static Items.Item _cut;
        */
       private static Obj_AI_Hero _player;

       public Volibear()
       {
           Load();
       }

       private void Load()
       {
           _player = ObjectManager.Player;

           _q = new Spell(SpellSlot.Q, 600);
           _w = new Spell(SpellSlot.W, 400);
           _e = new Spell(SpellSlot.E, 400);
           _r = new Spell(SpellSlot.R, 125);

           _spellList.Add(_q);
           _spellList.Add(_w);
           _spellList.Add(_e);
           _spellList.Add(_r);
           
           _cfg = new Menu("VoliBear", "Volibear",true);

           var _combo = new Menu("Combo", "Combo");
           {
               _combo.Add(new MenuBool("QCombo", "Use Q", true));
               _combo.Add(new MenuBool("WCombo", "Use W", true));
               _combo.Add(new MenuBool("ECombo", "Use E", true));
             //_combo.Add(new MenuBool("UseItems", "Use Items", true));
               _combo.Add(new MenuSlider("HPW", "Min Enemy HP% To Use W", 30, 0, 100));
               _combo.Add(new MenuBool("AutoR", "Auto use R", true));
               _combo.Add(new MenuSlider("CountR", "Num of Enemy in Range to Ult",3,0,5));

               _cfg.Add(_combo);

           }

           _cfg.Attach();
           Game.OnUpdate += Game_OnUpdate;



       }

       private static void Game_OnUpdate(EventArgs args)
       {
           switch (Orbwalker.ActiveMode)
           {
                   case OrbwalkerMode.Orbwalk:
                   Combo();
                   break;

           }
       }

       private static void Combo()
       {
           Obj_AI_Hero t = TargetSelector.GetTarget(400);
           if (t == null) return;

           if (_player.Distance(t) <= _q.Range && _q.IsReady() && _cfg["Combo"]["QCombo"].GetValue<MenuBool>().Value && t.IsValidTarget())
           {
               _q.Cast();
           }

           if (_player.Distance(t) <= _e.Range && _e.IsReady() && _cfg["Combo"]["ECombo"].GetValue<MenuBool>().Value && t.IsValidTarget())
           {
               _e.Cast();
           }

           var health = t.Health;
           var maxhealth = t.MaxHealth;

           float wcount = _cfg["Combo"]["HPW"].GetValue<MenuSlider>().Value;

           if (health < ((maxhealth * wcount) / 100))
           {
               if (_cfg["Combo"]["WCombo"].GetValue<MenuBool>().Value && _w.IsReady() && _player.Distance(t) <= _w.Range && t.IsValidTarget())
               {
                   _w.Cast(t);
               }
           }

           if (_cfg["Combo"]["AutoR"].GetValue<MenuBool>().Value && GetNumberHitByR(t) >= _cfg["Combo"]["CountR"].GetValue<MenuSlider>().Value && _r.IsReady() && t.IsValidTarget())
           {
               _r.Cast();
           }
       }

       private static int GetNumberHitByR(Obj_AI_Base t)
       {
          var totalHit = 0;
           foreach (Obj_AI_Hero current in ObjectManager.Get<Obj_AI_Hero>())
           {
               if (current.IsEnemy && Vector3.Distance(_player.ServerPosition, current.ServerPosition) <= _r.Range)
               {
                   totalHit = totalHit + 1;
               }
           }
           return totalHit;
       }
    }
}
