using UnityEngine;

namespace CinicGames.Tools.Report.Discord {
	[CreateAssetMenu(fileName = "discord_data", menuName = "Data/QA/Discord/Discord Data")]
	public class DiscordData : ScriptableObject {
		[SerializeField] private string webhook= "<DISCORD_WEBHOOK_URL>";
		public string Webhook => webhook;
	}
}