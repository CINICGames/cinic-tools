using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CinicTools.DiscordReporter {

	public class DiscordReporter : MonoBehaviour {
		private static DiscordReporter instance;

		public string discordWebhook;
		[Space]
		public List<LogType> reportedLogTypes = new List<LogType> { LogType.Assert, LogType.Error, LogType.Exception };
		public string[] ignoredMessages = new string[0];
		[Space]
		public bool autoReport = true;

		private readonly List<string> oldErrors = new List<string>();
			
		private void Awake() {
			#if !DEBUG
			Destroy(gameObject);
			#endif

			if (instance != null) {
				Debug.LogWarning("You have more than one Discord Reporter! This one will be destroyed.");
				Destroy(gameObject);
			} else {
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

		private void Start() {
			Debug.LogError("asd");
		}

		#region Enable - Disable

		#if DEBUG
		private void OnEnable() {
			Application.logMessageReceived += OnLogReceived;
		}

		private void OnDisable() {
			Application.logMessageReceived -= OnLogReceived;
		}
		#endif

		#endregion
		
		private void OnLogReceived(string condition, string stacktrace, LogType type) {
			if (!Application.isEditor && autoReport) {
				if (reportedLogTypes.Contains(type)) {
					// Don't spam duplicate messages, ignore some useless ones
					if (!oldErrors.Contains(stacktrace) && !ignoredMessages.Any(condition.Contains)) {
						oldErrors.Add(stacktrace);
						string content = DiscordAPI.GetReport(condition, stacktrace, type);
						StartCoroutine(DiscordAPI.SendDiscordReport(discordWebhook, content));
					}
				}
			}
		}
	}

}