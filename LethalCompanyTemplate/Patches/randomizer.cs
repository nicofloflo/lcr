using System;
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

	public class ChooseRandom
	{
		public static SelectableLevel RealLevel;
		public static SelectableLevel ScrapLevel = RandomLevel();
		public static SelectableLevel IndoorEnemiesLevel = RandomLevel();
		public static SelectableLevel OutdoorEnemiesLevel = RandomLevel();
		
		private static SelectableLevel RandomLevel()
		{
			Random random = new Random();
			int num = random.Next(0, RoundManager.Instance.playersManager.levels.Length);
			return RoundManager.Instance.playersManager.levels[num];
		}

		[HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
		[HarmonyPrefix]

		public static void Randomize(RoundManager __instance)
		{
			RealLevel = __instance.currentLevel;
			ScrapLevel = RandomLevel();
			IndoorEnemiesLevel = RandomLevel();
			OutdoorEnemiesLevel = RandomLevel();
			


		}
	}

	public class CheckStartOfRound
	{
		[HarmonyPatch(typeof(StartOfRound), "StartGame")]
		[HarmonyPrefix]
		public static void StartGame(StartOfRound __instance)
		{
			Debug.Log(__instance.currentLevel.PlanetName + " Start of Game Instance Level");
		}

		[HarmonyPatch(typeof(StartOfRound), "EndOfGame")]
		[HarmonyPrefix]
		public static void EndOfGame(StartOfRound __instance)
		{
			Debug.Log(__instance.currentLevel.PlanetName + " End Of Game Instance Level");
		}
	}
	
	public class ChangeScrap
	{
		public static SelectableLevel PickedLevel;

		[HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
		[HarmonyPrefix]
		public static void RandomizeScrap(RoundManager __instance)
		{
		
			Debug.Log(__instance.currentLevel.PlanetName);
			Debug.Log(ChooseRandom.ScrapLevel.PlanetName);
			
			while (ChooseRandom.ScrapLevel.PlanetName == "71 Gordion")
			{
				Random random = new Random();
				int num2 = random.Next(0, __instance.playersManager.levels.Length);
				
				ChooseRandom.ScrapLevel = __instance.playersManager.levels[num2];
				
			}
			
			Debug.Log(ChooseRandom.ScrapLevel.PlanetName);

			__instance.currentLevel = ChooseRandom.ScrapLevel;
			PickedLevel = __instance.currentLevel;
		}

		
			[HarmonyPatch(typeof(RoundManager), "SpawnScrapInLevel")]
			[HarmonyPostfix]
			public static void ChangeBackToOriginalLevel(RoundManager __instance)
			{
				
				__instance.currentLevel = ChooseRandom.RealLevel;
				
			}
		
	}
	public class ChangeMapObjects
	{
		
		[HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
		[HarmonyPrefix]
		public static void ChangeLevel(RoundManager __instance)
		{
			SelectableLevel pickedLevel = ChangeScrap.PickedLevel;
			__instance.currentLevel = pickedLevel;
			
			Debug.Log(__instance.currentLevel.PlanetName);
		}
		[HarmonyPatch(typeof(RoundManager), "SpawnMapObjects")]
		[HarmonyPostfix]
		public static void ChangeLevelPostFix(RoundManager __instance)
		{
			
			__instance.currentLevel = ChooseRandom.RealLevel;
			
			Debug.Log(__instance.currentLevel.PlanetName);
		}
		
		
		
		[HarmonyPatch(typeof(RoundManager), "DespawnPropsAtEndOfRound")]
		[HarmonyPrefix]
		public static void ChangeBackPropsAtEndOfRound(RoundManager __instance)
		{
			
			Debug.Log(StartOfRound.Instance.currentLevel.PlanetName);
			Debug.Log(__instance.currentLevel.PlanetName);
			__instance.currentLevel.levelID = ChooseRandom.RealLevel.levelID;
			Debug.Log(__instance.currentLevel.levelID);
		}
	
		
	}
	public class ChangeOutsideEnemies
	{
		[HarmonyPatch(typeof(RoundManager), "PredictAllOutsideEnemies")]
		[HarmonyPrefix]
		public static void ChangeOutsideEnemiesInGame(RoundManager __instance)
		{
			Debug.Log(__instance.currentMaxOutsidePower);
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
			Debug.Log(__instance.currentMaxOutsidePower);
			Debug.Log(__instance.currentLevel);
		}
		
		[HarmonyPatch(typeof(RoundManager), "PredictAllOutsideEnemies")]
		[HarmonyPostfix]
		public static void ChangeOutsideEnemiesInGamePostFix(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.RealLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "SpawnEnemiesOutside")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemies(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
			Debug.Log(__instance.currentMaxOutsidePower);
			Debug.Log(__instance.currentLevel);
			Debug.Log(__instance.currentOutsideEnemyPower);
		}
		[HarmonyPatch(typeof(RoundManager), "SpawnEnemiesOutside")]
		[HarmonyPostfix]
		public static void SpawnOutsideEnemiesPostFix(RoundManager __instance)
		{
			
			__instance.currentLevel = ChooseRandom.RealLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemy(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
		}
		[HarmonyPatch(typeof(RoundManager), "SpawnRandomOutsideEnemy")]
		[HarmonyPostfix]
		public static void SpawnOutsideEnemyPostFix(RoundManager __instance)
		{
			Debug.Log(__instance.currentMaxOutsidePower);
			Debug.Log(__instance.currentLevel);
			Debug.Log(__instance.currentOutsideEnemyPower);
			__instance.currentLevel = ChooseRandom.RealLevel;
		}

		[HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxOutsideEnemyPowerCount = ChooseRandom.OutdoorEnemiesLevel.maxOutsideEnemyPowerCount;
		}
		
	}
	public class ChangeInsideEnemies
	{
		
// LLL issue was found here
		[HarmonyPatch(typeof(RoundManager), "PlotOutEnemiesForNextHour")]
		[HarmonyPrefix]
		public static void PlotOutEnemiesForNextHour(RoundManager __instance)
		{
			__instance.currentLevel.enemySpawnChanceThroughoutDay = ChooseRandom.IndoorEnemiesLevel.enemySpawnChanceThroughoutDay;
			__instance.currentLevel.spawnProbabilityRange = ChooseRandom.IndoorEnemiesLevel.spawnProbabilityRange;
		}
		
		[HarmonyPatch(typeof(RoundManager), "AssignRandomEnemyToVent")]
		[HarmonyPrefix]
		public static void AssignRandomEnemyToVent(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
			__instance.currentLevel.Enemies = ChooseRandom.IndoorEnemiesLevel.Enemies;
			Debug.Log(__instance.currentLevel);
			Debug.Log(__instance.currentMaxInsidePower);
		}
		[HarmonyPatch(typeof(RoundManager), "AssignRandomEnemyToVent")]
		[HarmonyPostfix]
		public static void AssignRandomEnemyToVentPostFix(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.RealLevel;
		}
		
		[HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
		[HarmonyPrefix]
		public static void EnemyCannotBeSpawned(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
		}
		
		[HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
		[HarmonyPostfix]
		public static void EnemyCannotBeSpawnedPostFix(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.RealLevel;
		}
		// LLL issue was found here
		[HarmonyPatch(typeof(RoundManager), "BeginEnemySpawning")]
		[HarmonyPrefix]
		public static void BeginEnemySpawning(RoundManager __instance)
		{
			__instance.currentLevel.maxEnemyPowerCount = ChooseRandom.IndoorEnemiesLevel.maxEnemyPowerCount;
		}
		
		[HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxEnemyPowerCount = ChooseRandom.IndoorEnemiesLevel.maxEnemyPowerCount;
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
				var scrap = ChooseRandom.ScrapLevel.PlanetName ?? "Unknown";
				var enemies = ChooseRandom.IndoorEnemiesLevel.PlanetName ?? "Unknown";
				var outside = ChooseRandom.OutdoorEnemiesLevel.PlanetName ?? "Unknown";

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