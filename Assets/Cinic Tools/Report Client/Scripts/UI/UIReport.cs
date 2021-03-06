using System;
using System.Collections;
using CinicGames.Tools.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CinicGames.Tools.Report {
	[RequireComponent(typeof(CanvasGroup))]
	public class UIReport : MonoBehaviour {
		[SerializeField] private KeyCode openCloseKey = default;

		[SerializeField] private TMP_InputField title = default;
		[SerializeField] private TMP_InputField description = default;
		[SerializeField] private Toggle screenshotToggle = default;
		[Space]
		[SerializeField] private CanvasGroup sendingFrameCanvas = default;
		[SerializeField] private CanvasGroup sentFrameCanvas = default;
		

		private CanvasGroup canvasGroup;
		
		private byte[] screenshot;

		/// <summary>
		/// Invoked before taking a screenshot
		/// </summary>
		public event Action OnScreenshotTake;

		/// <summary>
		/// Invoked after taking a screenshot
		/// </summary>
		public event Action AfterScreenshotTake;
		
		private void Awake() {
			canvasGroup = GetComponent<CanvasGroup>();
		}
		
		private void Update() {
			if (Input.GetKeyDown(openCloseKey)) 
				Show(canvasGroup.alpha == 0);
		}

		public void Show(bool value) {
			canvasGroup.Show(value);
			if(!value)
				Clear();
		}

		public void SendReport() => StartCoroutine(Report());

		private IEnumerator Report() {
			screenshot = default;
			
			// Screenshot
			if (screenshotToggle.isOn) {
				OnScreenshotTake?.Invoke();
				yield return TakeScreenshot();
				AfterScreenshotTake?.Invoke();
			}
			
			canvasGroup.interactable = false;
			sendingFrameCanvas.Show(true);
			
			// Send Report
			yield return CinicReportClient.Singleton.SendReportReactions(title.text, description.text, screenshotToggle.isOn ? screenshot : null);

			sendingFrameCanvas.Show(false);
			sentFrameCanvas.Show(true);
		}

		private IEnumerator TakeScreenshot() {
			Show(false);
			yield return new WaitForEndOfFrame();
			screenshot = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
			Show(true);
			yield return new WaitForEndOfFrame();
		}

		private void Clear() {
			title.text = string.Empty;
			description.text = string.Empty;
		}

		public void ClosePopup() {
			canvasGroup.interactable = true;
			sentFrameCanvas.Show(false);
		}
	}
}