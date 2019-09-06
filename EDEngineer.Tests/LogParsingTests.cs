using System;
using System.Collections.Generic;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Utils;
using Moq;
using Newtonsoft.Json.Linq;
using NFluent;
using NUnit.Framework;

namespace EDEngineer.Tests
{
    [TestFixture]
    public class LogParsingTests
    {
        [Test]
        public void Can_convert_loadout()
        {
            #region json
            const string loadout = @"{
              ""timestamp"": ""2019-04-23T20:14:19Z"",
              ""event"": ""Loadout"",
              ""Ship"": ""krait_light"",
              ""ShipID"": 20,
              ""ShipName"": ""Astrolabe"",
              ""ShipIdent"": ""IC-22K"",
              ""HullValue"": 31430968,
              ""ModulesValue"": 50816876,
              ""HullHealth"": 1.0,
              ""UnladenMass"": 329.269989,
              ""CargoCapacity"": 32,
              ""MaxJumpRange"": 69.896164,
              ""FuelCapacity"": {
                ""Main"": 32.0,
                ""Reserve"": 0.63
              },
              ""Rebuy"": 4112393,
              ""Modules"": [
                {
                  ""Slot"": ""ShipCockpit"",
                  ""Item"": ""krait_light_cockpit"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""CargoHatch"",
                  ""Item"": ""modularcargobaydoor"",
                  ""On"": true,
                  ""Priority"": 2,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""MediumHardpoint2"",
                  ""Item"": ""hpt_pulselaser_fixed_small"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Broo Tarquin"",
                    ""EngineerID"": 300030,
                    ""BlueprintID"": 128673574,
                    ""BlueprintName"": ""Weapon_LightWeight"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""ExperimentalEffect"": ""special_weapon_lightweight"",
                    ""ExperimentalEffect_Localised"": ""Stripped Down"",
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 0.18,
                        ""OriginalValue"": 2.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 15.999999,
                        ""OriginalValue"": 40.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""PowerDraw"",
                        ""Value"": 0.234,
                        ""OriginalValue"": 0.39,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""DistributorDraw"",
                        ""Value"": 0.195,
                        ""OriginalValue"": 0.3,
                        ""LessIsGood"": 1
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""TinyHardpoint1"",
                  ""Item"": ""hpt_heatsinklauncher_turret_tiny"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""AmmoInClip"": 1,
                  ""AmmoInHopper"": 2,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Ram Tah"",
                    ""EngineerID"": 300110,
                    ""BlueprintID"": 128731475,
                    ""BlueprintName"": ""Misc_LightWeight"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 0.195,
                        ""OriginalValue"": 1.3,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 22.5,
                        ""OriginalValue"": 45.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""TinyHardpoint2"",
                  ""Item"": ""hpt_heatsinklauncher_turret_tiny"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""AmmoInClip"": 1,
                  ""AmmoInHopper"": 2,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Ram Tah"",
                    ""EngineerID"": 300110,
                    ""BlueprintID"": 128731475,
                    ""BlueprintName"": ""Misc_LightWeight"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 0.195,
                        ""OriginalValue"": 1.3,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 22.5,
                        ""OriginalValue"": 45.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""TinyHardpoint3"",
                  ""Item"": ""hpt_cloudscanner_size0_class5"",
                  ""On"": true,
                  ""Priority"": 3,
                  ""Health"": 1.0,
                  ""Engineering"": {}
                },
                {
                  ""Slot"": ""TinyHardpoint4"",
                  ""Item"": ""hpt_xenoscanner_basic_tiny"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""PaintJob"",
                  ""Item"": ""paintjob_krait_light_tactical_white"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Armour"",
                  ""Item"": ""krait_light_armour_grade1"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Selene Jean"",
                    ""EngineerID"": 300210,
                    ""BlueprintID"": 128673634,
                    ""BlueprintName"": ""Armour_Advanced"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""ExperimentalEffect"": ""special_armour_chunky"",
                    ""ExperimentalEffect_Localised"": ""Deep Plating"",
                    ""Modifiers"": [
                      {
                        ""Label"": ""DefenceModifierHealthMultiplier"",
                        ""Value"": 84.68,
                        ""OriginalValue"": 79.999992,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""KineticResistance"",
                        ""Value"": -5.060005,
                        ""OriginalValue"": -20.000004,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""ThermicResistance"",
                        ""Value"": 12.449998,
                        ""OriginalValue"": 0.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""ExplosiveResistance"",
                        ""Value"": -22.570002,
                        ""OriginalValue"": -39.999996,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""PowerPlant"",
                  ""Item"": ""int_guardianpowerplant_size4"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""MainEngines"",
                  ""Item"": ""int_engine_size4_class2"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Professor Palin"",
                    ""EngineerID"": 300220,
                    ""BlueprintID"": 128673659,
                    ""BlueprintName"": ""Engine_Dirty"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""ExperimentalEffect"": ""special_engine_overloaded"",
                    ""ExperimentalEffect_Localised"": ""Drag Drives"",
                    ""Modifiers"": [
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 54.400002,
                        ""OriginalValue"": 64.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""PowerDraw"",
                        ""Value"": 4.1328,
                        ""OriginalValue"": 3.69,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""EngineOptimalMass"",
                        ""Value"": 275.625,
                        ""OriginalValue"": 315.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""EngineOptPerformance"",
                        ""Value"": 145.600006,
                        ""OriginalValue"": 100.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""EngineHeatRate"",
                        ""Value"": 2.288,
                        ""OriginalValue"": 1.3,
                        ""LessIsGood"": 1
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""FrameShiftDrive"",
                  ""Item"": ""int_hyperdrive_size5_class5"",
                  ""On"": true,
                  ""Priority"": 2,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Felicity Farseer"",
                    ""EngineerID"": 300100,
                    ""BlueprintID"": 128673694,
                    ""BlueprintName"": ""FSD_LongRange"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""ExperimentalEffect"": ""special_fsd_heavy"",
                    ""ExperimentalEffect_Localised"": ""Mass Manager"",
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 26.0,
                        ""OriginalValue"": 20.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 93.840004,
                        ""OriginalValue"": 120.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""PowerDraw"",
                        ""Value"": 0.69,
                        ""OriginalValue"": 0.6,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""FSDOptimalMass"",
                        ""Value"": 1692.599976,
                        ""OriginalValue"": 1050.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""LifeSupport"",
                  ""Item"": ""int_lifesupport_size4_class2"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Lori Jameson"",
                    ""EngineerID"": 300230,
                    ""BlueprintID"": 128731494,
                    ""BlueprintName"": ""Misc_LightWeight"",
                    ""Level"": 4,
                    ""Quality"": 1.0,
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 1.0,
                        ""OriginalValue"": 4.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 43.200001,
                        ""OriginalValue"": 72.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""PowerDistributor"",
                  ""Item"": ""int_guardianpowerdistributor_size4"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Radar"",
                  ""Item"": ""int_sensors_size6_class2"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Lori Jameson"",
                    ""EngineerID"": 300230,
                    ""BlueprintID"": 128740673,
                    ""BlueprintName"": ""Sensor_LightWeight"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 3.2,
                        ""OriginalValue"": 16.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""Integrity"",
                        ""Value"": 45.0,
                        ""OriginalValue"": 90.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""SensorTargetScanAngle"",
                        ""Value"": 22.5,
                        ""OriginalValue"": 30.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""FuelTank"",
                  ""Item"": ""int_fueltank_size5_class3"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot01_Size6"",
                  ""Item"": ""int_fuelscoop_size6_class5"",
                  ""On"": true,
                  ""Priority"": 2,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot02_Size5"",
                  ""Item"": ""int_guardianfsdbooster_size5"",
                  ""On"": true,
                  ""Priority"": 2,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot03_Size5"",
                  ""Item"": ""int_cargorack_size5_class1"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot04_Size5"",
                  ""Item"": ""int_repairer_size5_class5"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot05_Size3"",
                  ""Item"": ""int_shieldgenerator_size3_class2"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Lei Cheung"",
                    ""EngineerID"": 300120,
                    ""BlueprintID"": 128673839,
                    ""BlueprintName"": ""ShieldGenerator_Reinforced"",
                    ""Level"": 5,
                    ""Quality"": 1.0,
                    ""ExperimentalEffect"": ""special_shield_lightweight"",
                    ""ExperimentalEffect_Localised"": ""Stripped Down"",
                    ""Modifiers"": [
                      {
                        ""Label"": ""Mass"",
                        ""Value"": 1.8,
                        ""OriginalValue"": 2.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""ShieldGenStrength"",
                        ""Value"": 124.199997,
                        ""OriginalValue"": 90.0,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""BrokenRegenRate"",
                        ""Value"": 1.683,
                        ""OriginalValue"": 1.87,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""EnergyPerRegen"",
                        ""Value"": 0.672,
                        ""OriginalValue"": 0.6,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""KineticResistance"",
                        ""Value"": 49.900002,
                        ""OriginalValue"": 39.999996,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""ThermicResistance"",
                        ""Value"": -0.199997,
                        ""OriginalValue"": -20.000004,
                        ""LessIsGood"": 0
                      },
                      {
                        ""Label"": ""ExplosiveResistance"",
                        ""Value"": 58.25,
                        ""OriginalValue"": 50.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""Slot06_Size3"",
                  ""Item"": ""int_repairer_size3_class5"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot07_Size3"",
                  ""Item"": ""int_dronecontrol_repair_size3_class5"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""Slot08_Size2"",
                  ""Item"": ""int_detailedsurfacescanner_tiny"",
                  ""On"": true,
                  ""Priority"": 0,
                  ""Health"": 1.0,
                  ""Engineering"": {
                    ""Engineer"": ""Lori Jameson"",
                    ""EngineerID"": 300230,
                    ""BlueprintID"": 128740151,
                    ""BlueprintName"": ""Sensor_Expanded"",
                    ""Level"": 5,
                    ""Quality"": 0.986,
                    ""Modifiers"": [
                      {
                        ""Label"": ""PowerDraw"",
                        ""Value"": 0.0,
                        ""OriginalValue"": 0.0,
                        ""LessIsGood"": 1
                      },
                      {
                        ""Label"": ""DSS_PatchRadius"",
                        ""Value"": 29.972,
                        ""OriginalValue"": 20.0,
                        ""LessIsGood"": 0
                      }
                    ]
                  }
                },
                {
                  ""Slot"": ""PlanetaryApproachSuite"",
                  ""Item"": ""int_planetapproachsuite"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""ShipKitSpoiler"",
                  ""Item"": ""krait_light_shipkit1_spoiler3"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""ShipKitWings"",
                  ""Item"": ""krait_light_shipkit1_wings3"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""ShipKitTail"",
                  ""Item"": ""krait_light_shipkit1_tail3"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""ShipKitBumper"",
                  ""Item"": ""krait_light_shipkit1_bumper3"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""WeaponColour"",
                  ""Item"": ""weaponcustomisation_white"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""EngineColour"",
                  ""Item"": ""enginecustomisation_white"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                },
                {
                  ""Slot"": ""VesselVoice"",
                  ""Item"": ""voicepack_archer"",
                  ""On"": true,
                  ""Priority"": 1,
                  ""Health"": 1.0
                }
              ]
            }";
            #endregion

            var converter = new JournalEntryConverter(new ItemNameConverter(new List<EntryData>()),
                Mock.Of<ISimpleDictionary<String, Entry>>(), new Languages(), new List<Blueprint>());

            var jObject = JObject.Parse(loadout);

            Check.ThatCode(() => converter.ExtractOperation(jObject, JournalEvent.Loadout)).DoesNotThrow();
        }
    }
}