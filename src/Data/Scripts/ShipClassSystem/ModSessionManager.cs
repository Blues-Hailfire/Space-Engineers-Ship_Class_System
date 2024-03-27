﻿using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Network;

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class ModSessionManager : MySessionComponentBase, IMyEventProxy
    {
        private Comms _comms;

        public ModConfig Config;

        public static ModSessionManager Instance { get; private set; }

        internal static Comms Comms => Instance._comms;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            Instance = this;

            Utils.Log("Init");

            _comms = new Comms(Settings.COMMS_MESSAGE_ID);
            Config = ModConfig.LoadConfig();
            ModConfig.SaveConfig(Config, Constants.ConfigFilename);
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(99, CubeGridModifiers.GridClassDamageHandler);
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdd;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();

            CockpitGUI.AddControls(ModContext);
        }

        private void OnEntityAdd(IMyEntity obj)
        {
            var grid = obj as IMyCubeGrid;
            if (grid == null) return;
            Utils.Log("ADD EVENT: " + grid.EntityId);
        }

        public static string[] GetIgnoredFactionTags()
        {
            return Instance.Config.IgnoreFactionTags;
        }

        public static GridClass GetGridClassById(long gridClassId)
        {
            return Instance.Config.GetGridClassById(gridClassId);
        }

        public static GridClass[] GetAllGridClasses()
        {
            return Instance.Config.GridClasses ?? Array.Empty<GridClass>();
        }

        internal static bool IsValidGridClass(long gridClassId)
        {
            return Instance.Config.IsValidGridClassId(gridClassId);
        }
    }
}