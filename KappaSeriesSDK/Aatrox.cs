using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LeagueSharp;
using LeagueSharp.SDK.Core;
using LeagueSharp.SDK.Core.Enumerations;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Extensions;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.Core.Utils;
using LeagueSharp.SDK.Core.Wrappers;
using SharpDX;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

namespace KappaSeriesSDK
{
    class Aatrox
    {
        private static List<Spell> _spellList = new List<Spell>();
        private static Spell _q;
        private static Spell _w;
        private static Spell _e;
        private static Spell _r;
        private static Menu _cfg;
        public static SpellSlot IgniteSlot;
        public static SpellSlot SmiteSlot;
        /*private static Items.Item _rdo;
        private static Items.Item _yoy;
        private static Items.Item _botk;
        private static Items.Item _hyd;
        private static Items.Item _rg;
        private static Items.Item _cut;
        */
        private static Obj_AI_Hero _player;

        public Aatrox()
        {
            Bootstrap.Init(new string[] { });
            LoadMenu();
        }

        private void LoadMenu()
        {
            _player = ObjectManager.Player;

            _q = new Spell(SpellSlot.Q, 676f);
            _w = new Spell(SpellSlot.W, 325f);
            _e = new Spell(SpellSlot.E, 980f);
            _r = new Spell(SpellSlot.R, 550f);

            _q.SetSkillshot(0.6f, 250f, 2000f, false, SkillshotType.SkillshotCircle);
            _e.SetSkillshot(0.25f, 35f, 1250f, false, SkillshotType.SkillshotLine);

            _spellList.Add(_q);
            _spellList.Add(_w);
            _spellList.Add(_e);
            _spellList.Add(_r);

            IgniteSlot = _player.GetSpellSlot("SummonerDot");
            SmiteSlot = _player.GetSpellSlot("summonersmite");

            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            InterruptableSpell.OnInterruptableTarget += OnPossibleToInterrupt;
        }

