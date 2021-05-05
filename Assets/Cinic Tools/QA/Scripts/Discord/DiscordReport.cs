using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CinicGames.Tools.Report.Discord {
	[CreateAssetMenu(fileName = "discord_report", menuName = "Data/QA/Discord/Discord Report")]
	public class DiscordReport : ReportReaction {
		public DiscordData discordData;

		public override IEnumerator React(string title, string msg, byte[] screenshot) {
			UnityWebRequest discordRequest = UnityWebRequest.Post(discordData.Webhook, DiscordUtility.GetFormData(title, msg, screenshot, "Report"));
			yield return discordRequest.SendWebRequest();
		}
	}
}