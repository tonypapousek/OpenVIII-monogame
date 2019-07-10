﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public partial class BattleMenus : Menus
    {

        #region Fields

        private int _player = 0;
        private Dictionary<Mode, Action> DrawActions;
        private Dictionary<Mode, Func<bool>> InputFunctions;
        private Dictionary<Mode, Func<bool>> UpdateFunctions;

        #endregion Fields

        //private Mode _mode = Mode.Starting;

        #region Enums

        public enum Mode : byte
        {
            Starting,
            Battle,
            Victory,
            GameOver,
        }

        private enum SectionName : byte
        {
            HP
        }

        #endregion Enums

        #region Methods

        public override void Draw()
        {
            StartDraw();
            DrawData();
            //menus?.ForEach(m => m.DrawData());
            EndDraw();
        }

        public override void DrawData()
        {
            if (DrawActions.ContainsKey((Mode)GetMode()))
                DrawActions[(Mode)GetMode()]();
            base.DrawData();
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputFunctions.ContainsKey((Mode)GetMode()))
                ret = InputFunctions[(Mode)GetMode()]() && ret;
            return ret;
        }

        public override void ReInit()
        {
            if (Memory.State?.Characters != null)
            {
                IEnumerable<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank));
                int count = party.Count();
                menus = new List<Menu>(count);
                foreach (KeyValuePair<int, Characters> m in party)
                {
                    BattleMenu tmp = new BattleMenu(Memory.State.PartyData[m.Key], m.Value);
                    tmp.Hide();
                    menus.Add(tmp);
                }
                menus.Add(new VictoryMenu());
                SetMode(Mode.Victory);
                UpdateFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    {Mode.Starting, UpdateStartingFunction},
                    {Mode.Battle, UpdateBattleFunction},
                    {Mode.Victory, UpdateVictoryFunction},
                    {Mode.GameOver, UpdateGameOverFunction},
                };
                DrawActions = new Dictionary<Mode, Action>()
                {
                    {Mode.Starting, DrawStartingAction},
                    {Mode.Battle, DrawBattleAction},
                    {Mode.Victory, DrawVictoryAction},
                    //{Mode.GameOver, DrawGameOverAction},
                };
                InputFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    //{Mode.Starting, InputStartingFunction},
                    {Mode.Battle, InputBattleFunction},
                    //{Mode.Victory, InputVictoryFunction},
                    //{Mode.GameOver, InputGameOverFunction},
                };
                menus?.ForEach(m => m.Show());
            }
            base.ReInit();
        }

        public override bool Update()
        {
            bool ret = false;
            if (UpdateFunctions.ContainsKey((Mode)GetMode()))
                UpdateFunctions[(Mode)GetMode()]();
            ret = base.Update() || ret;
            return ret;
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 881, Y = 636 };
            Data = new Dictionary<Enum, IGMData>()
            {
                //{SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X-389),507,389,126))}
            };
            base.Init();
        }

        private void DrawBattleAction() => menus?.Where(m=>m.GetType().Equals(typeof(BattleMenu))).ForEach(m => m.Draw());

        private void DrawGameOverAction() {}

        private void DrawStartingAction() { }

        private void DrawVictoryAction() => Victory_Menu.DrawData();

        public VictoryMenu Victory_Menu => (VictoryMenu)(menus?.Where(m => m.GetType().Equals(typeof(VictoryMenu))).First());
        private bool InputBattleFunction()
        {
            bool ret = false;
            foreach (var m in menus.Where(m => (BattleMenu.Mode)m.GetMode() == BattleMenu.Mode.YourTurn))
            {
                ret = m.Inputs() || ret;
                if (ret) return ret;
            }
            if (Input.Button(Buttons.Cancel))
            {
                switch ((BattleMenu.Mode)menus[_player].GetMode())
                {
                    case BattleMenu.Mode.YourTurn:
                        menus[_player].SetMode(BattleMenu.Mode.ATB_Charged);
                        break;
                }
                if (++_player > 2) _player = 0;
                init_debugger_Audio.PlaySound(14);
                menus[_player].SetMode(BattleMenu.Mode.YourTurn);
                switch ((BattleMenu.Mode)menus[_player].GetMode())
                {
                    case BattleMenu.Mode.ATB_Charged:
                        menus[_player].SetMode(BattleMenu.Mode.YourTurn);
                        break;
                }
            }
            return ret;
        }

        private bool InputGameOverFunction() => false;
        private bool InputStartingFunction() => false;

        private bool InputVictoryFunction() => throw new NotImplementedException();
        private bool UpdateBattleFunction()
        {
            menus?[_player].SetMode(BattleMenu.Mode.YourTurn);
            bool ret = false;
            foreach (var m in menus)
            {
                ret = m.Update() || ret;
            }
            return ret;
        }
        
        private bool UpdateGameOverFunction()
        {
            Memory.module = Memory.MODULE_FIELD_DEBUG;
            Memory.FieldHolder.FieldID = 75; //gover
            init_debugger_Audio.PlayMusic(0);
            Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
            return true;
        }

        private bool UpdateStartingFunction() => throw new NotImplementedException();

        private bool UpdateVictoryFunction()
        {
            init_debugger_Audio.PlayMusic(1);
            return false;
        }
        #endregion Methods

    }
}