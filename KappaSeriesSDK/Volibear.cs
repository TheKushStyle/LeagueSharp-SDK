using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

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
           Bootstrap.Init(new string[] { });
           LoadMenu();
       }

       private void LoadMenu()
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

           CreateMenu();
           Game.OnUpdate += Game_OnUpdate;
           Drawing.OnDraw += Drawing_OnDraw;
       }

       private void CreateMenu()
       {
           _cfg = new Menu("VoliBear", "Volibear", true);
           

           var _combo = new Menu("Combo", "Combo");
           {
               _combo.Add(new MenuBool("QCombo", "Use Q", true));
               _combo.Add(new MenuBool("WCombo", "Use W", true));
               _combo.Add(new MenuBool("ECombo", "Use E", true));
               //_combo.Add(new MenuBool("UseItems", "Use Items", true));
               _combo.Add(new MenuSlider("HPW", "Min Enemy HP% To Use W", 30, 0, 100));
               _combo.Add(new MenuBool("AutoR", "Auto use R", true));
               _combo.Add(new MenuSlider("CountR", "Num of Enemy in Range to Ult", 3, 0, 5));

               _cfg.Add(_combo);

           }

           var _harass = new Menu("Harass", "Harass");
           {
               _harass.Add(new MenuBool("HarassW", "Use W in Harass", true));
               _harass.Add(new MenuBool("HarassE", "Use E in Harass",true));
               _harass.Add(new MenuSlider("HPWHarass", "Min Enemy HP% To Use W", 30, 0, 100));

               _cfg.Add(_harass);
           }

           var _laneclear = new Menu("LaneClear", "LaneClear");
           {
               _laneclear.Add(new MenuBool("UseQLane", "Use Q", true));
               _laneclear.Add(new MenuBool("UseWLane", "Use W", true));
               _laneclear.Add(new MenuBool("UseELane", "Use E", true));
               _cfg.Add(_laneclear);
           }

           var _jungleclear = new Menu("JungleClear", "JungleClear");
           {
               _jungleclear.Add(new MenuBool("UseQJungle", "Use Q", true));
               _jungleclear.Add(new MenuBool("UseWJungle", "Use W", true));
               _jungleclear.Add(new MenuBool("UseEJungle", "Use E", true));
               _cfg.Add(_jungleclear);
           }

           var _killsteal = new Menu("KillSteal", "KillSteal");
           {
               _killsteal.Add(new MenuBool("SmartKS", "Smart KillSteal", true));
               _cfg.Add(_killsteal);
           }

           var _flee = new Menu("Flee", "Flee");
           {
               _flee.Add(new MenuKeyBind("ActiveFlee", "Flee!", Keys.G, KeyBindType.Press));
               _cfg.Add(_flee);
           }

           var _drawings = new Menu("Drawings", "Drawings");
           {
               _drawings.Add(new MenuBool("DrawQ", "Draw Q", true));
               _drawings.Add(new MenuBool("DrawW", "Draw W", true));
               _drawings.Add(new MenuBool("DrawE", "Draw E", true));
               _drawings.Add(new MenuBool("DrawR", "Draw R", true));
               _cfg.Add(_drawings);
           }
           _cfg.Attach();
       }

       private static void Game_OnUpdate(EventArgs args)
       {
            if (_cfg["KillSteal"]["SmartKS"].GetValue<MenuBool>().Value)
           {
               Smartks();
           }
           

           switch (Orbwalker.ActiveMode)
           {
                   case OrbwalkerMode.Orbwalk:
                   Combo();
                   break;

                   case OrbwalkerMode.LaneClear:
                   LaneClear();
                   JungleClear();
                   break;

                   case OrbwalkerMode.Hybrid:
                   Harass();
                   break;

           }
       }

       private static void Harass()
       {
           var t = TargetSelector.GetTarget(_w.Range);
           if (t == null) return;

           if (_player.Distance(t) <= _e.Range && _e.IsReady() && _cfg["Harass"]["EHarass"].GetValue<MenuBool>().Value && t.IsValidTarget())
           {
               _e.Cast();
           }

           var health = t.Health;
           var maxhealth = t.MaxHealth;

           float wcount = _cfg["Harass"]["HPWHarass"].GetValue<MenuSlider>().Value;

           if (health < ((maxhealth * wcount) / 100))
           {
               if (_cfg["Harass"]["WHarass"].GetValue<MenuBool>().Value && _w.IsReady() && _player.Distance(t) <= _w.Range && t.IsValidTarget())
               {
                   _w.Cast(t);
               }
           }
       }

       private static void JungleClear()
       {
           var jungleMonster = GameObjects.JungleLarge.Where(i => (_player.Distance(i) <= _w.Range)).ToList();
           if (_cfg["JungleClear"]["UseQJungle"].GetValue<MenuBool>().Value && _q.IsReady())
           {
               _q.Cast();
           }

           if (_cfg["JungleClear"]["UseQJungle"].GetValue<MenuBool>().Value && _w.IsReady())
           {
               _w.Cast(jungleMonster[0]);
           }

           if (_cfg["JungleClear"]["UseEJungle"].GetValue<MenuBool>().Value && _e.IsReady())
           {
               _e.Cast();
           }
       }

       private static void LaneClear()
       {
           var minion = GameObjects.EnemyMinions.Where(min => min.IsValid && min.Distance(_player) < _w.Range).ToList();

           if (_cfg["LaneClear"]["UseQLane"].GetValue<MenuBool>().Value && _q.IsReady())
           {
               _q.Cast();
           }

           if (_cfg["LaneClear"]["UseQLane"].GetValue<MenuBool>().Value && _w.IsReady())
           {
               _w.Cast(minion[0]);
           }

           if (_cfg["LaneClear"]["UseELane"].GetValue<MenuBool>().Value && _e.IsReady())
           {
               _e.Cast();
           }
       }

       private static void Smartks()
       {
           foreach (
               var t in ObjectManager.Get<Obj_AI_Hero>().Where(t => t.IsEnemy).Where(t => t.IsValidTarget(_w.Range)))
           {
               if (t.Health <= _player.GetSpellDamage(t, SpellSlot.W) && _w.IsReady() && _player.Distance(t) <= _w.Range && t.IsValidTarget())
               {
                   _w.Cast(t);
               }

               if (t.Health <= _player.GetSpellDamage(t, SpellSlot.E) && _e.IsReady() && _player.Distance(t) <= _e.Range && t.IsValidTarget())
               {
                   _e.Cast();
               }

               else if (t.Health <= (_player.GetSpellDamage(t, SpellSlot.W) + _player.GetSpellDamage(t, SpellSlot.E)) && _e.IsReady() && _w.IsReady() && t.IsValidTarget())
               {
                   _w.Cast(t);
                   _e.Cast();
               }
           }
       }

       private static void Combo()
       {
           var t = TargetSelector.GetTarget(_q.Range);
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
           return ObjectManager.Get<Obj_AI_Hero>().Count(current => current.IsEnemy && Vector3.Distance(_player.ServerPosition, current.ServerPosition) <= _r.Range);
       }
       
       private static void Drawing_OnDraw(EventArgs args)
       {
           if (_cfg["Drawings"]["DrawQ"].GetValue<MenuBool>().Value)
               {
                   Drawing.DrawCircle(_player.Position,_q.Range, Color.BlueViolet);
               }

           if (_cfg["Drawings"]["DrawW"].GetValue<MenuBool>().Value)
           {
               Drawing.DrawCircle(_player.Position,_w.Range, Color.Blue);
           }

           if (_cfg["Drawings"]["DrawE"].GetValue<MenuBool>().Value)
           {
               Drawing.DrawCircle(_player.Position,_e.Range, Color.DarkGoldenrod);
           }

           if (_cfg["Drawings"]["DrawR"].GetValue<MenuBool>().Value)
           {
               Drawing.DrawCircle(_player.Position,_w.Range, Color.DarkMagenta);
           }

           }
       }
    }
