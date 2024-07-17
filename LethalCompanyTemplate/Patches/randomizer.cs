using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity;
using Unity.Netcode;
using LethalCompanyRandomizer;
using UnityEngine.Subsystems;
using System.Security.Cryptography;
using BepInEx.Logging;

namespace Randomizer.Patches
{
    public class changescrap
    {

        public static SelectableLevel PickedLevel;

        [HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]



        public static void RandomizeScrap(RoundManager __instance)
        {

            var random = new System.Random();
            int randomNumber = random.Next(0, __instance.playersManager.levels.Length - 1);

            //UnityEngine.Debug.Log(__instance.currentLevel.PlanetName);

                while (__instance.playersManager.levels[randomNumber].PlanetName == "71 Gordion")
                {
                    //UnityEngine.Debug.Log("Was Gordion");
                    int updatedNumber = random.Next(0, __instance.playersManager.levels.Length - 1);
                    randomNumber = updatedNumber;
                }

            __instance.currentLevel = __instance.playersManager.levels[randomNumber];

            PickedLevel = __instance.currentLevel;

            //UnityEngine.Debug.Log(__instance.currentLevel.PlanetName);

        }


    }
    public class changemapobjects
    {

        [HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
        [HarmonyPrefix]

        public static void changeLevel(RoundManager __instance)
        {

            var PickedLevel = changescrap.PickedLevel;
            __instance.currentLevel = PickedLevel;

        }


    }
    public class changeoutsideenemies
    {

        public static SelectableLevel RandomLevel()
        {
            var random = new System.Random();
            int randomNumber = random.Next(0, RoundManager.Instance.playersManager.levels.Length - 1);

            return RoundManager.Instance.playersManager.levels[randomNumber];
        }


        public static SelectableLevel ORLevel = RandomLevel();

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]


        public static void ChangeRandomNumber(RoundManager __instance)
        {

            ORLevel = RandomLevel();

        }

        [HarmonyPatch(typeof(RoundManager), "PredictAllOutsideEnemies")]
        [HarmonyPrefix]


        public static void ChangeOutsideEnemies(RoundManager __instance)
        {         

            __instance.currentLevel =  ORLevel;
            //UnityEngine.Debug.Log("Outside Enemies: " + __instance.currentLevel.PlanetName);

        }

        [HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
        [HarmonyPrefix]


        public static void SpawnOutsideEnemy(RoundManager __instance)
        {
            __instance.currentLevel = ORLevel;
            //UnityEngine.Debug.Log("Outside Enemies: " + __instance.currentLevel.PlanetName);
        }

        public static void RefreshEnemyList(RoundManager __instance)
        {
            __instance.currentLevel = ORLevel;
            //UnityEngine.Debug.Log("Outside Enemies: " + __instance.currentLevel.PlanetName);
        }


    }

    public class ChangeInsideEnemies
    {

        public static SelectableLevel RandomLevel()
        {

            var random = new System.Random();
            int randomNumber = random.Next(0, RoundManager.Instance.playersManager.levels.Length - 1);

            return RoundManager.Instance.playersManager.levels[randomNumber];
        }

        public static SelectableLevel RLevel = RandomLevel();

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]

        public static void ChangeRandomNumber(RoundManager __instance)
        {
            RLevel = RandomLevel();

        }

        [HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
        [HarmonyPrefix]

        public static void PlotOutEnemiesForNextHour(RoundManager __instance)
        {
             __instance.currentLevel = RLevel;
            //UnityEngine.Debug.Log("Inside Enemies are " + __instance.currentLevel.PlanetName);
        }

        [HarmonyPatch(typeof(RoundManager), "AssignRandomEnemyToVent")]
        [HarmonyPrefix]

        public static void AssignRandomEnemyToVent(RoundManager __instance)
        {
            __instance.currentLevel = RLevel;
            //UnityEngine.Debug.Log("Inside Enemies are " + __instance.currentLevel.PlanetName);
        }

        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]

        public static void EnemyCannotBeSpawned(RoundManager __instance)
        {
            __instance.currentLevel = RLevel;
            //UnityEngine.Debug.Log("Inside Enemies are " + __instance.currentLevel.PlanetName);
        }

        [HarmonyPatch(typeof(RoundManager), "BeginEnemySpawning")]
        [HarmonyPrefix]

        public static void BeginEnemySpawning(RoundManager __instance)
        {
            __instance.currentLevel = RLevel;
            //UnityEngine.Debug.Log("Inside Enemies are " + __instance.currentLevel.PlanetName);

        }
        public static void RefreshEnemyList(RoundManager __instance)
        {
            __instance.currentLevel = RLevel;
            //UnityEngine.Debug.Log("Inside Enemies: " + __instance.currentLevel.PlanetName);
        }

    }

    public class ChangeText
    {
        [HarmonyPatch(typeof(StartOfRound), "WritePlayerNotes")]
        [HarmonyPostfix]

        public static void Update(StartOfRound __instance)
        {
            __instance.gameStats.allPlayerStats[0].playerNotes.Clear();
            __instance.gameStats.allPlayerStats[0].playerNotes.Add("S:" + changescrap.PickedLevel.PlanetName);
            __instance.gameStats.allPlayerStats[0].playerNotes.Add("IE:" + ChangeInsideEnemies.RLevel.PlanetName);
            __instance.gameStats.allPlayerStats[0].playerNotes.Add("OE:" + changeoutsideenemies.ORLevel.PlanetName);

        }
    }

}
