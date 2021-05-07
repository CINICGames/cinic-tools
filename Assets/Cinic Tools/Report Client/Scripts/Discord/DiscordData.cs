using UnityEngine;

namespace CinicGames.Tools.Report.Discord {
	[CreateAssetMenu(fileName = "DiscordData", menuName = "Cinic Tools/Report/Discord/Discord Data")]
	public class DiscordData : ScriptableObject {
		[SerializeField] private string webhook= "<DISCORD_WEBHOOK_URL>";
		public string Webhook => webhook;
	}
}