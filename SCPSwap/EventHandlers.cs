﻿using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ScpSwap
{
	public class EventHandlers
	{
		// Riverrunner is the best, hehe
		public static Dictionary<Player, Player> ongoingReqs = new Dictionary<Player, Player>();

		public static List<CoroutineHandle> coroutines = new List<CoroutineHandle>();
		public static Dictionary<Player, CoroutineHandle> reqCoroutines = new Dictionary<Player, CoroutineHandle>();

		public static bool allowSwaps = false;
		public static bool isRoundStarted = false;

		private StringBuilder listBuilder = new StringBuilder();

		public static Dictionary<string, RoleType> valid = new Dictionary<string, RoleType>()
		{
			{"173", RoleType.Scp173},
			{"Scp173", RoleType.Scp173},
			{"scp173", RoleType.Scp173},
			{"peanut", RoleType.Scp173},
			{"939", RoleType.Scp93953},
			{"Scp93953", RoleType.Scp93953},
			{"scp93953", RoleType.Scp93953},
			{"Scp93989", RoleType.Scp93953},
			{"scp93989", RoleType.Scp93953},
			{"dog", RoleType.Scp93953},
			{"Scp079", RoleType.Scp079},
			{"scp079", RoleType.Scp079},
			{"079", RoleType.Scp079},
			{"79", RoleType.Scp079},
			{"computer", RoleType.Scp079},
			{"106", RoleType.Scp106},
			{"Scp106", RoleType.Scp106},
			{"scp106", RoleType.Scp106},
			{"larry", RoleType.Scp106},
			{"096", RoleType.Scp096},
			{"Scp096", RoleType.Scp096},
			{"scp096", RoleType.Scp096},
			{"96", RoleType.Scp096},
			{"shyguy", RoleType.Scp096},
			{"049", RoleType.Scp049},
			{"Scp049", RoleType.Scp049},
			{"scp049", RoleType.Scp049},
			{"49", RoleType.Scp049},
			{"doctor", RoleType.Scp049},
			{"0492", RoleType.Scp0492},
			{"Scp0492", RoleType.Scp0492},
			{"scp0492", RoleType.Scp0492},
			{"492", RoleType.Scp0492},
			{"zombie", RoleType.Scp0492}
		};

		public static ScpSwap plugin;

		public EventHandlers(ScpSwap pl)
		{
			plugin = pl;
		}

		public static IEnumerator<float> SendRequest(Player source, Player dest)
		{
			ongoingReqs.Add(source, dest);
			dest.Broadcast(5, "<i>You have an SCP Swap request!\nCheck your console by pressing [`] or [~]</i>");
			dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, $"You have received a swap request from {source.ReferenceHub.nicknameSync.Network_myNickSync} who is SCP-{valid.FirstOrDefault(x => x.Value == source.Role).Key}. Would you like to swap with them? Type \".scpswap yes\" to accept or \".scpswap no\" to decline.", "yellow");
			yield return Timing.WaitForSeconds(plugin.Config.SwapRequestTimeout);
			TimeoutRequest(source);
		}

		public static void TimeoutRequest(Player source)
		{
			if (ongoingReqs.ContainsKey(source))
			{
				Player dest = ongoingReqs[source];
				source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "The player did not respond to your request.", "red");
				dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has timed out.", "red");
				ongoingReqs.Remove(source);
			}
		}

		public static void PerformSwap(Player source, Player dest)
		{
			source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "Swap successful!", "green");

			RoleType sRole = source.Role.Type;
			RoleType dRole = dest.Role.Type;

			Vector3 sPos = source.Position;
			Vector3 dPos = dest.Position;

			float sHealth = source.Health;
			float dHealth = dest.Health;

			source.Role.Type = dRole;
			source.Position = dPos;
			source.Health = dHealth;

			dest.Role.Type = sRole;
			dest.Position = sPos;
			dest.Health = sHealth;

			ongoingReqs.Remove(source);
		}

		public void OnRoundStart()
		{
			allowSwaps = true;
			isRoundStarted = true;
			Timing.CallDelayed(plugin.Config.SwapTimeout, () => allowSwaps = false);
			Timing.CallDelayed(1f, () => BroadcastMessage());
		}

		public void OnRoundRestart()
		{
			// fail safe
			isRoundStarted = false;
			foreach (CoroutineHandle hndl in coroutines)
				Timing.KillCoroutines(hndl);
			foreach (CoroutineHandle hndl in reqCoroutines.Values)
				Timing.KillCoroutines(hndl);
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			isRoundStarted = false;
			foreach (CoroutineHandle hndl in coroutines)
				Timing.KillCoroutines(hndl);
			foreach (CoroutineHandle hndl in reqCoroutines.Values)
				Timing.KillCoroutines(hndl);
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnWaitingForPlayers()
		{
			allowSwaps = false;
		}


		public void BroadcastMessage()
		{
			if (plugin.Config.DisplayStartMessage)
			{
				foreach (Player ply in Player.List)
					if (ply.Role.Team == Team.SCP)
						ply.Broadcast(plugin.Config.StartMessageTime, plugin.Config.DisplayMessageText);
			}
		}
	}
}
