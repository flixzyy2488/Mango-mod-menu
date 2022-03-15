using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using ModMenuPatch.HarmonyPatches;
using UnityEngine;
using Utilla;
using System.ComponentModel;

namespace ModMenuPatch
{
    [Description("HauntedModMenu")]
    [BepInPlugin("org.legoandmars.gorillatag.modmenupatch", "Mod Menu Patch", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    public class ModMenuPatch : BaseUnityPlugin
    {
        public static bool allowSpaceMonke = true;
        public static ConfigEntry<float> multiplier;

        public static ConfigEntry<float> speedMultiplier;
        public static ConfigEntry<float> jumpMultiplier;

        void OnEnable()
        {
            ModMenuPatches.ApplyHarmonyPatches();

            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "ModMonkeyPatch.cfg"), true);
            //multiplier = customFile.Bind("Configuration", "JumpMultiplier", 10f, "How much to multiply the jump height/distance by. 10 = 10x higher jumps");
            
            speedMultiplier = customFile.Bind("Configuration", "SpeedMultiplier", 100f, "How much to multiply the speed. 10 = 10x higher jumps");
            jumpMultiplier = customFile.Bind("Configuration", "JumpMultiplier", 1.30f, "How much to multiply the jump height/distance by. 10 = 10x higher jumps");
        }

        void OnDisable()
        {
            ModMenuPatches.RemoveHarmonyPatches();
        }

        [ModdedGamemodeJoin]
        private void RoomJoined()
		{
            allowSpaceMonke = true;
		}

        [ModdedGamemodeLeave]
        private void RoomLeft()
		{
            allowSpaceMonke = true;
		}
    }
}
