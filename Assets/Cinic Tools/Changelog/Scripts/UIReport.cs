using System.Collections;
using CinicGames.Tools.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Changelog {
	public class UIReport : MonoBehaviour {
		[SerializeField] private CanvasGroup canvasGroup = default;
		[SerializeField] private TMP_InputField title = default;
		[SerializeField] private TMP_InputField description = default;
		[SerializeField] private Toggle screenshotToggle = default;
		[SerializeField] private CanvasGroup sendingFrameCanvas = default;
		[SerializeField] private CanvasGroup sentFrameCanvas = default;
		private byte[] screenshot;

		public void Show(bool value) {
			canvasGroup.Show(value);
		}

		public void SendReport() {
			StartCoroutine(Report());
		}

		private IEnumerator Report() {
			screenshot = default;
			// Screenshot
			if (screenshotToggle.isOn) {
				yield return TakeScreenshot();
			}
			canvasGroup.interactable = false;
			sendingFrameCanvas.Show(true);
			
			// Discord
			yield return RestClient.Instance.SendDiscordReport(title.text, description.text, screenshotToggle.isOn ? screenshot : null);
			
			// HackNPlan
			yield return RestClient.Instance.CreateReportTask(title.text, description.text, screenshotToggle.isOn ? screenshot : null);
			sendingFrameCanvas.Show(false);
			sentFrameCanvas.Show(true);
		}

		private IEnumerator TakeScreenshot() {
			UIChangelogManager.Instance.Freeze(false);
			UIChangelogManager.Instance.canvasGroup.Show(false);
			yield return new WaitForEndOfFrame();
			screenshot = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
			UIChangelogManager.Instance.canvasGroup.Show(true);
			UIChangelogManager.Instance.Freeze(true);
			yield return new WaitForEndOfFrame();
		}
		
		public void Clear() {
			title.text = string.Empty;
			description.text = string.Empty;
			screenshotToggle.isOn = true; 
		}

		public void ClosePopup() {
			canvasGroup.interactable = true;
			sentFrameCanvas.Show(false);
		}
		
	}
}