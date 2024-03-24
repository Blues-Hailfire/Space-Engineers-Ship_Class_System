﻿using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

//TODO better unknown config handling

namespace ShipClassSystem.Data.Scripts.ShipClassSystem
{
    public class ModConfig
    {
        private readonly Dictionary<long, GridClass> _gridClassesById = new Dictionary<long, GridClass>();

        private GridClass _defaultGridClass = DefaultGridClassConfig.DefaultGridClassDefinition;

        private GridClass[] _gridClasses;
        public string[] IgnoreFactionTags = Array.Empty<string>();

        public bool IncludeAiFactions = false;

        public GridClass DefaultGridClass
        {
            get { return _defaultGridClass; }
            set
            {
                _defaultGridClass = value;
                UpdateGridClassesDictionary();
            }
        }

        public GridClass[] GridClasses
        {
            get { return _gridClasses; }
            set
            {
                _gridClasses = value;
                UpdateGridClassesDictionary();
            }
        }

        public GridClass GetGridClassById(long gridClassId)
        {
            GridClass id;
            if (_gridClassesById.TryGetValue(gridClassId, out id)) return id;

            Utils.Log($"Unknown grid class {gridClassId}, using default grid class");

            return DefaultGridClass;
        }

        public bool IsValidGridClassId(long gridClassId)
        {
            return _gridClassesById.ContainsKey(gridClassId);
        }

        private void UpdateGridClassesDictionary()
        {
            _gridClassesById.Clear();

            if (_defaultGridClass != null)
                _gridClassesById[0] = DefaultGridClass;
            else
                _gridClassesById[0] = DefaultGridClassConfig.DefaultGridClassDefinition;

            if (_gridClasses == null) return;
            foreach (var gridClass in _gridClasses)
                _gridClassesById[gridClass.Id] = gridClass;
        }

