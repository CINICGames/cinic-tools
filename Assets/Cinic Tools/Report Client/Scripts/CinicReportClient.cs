using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// License: MIT
// Created by: Aceria_
// Edited by: Nesh108
namespace CinicGames.Tools.Report {
	public class CinicReportClient : MonoBehaviour {
		#region Fields

		[SerializeField] private bool enableLogErrorsReaction = true;

		public static CinicReportClient Singleton { get; private set; }
		public List<LogReaction> logAddons;
		public List<ReportReaction> reportAddons;
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
			if (!Singleton) {
				Singleton = this;
				DontDestroyOnLoad(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}

#if DEVELOPMENT_BUILD

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
			if (enableLogErrorsReaction) {
				if (AllowedLogTypes.Contains(type)) {
					// Don't spam duplicate messages, ignore some useless ones
					if (!LastErrors.Contains(stack) && !FilteredMessages.Any(msg.Contains)) {
						LastErrors.Add(stack);
						StartCoroutine(SendLogReactions(msg, stack, type));
					}
				}
			}
		}

		private IEnumerator SendLogReactions(string msg, string stack, LogType type) {
			// screenshot
			yield return new WaitForEndOfFrame();
			byte[] screenshot = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();

			// console off
			yield return new WaitForEndOfFrame();
			Debug.developerConsoleVisible = false;

			// send reactions
			foreach (LogReaction addOn in logAddons) {
				yield return addOn.React(msg, stack, type, screenshot);
			}
		}

		public IEnumerator SendReportReactions(string msg, string stack, byte[] screenshot) {
			// send reactions
			foreach (ReportReaction addOn in reportAddons) {
				yield return addOn.React(msg, stack, screenshot);
			}
		}

		#endregion
	}
}
