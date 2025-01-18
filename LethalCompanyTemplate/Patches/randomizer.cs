﻿using System;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;
using Random = System.Random;



namespace Randomizer.Patches
{
	public class ChangeScrap
	{
		public static SelectableLevel PickedLevel;

		[HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
		[HarmonyPrefix]
		public static void RandomizeScrap(RoundManager __instance)
		{
			if (StartOfRound.Instance.currentLevel.levelID == 3 && StartOfRound.Instance.currentLevel.PlanetName == "71 Gordion")
			{
				PickedLevel = StartOfRound.Instance.levels[2];
				return;
			};
			Random random = new Random();
			int num = random.Next(0, __instance.playersManager.levels.Length);
			while (__instance.playersManager.levels[num].PlanetName == "71 Gordion")
			{
				int num2 = random.Next(0, __instance.playersManager.levels.Length);
				num = num2;
			}

			__instance.currentLevel = __instance.playersManager.levels[num];
			PickedLevel = __instance.currentLevel;
		}
	}
	public class ChangeMapObjects
	{

		// private static SelectableLevel _actualCurrentLevel;
		
		
		[HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
		[HarmonyPrefix]
		public static void ChangeLevel(RoundManager __instance)
		{
			SelectableLevel pickedLevel = ChangeScrap.PickedLevel;
			__instance.currentLevel = pickedLevel;
		}
		[HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
		[HarmonyPrefix]
		public static void ChangeBackPropsAtEndOfRound(RoundManager __instance)
		{
			__instance.currentLevel.levelID = StartOfRound.Instance.currentLevel.levelID;
		}

		
	}
	public class ChangeOutsideEnemies
	{
		public static SelectableLevel ORLevel = RandomLevel();

		public static SelectableLevel RandomLevel()
		{
			Random random = new Random();
			int num = random.Next(0, RoundManager.Instance.playersManager.levels.Length);
			return RoundManager.Instance.playersManager.levels[num];
		}

		[HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
		[HarmonyPostfix]
		public static void ChangeRandomNumber(RoundManager __instance)
		{
			ORLevel = __instance.currentLevel.PlanetName != "71 Gordion" ? RandomLevel() : __instance.currentLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "PredictAllOutsideEnemies")]
		[HarmonyPrefix]
		public static void ChangeOutsideEnemiesInGame(RoundManager __instance)
		{
			__instance.currentLevel = ORLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "SpawnEnemiesOutside")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemies(RoundManager __instance)
		{
			__instance.currentLevel = ORLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemy(RoundManager __instance)
		{
			__instance.currentLevel = ORLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxOutsideEnemyPowerCount = ORLevel.maxOutsideEnemyPowerCount;
		}
		
	}
	public class ChangeInsideEnemies
	{
		public static SelectableLevel RLevel = RandomLevel();

		private static SelectableLevel RandomLevel()
		{
			Random random = new Random();
			int num = random.Next(0, RoundManager.Instance.playersManager.levels.Length);
			return RoundManager.Instance.playersManager.levels[num];
		}

		[HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
		[HarmonyPrefix]
		public static void ChangeRandomNumber(RoundManager __instance)
		{
			
			RLevel = __instance.currentLevel.PlanetName != "71 Gordion" ? RandomLevel() : __instance.currentLevel;
			
		}
// LLL issue was found here
		[HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
		[HarmonyPrefix]
		public static void PlotOutEnemiesForNextHour(RoundManager __instance)
		{
			__instance.currentLevel.enemySpawnChanceThroughoutDay = RLevel.enemySpawnChanceThroughoutDay;
			__instance.currentLevel.spawnProbabilityRange = RLevel.spawnProbabilityRange;
			
		}
		
		[HarmonyPatch(typeof(RoundManager), "AssignRandomEnemyToVent")]
		[HarmonyPrefix]
		public static void AssignRandomEnemyToVent(RoundManager __instance)
		{
			__instance.currentLevel = RLevel;
		}
		
		[HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
		[HarmonyPrefix]
		public static void EnemyCannotBeSpawned(RoundManager __instance)
		{
			__instance.currentLevel = RLevel;
		}
		// LLL issue was found here
		[HarmonyPatch(typeof(RoundManager), "BeginEnemySpawning")]
		[HarmonyPrefix]
		public static void BeginEnemySpawning(RoundManager __instance)
		{
			__instance.currentLevel.maxEnemyPowerCount = RLevel.maxEnemyPowerCount;
		}
		
		[HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxEnemyPowerCount = RLevel.maxEnemyPowerCount;
		}
	}
	public class ChangeText
	{
		private static LNetworkMessage<string> EventMessage = LNetworkMessage<string>.Connect("ServerSendAnnouncment");
		private static string pickedPlanetScrap;
		private static string pickedPlanetEnemies;
		private static string pickedPlanetOutside;

// Static constructor to ensure the handler is registered only once
	
		
		

		[HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
		[HarmonyPrefix]
		public static void SendPlanets(StartOfRound __instance)
		{
			try
			{

				EventMessage.OnClientReceived += EventMessageOnOnClientReceived;
				// Validate the data
				var scrap = ChangeScrap.PickedLevel?.PlanetName ?? "Unknown";
				var enemies = ChangeInsideEnemies.RLevel?.PlanetName ?? "Unknown";
				var outside = ChangeOutsideEnemies.ORLevel?.PlanetName ?? "Unknown";

				// Send the message to clients
				EventMessage.SendClients($"{scrap}/{enemies}/{outside}");
				
				EventMessage.OnClientReceived -= EventMessageOnOnClientReceived;
			}
			catch (Exception e)
			{
				Debug.LogError($"Error in SendPlanets: {e.Message}\n{e.StackTrace}");
			}
		}

		[HarmonyPatch(typeof(StartOfRound), "WritePlayerNotes")]
		[HarmonyPostfix]
		public static void Update(StartOfRound __instance)
		{
		
		try
		{
			__instance.gameStats.allPlayerStats[0].playerNotes.Clear();
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("S:" + pickedPlanetScrap);
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("IE:" + pickedPlanetEnemies);
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("OE:" + pickedPlanetOutside);
		}
		catch (Exception ex)
		{
			Debug.Log((object)ex);
		}
		}
		private static void EventMessageOnOnClientReceived(string obj)
		{
			
			string[] pickedPlanets = obj.Split('/');
			
			Debug.Log("EventMessageOnOnClientReceived: " + pickedPlanets[0] + "/" + pickedPlanets[1] + "/" + pickedPlanets[2]);

			pickedPlanetScrap = pickedPlanets[0];
			pickedPlanetEnemies = pickedPlanets[1];
			pickedPlanetOutside = pickedPlanets[2];
			
		}

	}
}