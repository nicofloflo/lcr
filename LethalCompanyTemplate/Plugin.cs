using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Randomizer.Patches;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

namespace LethalCompanyRandomizer
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("LethalNetworkAPI")]
    public class Plugin : BaseUnityPlugin
    {

        private const string PLUGIN_GUID = "Randomizer";
        private const string PLUGIN_NAME = "Lethal Company Randomizer";
        private const string PLUGIN_VERSION = "1.0.4";



        private readonly Harmony harmony = new Harmony(PLUGIN_GUID);


        private void Awake()
        {


            UnityEngine.Debug.Log($"I'm a cowboy");
            harmony.PatchAll(typeof(ChangeScrap));
            harmony.PatchAll(typeof(ChangeMapObjects));
            harmony.PatchAll(typeof(ChangeOutsideEnemies));
            harmony.PatchAll(typeof(ChangeInsideEnemies));
            harmony.PatchAll(typeof(ChangeText));
           


        }

    }
}