        public static ModConfig LoadConfig()
        {
            var config = DefaultGridClassConfig.DefaultModConfig;
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Constants.ConfigFilename, typeof(ModConfig)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Constants.ConfigFilename, typeof(ModConfig));
                    var myText = reader.ReadToEnd();
                    reader.Close();
                    config = MyAPIGateway.Utilities.SerializeFromXML<ModConfig>(myText);
                    if (config == null) { throw new Exception("Word Settings Empty! :(.... \n ...Fixed!"); }
                }

            }
            catch (Exception x)
            {
                config = DefaultGridClassConfig.DefaultModConfig;
                MyAPIGateway.Utilities.ShowMessage("Debug", $"ConfigLoad crashed because...\n{x.Message}");
            }

            return config;
        }

        public static void SaveConfig(ModConfig config, string filename)
        {
            try
            {
                var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(ModConfig));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                writer.Close();
            }
            catch (Exception e)
            {
                Utils.Log($"Failed to save ModConfig file {filename}, reason {e.Message}", 3);
            }
        }
    }

    [ProtoContract]
    public class GridClass
    {
        [ProtoMember(1)]
        public int Id;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public bool ForceBroadCast = false;
        [ProtoMember(4)]
        public float ForceBroadCastRange = 0;
        [ProtoMember(5)]
        public bool LargeGridStatic = false;
        [ProtoMember(6)]
        public bool LargeGridMobile = false;
        [ProtoMember(7)]
        public bool SmallGrid = false;
        [ProtoMember(8)]
        public int MaxBlocks = -1;
        [ProtoMember(9)]
        public float MaxMass = -1;
        [ProtoMember(10)]
        public int MaxPCU = -1;
        [ProtoMember(11)]
        public int MaxPerFaction = -1;
        [ProtoMember(12)]
        public int MaxPerPlayer = -1;
        [ProtoMember(13)]
        public int MinBlocks = -1;
        [ProtoMember(14)]
        public GridModifiers Modifiers = new GridModifiers();
        [ProtoMember(15)]
        public GridDamageModifiers DamageModifiers = new GridDamageModifiers();
        [ProtoMember(16)]
        public BlockLimit[] BlockLimits;
    }

    [ProtoContract]
    public class GridModifiers
    {
        [ProtoMember(1)]
        public float AssemblerSpeed = 1;
        [ProtoMember(2)]
        public float DrillHarvestMultiplier = 1;
        [ProtoMember(3)]
        public float GyroEfficiency = 1;
        [ProtoMember(4)]
        public float GyroForce = 1;
        [ProtoMember(5)]
        public float PowerProducersOutput = 1;
        [ProtoMember(6)]
        public float RefineEfficiency = 1;
        [ProtoMember(7)]
        public float RefineSpeed = 1;
        [ProtoMember(8)]
        public float ThrusterEfficiency = 1;
        [ProtoMember(9)]
        public float ThrusterForce = 1;

        public override string ToString()
        {
            return
                $"<GridModifiers ThrusterForce={ThrusterForce} ThrusterEfficiency={ThrusterEfficiency} GyroForce={GyroForce} GyroEfficiency={GyroEfficiency} RefineEfficiency={RefineEfficiency} RefineSpeed={RefineSpeed} AssemblerSpeed={AssemblerSpeed} PowerProducersOutput={PowerProducersOutput} DrillHarvestMutiplier={DrillHarvestMultiplier} />";
        }

        public IEnumerable<ModifierNameValue> GetModifierValues()
        {
            yield return new ModifierNameValue("Thruster force", ThrusterForce);
            yield return new ModifierNameValue("Thruster efficiency", ThrusterEfficiency);
            yield return new ModifierNameValue("Gyro force", GyroForce);
            yield return new ModifierNameValue("Gyro efficiency", GyroEfficiency);
            yield return new ModifierNameValue("Refinery efficiency", RefineEfficiency);
            yield return new ModifierNameValue("Refinery speed", RefineSpeed);
            yield return new ModifierNameValue("Assembler speed", AssemblerSpeed);
            yield return new ModifierNameValue("Power output", PowerProducersOutput);
            yield return new ModifierNameValue("Drill harvest", DrillHarvestMultiplier);
        }
    }

    public struct ModifierNameValue
    {
        public string Name;
        public float Value;

        public ModifierNameValue(string name, float value)
        {
            Name = name;
            Value = value;
        }
    }

    [ProtoContract]
    public class BlockLimit
    {
        [ProtoMember(1)] 
        public string Name;

        [ProtoMember(2)] 
        public BlockType[] BlockTypes;

        [ProtoMember(3)] 
        public float MaxCount;

        public bool IsLimitedBlock(IMyFunctionalBlock block, out float blockCountWeight)
        {
            blockCountWeight = 0;

            foreach (var blockType in BlockTypes)
                if (blockType.IsBlockOfType(block))
                {
                    blockCountWeight = blockType.CountWeight;

                    return true;
                }

            return false;
        }
    }


    [ProtoContract]
    public class BlockType
    {
        [ProtoMember(1)] 
        public string TypeId;

        [ProtoMember(2)] 
        public string SubtypeId;

        [ProtoMember(3)] 
        public float CountWeight;

        public BlockType()
        {
        }

        public BlockType(string typeId, string subtypeId = "", float countWeight = 1)
        {
            TypeId = typeId;
            SubtypeId = subtypeId;
            CountWeight = countWeight;
        }

        public bool IsBlockOfType(IMyFunctionalBlock block)
        {
            return Utils.GetBlockId(block) == TypeId && (string.IsNullOrEmpty(SubtypeId) ||
                                                         Convert.ToString(block.BlockDefinition.SubtypeId) ==
                                                         SubtypeId);
        }
    }

    [ProtoContract]
    public class GridDamageModifiers
    {
        [ProtoMember(1)]
        public float Bullet = 1f;
        [ProtoMember(2)]
        public float Rocket = 1f;
        [ProtoMember(3)]
        public float Explosion = 1f;
        [ProtoMember(4)]
        public float Environment = 1f;
        [ProtoMember(5)]
        public float Energy = 1f;
        [ProtoMember(6)]
        public float Kinetic = 1f;
    }
}