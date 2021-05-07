using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace CinicTools.DiscordReporter {

	public static class DiscordAPI {

		public static IEnumerator SendDiscordReport(string url, string content, bool attachScreenShot = true) {
			yield return new WaitForEndOfFrame();
			
			var formData = new WWWForm();
			formData.AddField("content", content);
			
			// Screenshot, optional but very useful for seeing UI errors
			if (attachScreenShot) {
				formData.AddBinaryData("screenshot", ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG(), "discord_error_screenshot.png");
			}
			
			Debug.Log("Sending request");
			UnityWebRequest request = UnityWebRequest.Post(url, formData);
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success) {
				Debug.Log(request.error);
				Debug.Log(request.downloadHandler.text);
			}

			yield return new WaitForEndOfFrame();
			
			// Console off
			Debug.developerConsoleVisible = false;
		}

		public static string GetReport(string title, string stackTrace, LogType type = LogType.Error, bool userReport = false) {
			// Add build version
			string content = $"**v{Application.version}** ({(Debug.isDebugBuild ? "Debug" : "Release")} on __{Application.platform}__)\n";
			
			// Set up the report
			if (userReport) {
				content += "**Type**: User Report\n";
				content += $"**Title**: {title}\n";
				content += $"**Message**: {stackTrace}";
			} else {
				content += "**AUTO REPORT**\n";
				content += $"**Type**: {type}\n";
				content += $"**Title**: {title}\n";
				content += $"**{(type == LogType.Exception ? "StackTrace" : "Message")}**: {stackTrace}";
			}
			
			return content;
		}
	}

}