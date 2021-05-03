using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using UnityEngine;
using UnityEngine.Networking;

// License: MIT
// Created by: Aceria_
// Edited by: Nesh108

public class RestClient : MonoBehaviour {
	#region Fields

	[SerializeField] private string discordWebhook = "<DISCORD_WEBHOOK_URL>";
	[SerializeField] private string projectId = "<PROJECT ID>";
	[SerializeField] private string bugFixBoardId = "<BOARD ID>";
	[SerializeField] private string reportBoardId = "<BOARD ID>";
	[SerializeField] private HacknPlanData hacknPlanData = default;

	[SerializeField] private bool enableErrorsReport = true;

	private static RestClient instance;
	public static RestClient Instance => instance;
	private static readonly List<string> LastErrors = new List<string>();

	private static readonly LogType[] AllowedLogTypes =
	{
		LogType.Assert,
		LogType.Error,
		LogType.Exception
	};

	private static readonly string[] FilteredMessages =
	{
		"Failed to read input report",
		"FMOD failed to initialize the output",
		"Failed to create device file"
	};

	#endregion

	#region Unity Messages

	private void Awake() {
		if (!instance) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

#if DEBUG

	private void OnEnable() {
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable() {
		Application.logMessageReceived -= HandleLog;
	}

#endif

	#endregion

	#region Methods

	private void HandleLog(string msg, string stack, LogType type) {
		if (!Application.isEditor && enableErrorsReport) {
			if (AllowedLogTypes.Contains(type)) {
				// Don't spam duplicate messages, ignore some useless ones
				if (!LastErrors.Contains(stack) && !FilteredMessages.Any(msg.Contains)) {
					LastErrors.Add(stack);
					StartCoroutine(PrepareRequest(msg, stack, type));
				}
			}
		}
	}

	private IEnumerator PrepareRequest(string msg, string stack, LogType type) {
		// screenshot
		yield return new WaitForEndOfFrame();
		byte[] screenshot = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
		// discord
		UnityWebRequest discordRequest = UnityWebRequest.Post(discordWebhook, GetDiscordFormData(msg, stack, type, screenshot));
		discordRequest.SendWebRequest();
		// console off
		yield return new WaitForEndOfFrame();
		Debug.developerConsoleVisible = false;
		// hacknplan
		StartCoroutine(CreateBugfixTask(msg, stack, screenshot));
	}

	private static WWWForm GetDiscordFormData(string msg, string stack, LogType type, byte[] screenshot) {
		var formData = new WWWForm();
		// Add build version
		string content = $"**v{Application.version}** ({(Debug.isDebugBuild ? "Debug" : "Release")} on __{Application.platform}__)\n";
		// Set up the report
		content += $"**Type**: {type}\n";
		content += $"**Message**: {msg}\n";
		content += $"**Callstack**: {stack}";
		formData.AddField("content", content);
		//optional, but very useful for seeing UI errors
		if (screenshot != null) {
			formData.AddBinaryData("screenshot", screenshot, "discord_error_screenshot.png");
		}
		return formData;
	}

	public IEnumerator CreateBugfixTask(string msg, string stack, byte[] screenshot) {
		// new task
		var t = new HacknPlanData.Task
		{
			boardId = int.Parse(bugFixBoardId),
			title = $"v{Application.version} - {msg}",
			description = "-----\n" +
			              $"v{Application.version} - {msg}\n" +
			              $"```\n{stack}\n```",
			assignedUserIds = hacknPlanData.GetReportTaskUsers()
		};
		string jsonBody = JsonUtility.ToJson(t);
		UnityWebRequest www = UnityWebRequest.Put($"https://api.hacknplan.com/v0/projects/{projectId}/workitems", jsonBody);
		www.method = "POST";
		www.SetRequestHeader("Content-Type", "application/json");
		www.SetRequestHeader("Authorization", "ApiKey b9aa1c6855994729ae8ccb345b8e4bb9");
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError) {
			Debug.LogError(www.error);
		}
		else {
			Debug.Log(www.downloadHandler.text);
			var newTask = (HacknPlanData.Task)JsonUtility.FromJson(www.downloadHandler.text, typeof(HacknPlanData.Task));
			// attachment
			yield return new WaitForEndOfFrame();
			var client = new RestSharp.RestClient($"https://api.hacknplan.com/v0/projects/{newTask.projectId}/workitems/{newTask.workItemId}/attachments");
			client.Timeout = -1;
			var request = new RestRequest(Method.POST);
			request.AddHeader("Content-Type", "multipart/form-data");
			request.AddHeader("Authorization", "ApiKey b9aa1c6855994729ae8ccb345b8e4bb9");
			request.AddFile("screenshot", screenshot, "screenshot.png");
			client.Execute(request);
		}
	}

	public IEnumerator CreateReportTask(string title, string msg, byte[] screenshot) {
		// new task
		var t = new HacknPlanData.Task
		{
			boardId = int.Parse(reportBoardId),
			title = $"v{Application.version} - {title}",
			description = "-----\n" +
			              $"v{Application.version} - {msg}",
			assignedUserIds = hacknPlanData.GetReportTaskUsers()
		};
		string jsonBody = JsonUtility.ToJson(t);
		UnityWebRequest www = UnityWebRequest.Put($"https://api.hacknplan.com/v0/projects/{projectId}/workitems", jsonBody);
		www.method = "POST";
		www.SetRequestHeader("Content-Type", "application/json");
		www.SetRequestHeader("Authorization", "ApiKey b9aa1c6855994729ae8ccb345b8e4bb9");
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError) {
			Debug.LogError(www.error);
		}
		else {
			Debug.Log(www.downloadHandler.text);
			var newTask = (HacknPlanData.Task)JsonUtility.FromJson(www.downloadHandler.text, typeof(HacknPlanData.Task));
			// attachment
			yield return new WaitForEndOfFrame();
			var client = new RestSharp.RestClient($"https://api.hacknplan.com/v0/projects/{newTask.projectId}/workitems/{newTask.workItemId}/attachments");
			client.Timeout = -1;
			var request = new RestRequest(Method.POST);
			request.AddHeader("Content-Type", "multipart/form-data");
			request.AddHeader("Authorization", "ApiKey b9aa1c6855994729ae8ccb345b8e4bb9");
			if (screenshot != null) {
				request.AddFile("screenshot", screenshot, "screenshot.png");
			}
			client.Execute(request);
		}
	}

	public IEnumerator SendDiscordReport(string title, string msg, byte[] screenshot) {
		UnityWebRequest discordRequest = UnityWebRequest.Post(discordWebhook, GetDiscordFormData(title, msg, screenshot));
		yield return discordRequest.SendWebRequest();
	}

	private static WWWForm GetDiscordFormData(string title, string msg, byte[] screenshot) {
		var formData = new WWWForm();
		// Add build version
		string content = $"**v{Application.version}** ({(Debug.isDebugBuild ? "Debug" : "Release")} on __{Application.platform}__)\n";
		// Set up the report
		content += "**Type**: Report\n";
		content += $"**Title**: {title}\n";
		content += $"**Message**: {msg}";
		formData.AddField("content", content);
		//optional, but very useful for seeing UI errors
		if (screenshot != null) {
			formData.AddBinaryData("screenshot", screenshot, "discord_report_screenshot.png");
		}
		return formData;
	}

	#endregion
}