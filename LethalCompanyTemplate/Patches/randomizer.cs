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
		public static SelectableLevel ScrapLevel;
		public static SelectableLevel IndoorEnemiesLevel;
		public static SelectableLevel OutdoorEnemiesLevel;
		
		[HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
		[HarmonyPrefix]

		public static void Randomize(RoundManager __instance)
		{
			RealLevel = __instance.currentLevel;
			ScrapLevel =  RoundManager.Instance.playersManager.levels[new Random().Next(0, RoundManager.Instance.playersManager.levels.Length)];
			IndoorEnemiesLevel =  RoundManager.Instance.playersManager.levels[new Random().Next(0, RoundManager.Instance.playersManager.levels.Length)];
			OutdoorEnemiesLevel =  RoundManager.Instance.playersManager.levels[new Random().Next(0, RoundManager.Instance.playersManager.levels.Length)];
			while (OutdoorEnemiesLevel.PlanetName == "44 Liquidation")
			{
				OutdoorEnemiesLevel =  RoundManager.Instance.playersManager.levels[new Random().Next(0, RoundManager.Instance.playersManager.levels.Length)];
				Debug.Log(OutdoorEnemiesLevel.PlanetName + " was liquidation before");
			}
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
				ChooseRandom.ScrapLevel = RoundManager.Instance.playersManager.levels[new Random().Next(0, RoundManager.Instance.playersManager.levels.Length)];
			}
			
			Debug.Log(ChooseRandom.ScrapLevel.PlanetName);
			__instance.currentLevel = ChooseRandom.ScrapLevel;
			PickedLevel = __instance.currentLevel;
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
	}
	public class ChangeOutsideEnemies
	{
		[HarmonyPatch(typeof (RoundManager), "PredictAllOutsideEnemies")]
		[HarmonyPrefix]
		public static void ChooseOutsideEnemies(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
			Debug.Log((object) ("Outside Enemies: " + __instance.currentLevel.PlanetName));
		}

		[HarmonyPatch(typeof (RoundManager), "SpawnEnemiesOutside")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemies(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
			Debug.Log((object) ("Outside Enemies: " + __instance.currentLevel.PlanetName));
		}

		[HarmonyPatch(typeof (RoundManager), "SpawnRandomOutsideEnemy")]
		[HarmonyPrefix]
		public static void SpawnOutsideEnemy(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.OutdoorEnemiesLevel;
		}

		[HarmonyPatch(typeof (RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxOutsideEnemyPowerCount = ChooseRandom.OutdoorEnemiesLevel.maxOutsideEnemyPowerCount;
		}
	}
	public class ChangeInsideEnemies
	{
		
// LLL issue was found here
		[HarmonyPatch(typeof (RoundManager), "PlotOutEnemiesForNextHour")]
		[HarmonyPrefix]
		public static void PlotOutEnemiesForNextHour(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
		}

		[HarmonyPatch(typeof (RoundManager), "AssignRandomEnemyToVent")]
		[HarmonyPrefix]
		public static void AssignRandomEnemyToVent(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
		}

		[HarmonyPatch(typeof (RoundManager), "EnemyCannotBeSpawned")]
		[HarmonyPrefix]
		public static void EnemyCannotBeSpawned(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
		}

		[HarmonyPatch(typeof (RoundManager), "BeginEnemySpawning")]
		[HarmonyPrefix]
		public static void BeginEnemySpawning(RoundManager __instance)
		{
			__instance.currentLevel = ChooseRandom.IndoorEnemiesLevel;
		}

		[HarmonyPatch(typeof (RoundManager), "RefreshEnemiesList")]
		[HarmonyPrefix]
		public static void RefreshEnemyList(RoundManager __instance)
		{
			__instance.currentLevel.maxEnemyPowerCount = ChooseRandom.IndoorEnemiesLevel.maxEnemyPowerCount;
		}
	
	}
	public class ChangeText
	{
		private static LNetworkMessage<string> EventMessage = LNetworkMessage<string>.Connect("ServerSendAnnouncment");
		private static string _planetScrap;
		private static string _planetEnemies;
		private static string _planetOutside;
		
		[HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
		[HarmonyPrefix]
		public static void SendPlanets(StartOfRound __instance)
		{
			try
			{

				EventMessage.OnClientReceived += EventMessageOnOnClientReceived;
				
				if (__instance.IsServer)
				{
					var scrap = ChooseRandom.ScrapLevel.PlanetName ?? "Unknown";
					var enemies = ChooseRandom.IndoorEnemiesLevel.PlanetName ?? "Unknown";
					var outside = ChooseRandom.OutdoorEnemiesLevel.PlanetName ?? "Unknown";
					EventMessage.SendClients($"{scrap}/{enemies}/{outside}");
				}
				
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
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("S:" + _planetScrap);
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("IE:" + _planetEnemies);
			__instance.gameStats.allPlayerStats[0].playerNotes.Add("OE:" + _planetOutside);
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

			_planetScrap = pickedPlanets[0];
			_planetEnemies = pickedPlanets[1];
			_planetOutside = pickedPlanets[2];
			
		}

	}
}