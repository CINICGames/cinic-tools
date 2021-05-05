using UnityEngine;

namespace CinicGames.Tools.Report.Discord {
	public static class DiscordUtility {
		public static WWWForm GetFormData(string title, string msg, byte[] screenshot, string messageType) {
			var formData = new WWWForm();

			// Add build version
			string content = $"**v{Application.version}** ({(Debug.isDebugBuild ? "Debug" : "Release")} on __{Application.platform}__)\n";

			// Set up the report
			content += $"**Type**: {messageType}\n";
			content += $"**Title**: {title}\n";
			content += $"**Message**: {msg}";
			formData.AddField("content", content);

			//optional, but very useful for seeing UI errors
			if (screenshot != null) {
				formData.AddBinaryData("screenshot", screenshot, "screenshot.png");
			}
			return formData;
		}
	}
}