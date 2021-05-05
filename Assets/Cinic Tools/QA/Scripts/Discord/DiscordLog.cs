using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CinicGames.Tools.Report.Discord {
	[CreateAssetMenu(fileName = "discord_log", menuName = "Data/QA/Discord/Discord Log")]
	public class DiscordLog : LogReaction {
		public DiscordData discordData;

		public override IEnumerator React(string msg, string stack, LogType logType, byte[] screenshot) {
			UnityWebRequest discordRequest = UnityWebRequest.Post(discordData.Webhook, DiscordUtility.GetFormData(msg, stack, screenshot, logType.ToString()));
			yield return discordRequest.SendWebRequest();
		}
	}
}