        private static void OnPossibleToInterrupt(object sender, InterruptableSpell.InterruptableTargetEventArgs e)
        {
            if (!_cfg["Misc"]["IntQ"].GetValue<MenuBool>().Value || !_q.IsReady() || !e.Sender.IsValidTarget(_q.Range))
            {
               return; 
            }
            if (e.DangerLevel == DangerLevel.High ||
                e.DangerLevel == DangerLevel.Medium && _cfg["Misc"]["IntMed"].GetValue<MenuBool>().Value)
            {
                _q.Cast(e.Sender);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_cfg["Drawings"]["DrawQ"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(_player.Position, _q.Range, Color.BlueViolet);
            }

            if (_cfg["Drawings"]["DrawE"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(_player.Position, _e.Range, Color.DarkGoldenrod);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_player.IsDead)
            {
                return;
            }
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
            var t = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
            if (t == null) return;

            if (t.IsValidTarget() && _e.IsReady() && _cfg["Harass"]["HarE"].GetValue<MenuBool>().Value && _player.Distance(t) <= _e.Range)
            {
                _e.Cast(t);
            }
            if (t.IsValidTarget() && _q.IsReady() && _cfg["Harass"]["HarQ"].GetValue<MenuBool>().Value &&
                _player.Distance(t) <= _q.Range && !t.IsUnderTurret())
            {
                _q.Cast(t);
            }
        }

        private static void LaneClear()
        {
            var minion = GameObjects.EnemyMinions.Where(min => min.IsValid && min.Distance(_player) < _q.Range).ToList();
            Console.WriteLine("Lane" + minion.Count);
            if (minion.Count < 3)
            {
                return;
            }

            if (_cfg["LaneClear"]["UseQlane"].GetValue<MenuBool>().Value && _q.IsReady())
            {
                _q.Cast(minion[0]);
            }

            if (_cfg["LaneClear"]["UseWLane"].GetValue<MenuBool>().Value && _w.IsReady() && _player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
            {
                _w.Cast();
            }

            if (_cfg["LaneClear"]["UseELane"].GetValue<MenuBool>().Value && _e.IsReady())
            {
                _e.Cast(minion[0].ServerPosition);
            }
        }

        private static void JungleClear()
        {
            var jungleMonster = GameObjects.JungleLarge.Where(i => (_player.Distance(i) <= _q.Range)).ToList();
            if (jungleMonster.Count < 3)
            {
                return;
            }

            if (_cfg["JungleClear"]["UseQJungle"].GetValue<MenuBool>().Value && _q.IsReady())
            {
                _q.Cast(jungleMonster[0]);
            }

            if (_cfg["JungleClear"]["UseQJungle"].GetValue<MenuBool>().Value && _w.IsReady() && _player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
            {
                _w.Cast();
            }

            if (_cfg["JungleClear"]["UseEJungle"].GetValue<MenuBool>().Value && _e.IsReady())
            {
                _e.Cast(jungleMonster[0]);
            }
        }

        private static void Combo()
        {
            var t = TargetSelector.GetTarget(_e.Range, DamageType.Physical);
            //var youm = ItemData.Youmuus_Ghostblade;
            //var bil = ItemData.Bilgewater_Cutlass;
            //var botrk = ItemData.Blade_of_the_Ruined_King;

            if (t == null) return;

            #region e

            if (_e.IsReady() && _cfg["Combo"]["ECombo"].GetValue<MenuBool>().Value && _player.Distance(t) <= _e.Range)
            {
                _e.Cast(t);
            }
            #endregion
            #region q

            if (_q.IsReady() && _cfg["Combo"]["QCombo"].GetValue<MenuBool>().Value && _player.Distance(t) <= _q.Range)
            {
                if (_cfg["Combo"]["DontQ"].GetValue<MenuBool>().Value && t.IsUnderTurret())
                {
                    if (_cfg["Combo"]["Dive"].GetValue<MenuBool>().Value)
                    {
                        if (GetHealthPercent(t) <= _cfg["Combo"]["DiveTHP"].GetValue<MenuSlider>().Value)
                        {
                            if (GetHealthPercent(_player) >= _cfg["Combo"]["DiveMHP"].GetValue<MenuSlider>().Value)
                            {
                                _q.Cast(t);
                            }
                        }
                    }
                }
                else
                {
                    _q.Cast(t);
                }
            }
            #endregion
            #region W

            if (_w.IsReady() && _cfg["Combo"]["WCombo"].GetValue<MenuBool>().Value)
            {
                #region Smart W

                if (_cfg["Misc"]["SmartW"].GetValue<MenuBool>().Value)
                {
                    if (_player.Health < (_player.MaxHealth * 0.95))
                    {
                        if (_player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
                        {
                            _w.Cast();
                        }
                    }
                    else if (_player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        _w.Cast();
                    }
                }
                #endregion
                #region Normal W

                else if (!_cfg["Misc"]["SmartW"].GetValue<MenuBool>().Value)
                {
                    if (_w.IsReady() && GetHealthPercent(_player) < (_cfg["Combo"]["minW"].GetValue<MenuSlider>().Value))
                    {
                        if (_player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
                        {
                            _w.Cast();
                        }
                    }
                    if (_w.IsReady() && GetHealthPercent(_player) > (_cfg["Combo"]["maxW"].GetValue<MenuSlider>().Value))
                    {
                        if (_player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                        {
                            _w.Cast();
                        }
                    }
                }
                #endregion
                
            }
            #endregion
            #region R

            if (_r.IsReady() && _cfg["Combo"]["RCombo"].GetValue<MenuBool>().Value && GetNumberHitByR(t) >= _cfg["Combo"]["minR"].GetValue<MenuSlider>().Value && _player.Distance(t) <= _r.Range)
            {
                _r.Cast();
            }
            #endregion
        }

        private static float GetHealthPercent(Obj_AI_Hero player)
        {
            return player.Health * 100 / player.MaxHealth;
        }

        private static int GetNumberHitByR(Obj_AI_Base t)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Count(current => current.IsEnemy && Vector3.Distance(_player.ServerPosition, current.ServerPosition) <= _r.Range);
        }

        private static void Smartks()
        {
            foreach (
                var t in ObjectManager.Get<Obj_AI_Hero>().Where(t => t.IsEnemy).Where(t => t.IsValidTarget(_q.Range)))
            {
                #region e
                if (t.Health < _player.GetSpellDamage(t, SpellSlot.E) && _e.IsReady())
                {
                    _e.Cast(t);
                }
                #endregion
                #region q

                else if (t.Health < _player.GetSpellDamage(t, SpellSlot.Q) && _q.IsReady())
                {
                    _q.Cast(t);
                }
                #endregion
                #region eq
                else if (t.Health < (_player.GetSpellDamage(t, SpellSlot.Q) + (_player.GetSpellDamage(t, SpellSlot.E))) &&_e.IsReady() && _q.IsReady())
                {
                    _e.Cast(t);

                    if (_e.Cast(t) == CastStates.SuccessfullyCasted)
                    {
                        _q.Cast(t);
                    }
                }
                #endregion
                #region eq ignite

                else if ((t.Health < (_player.GetSpellDamage(t, SpellSlot.Q) + (_player.GetSpellDamage(t, SpellSlot.E) + _player.GetSpellDamage(t, IgniteSlot)))) && _e.IsReady() && IgniteSlot.IsReady() && _q.IsReady() && IgniteSlot != SpellSlot.Unknown)
                {
                    _e.Cast(t);
                    if (_e.Cast(t) == CastStates.SuccessfullyCasted)
                    {
                        _q.Cast(t);
                        _player.Spellbook.CastSpell(IgniteSlot, t);
                    }
                }
                #endregion
                #region eq Ignite R
                else if (_cfg["KillSteal"]["RKS"].GetValue<MenuBool>().Value && t.Health < (_player.GetSpellDamage(t, SpellSlot.Q) + _player.GetSpellDamage(t, SpellSlot.E) + _player.GetSpellDamage(t, IgniteSlot) + _player.GetSpellDamage(t, SpellSlot.R)) && _e.IsReady() && IgniteSlot.IsReady() && _q.IsReady() && _r.IsReady() && IgniteSlot != SpellSlot.Unknown)
                {
                    if (t.Distance(_player) <= _e.Range)
                    {
                        _e.Cast(t);
                    }
                    if (_e.Cast(t) == CastStates.SuccessfullyCasted && _player.Distance(t) <= _q.Range)
                    {
                        _q.Cast(t);
                    }
                    if (_player.Distance(t) < 600)
                    {
                        _player.Spellbook.CastSpell(IgniteSlot, t);
                    }
                    if (_player.Distance(t) <= _r.Range && _q.Cast(t) == CastStates.SuccessfullyCasted)
                    {
                        _r.Cast();
                    }
                }
                #endregion
            }
        }

        private void CreateMenu()
        {
            _cfg = new Menu("Aatrox", "Aatrox", true);

            var combo = new Menu("Combo", "Combo");
            {
                combo.Add(new MenuBool("QCombo", "Use Q", true));
                combo.Add(new MenuBool("WCombo", "Use W", true));
                combo.Add(new MenuBool("ECombo", "Use E", true));
                combo.Add(new MenuBool("RCombo", "Use R", true));
                combo.Add(new MenuSlider("minW", "Min HP % W", 50, 0, 100));
                combo.Add(new MenuSlider("maxW", "Max HP % W", 80, 0, 100));
                combo.Add(new MenuBool("DontQ", "Don't Q at enemy tower", true));
                combo.Add(new MenuBool("Dive", "Dive Tower when target HP is lower then %", true));
                combo.Add(new MenuSlider("DiveMHP", "My HP % to Towerdive", 60, 0, 100));
                combo.Add(new MenuSlider("DiveTHP", "Target HP % to Towerdive", 10, 0, 100));
                combo.Add(new MenuBool("UseItems", "Use Items", true));

                _cfg.Add(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.Add(new MenuBool("HarQ", "Use Q in Harass", false));
                harass.Add(new MenuBool("HarE", "Use E in Harass", true));

                _cfg.Add(harass);
            }

            var laneclear = new Menu("LaneClear", "LaneClear");
            {
                laneclear.Add(new MenuBool("UseQLane", "Use Q", false));
                laneclear.Add(new MenuBool("UseWLane", "Use W", true));
                laneclear.Add(new MenuBool("UseELane", "Use E", true));
                _cfg.Add(laneclear);
            }

            var jungleclear = new Menu("JungleClear", "JungleClear");
            {
                jungleclear.Add(new MenuBool("UseQJungle", "Use Q", true));
                jungleclear.Add(new MenuBool("UseWJungle", "Use W", true));
                jungleclear.Add(new MenuBool("UseEJungle", "Use E", true));
                _cfg.Add(jungleclear);
            }

            var killsteal = new Menu("KillSteal", "KillSteal");
            {
                killsteal.Add(new MenuBool("SmartKS", "Smart KillSteal", true));
                killsteal.Add(new MenuBool("RKS", "Use R in KS", false));

                _cfg.Add(killsteal);
            }

            var drawings = new Menu("Drawings", "Drawings");
            {
                drawings.Add(new MenuBool("DrawQ", "Draw Q", true));
                drawings.Add(new MenuBool("DrawE", "Draw E", true));
                _cfg.Add(drawings);
            }

            var misc = new Menu("Misc", "Misc");
            {
                misc.Add(new MenuBool("TowerQ", "Auto Q Under Turret", false));
                misc.Add(new MenuBool("IntQ", "Auto Interrupt with Q", false));
                misc.Add(new MenuBool("IntMed", "Interrupt Medium Danger Spells", false));
                misc.Add(new MenuBool("SmartW", "Smart W Logic", true));

                _cfg.Add(misc);
            }

            var flee = new Menu("Flee", "Flee");
            {
                flee.Add(new MenuKeyBind("ActiveFlee", "Flee!", Keys.G, KeyBindType.Press));

                _cfg.Add(flee);
            }
            _cfg.Attach();
        }

    }
}
