using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CinicTools.Tools.Utilities {

	public static class RestAPI {

		/// <summary>
		/// Sends a report to the specified webhook
		/// </summary>
		/// <param name="url">Url to send the report to</param>
		/// <param name="content">Content of the message</param>
		/// <param name="screenshot">Screenshot to attach, nothing attached if null</param>
		/// <returns></returns>
		public static IEnumerator SendReport(string url, string content, byte[] screenshot) {
			yield return new WaitForEndOfFrame();
			
			var formData = new WWWForm();
			formData.AddField("content", content);
			
			// Screenshot, optional but very useful for seeing UI errors
			if (screenshot != null) {
				formData.AddBinaryData("screenshot", screenshot, "error_screenshot.png");
			}
			
			Debug.Log("Sending request");
			UnityWebRequest request = UnityWebRequest.Post(url, formData);
			request.SendWebRequest();

			yield return new WaitForEndOfFrame();
			
			// Console off
			Debug.developerConsoleVisible = false;
		}

		/// <summary>
		/// Automatic content format for the discord message, two modes:
		/// - User report
		/// - Auto report
		/// </summary>
		/// <param name="title">Title of the report</param>
		/// <param name="stackTrace">StackTrace of the report, also the message for the user report </param>
		/// <param name="type">Type of the log</param>
		/// <param name="userReport">Toggles the format to use (true -> auto report, false -> user report). Default is false</param>
		/// <returns></returns>
		public static string FormatDiscordContent(string title, string stackTrace, LogType type = LogType.Error, bool userReport = false) {
			// Set build version
			string version = $"**v{Application.version}** ({(Debug.isDebugBuild ? "Debug" : "Release")} on __{Application.platform}__)\n";
			
			// Set up the report
			string content = "";
			content += $"**{(userReport ? "USER" : "AUTO")} REPORT**\n";
			content += version;
			content += $"**Title**: {title}\n";
			content += $"**Type**: {type}\n";
			content += $"**{(userReport ? "Message" : "StackTrace")}**\n";
			content += $"{stackTrace}";
			
			return content;
		}
	}

}