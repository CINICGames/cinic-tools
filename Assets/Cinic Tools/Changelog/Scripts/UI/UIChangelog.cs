using System.Collections;
using System.Collections.Generic;
using CinicGames.Tools.Utilities;
using UnityEngine;

namespace CinicGames.Tools.Changelog {
	[RequireComponent(typeof(CanvasGroup))]
	public class UIChangelog : MonoBehaviour {
		[SerializeField] private UILog logPrefab = default;
		[SerializeField] private UILogTitle titlePrefab = default;
		[SerializeField] private Transform scrollViewContent = default;
		[SerializeField] private CanvasGroup canvasGroup = default;
		
		[SerializeField] private KeyCode openCloseKey = default;
		
		private void Awake() {
			canvasGroup = GetComponent<CanvasGroup>();
		}
		
		private void Update() {
			if (Input.GetKeyDown(openCloseKey)) 
				Show(canvasGroup.alpha == 0);
		}

		public void Show(bool showing) {
			canvasGroup.Show(showing);
			if (showing) OnShow();
			else OnHide();
		}
		
		private void OnShow() {
			ChangelogParser.Initialized = false;
			CinicChangelog.Singleton.LogsReady += SetLogs;
			CinicChangelog.Singleton.UpdateSheet();
		}

		private void OnHide() => UIUtility.Clear(scrollViewContent);

		private void SetLogs(Dictionary<string, List<Log>> changelog) {
			CinicChangelog.Singleton.LogsReady -= SetLogs;

			if (changelog == null)
				return;
			
			foreach (KeyValuePair<string, List<Log>> pair in changelog) {
				UIUtility.AddElement(titlePrefab, scrollViewContent).Set(pair.Key);
				foreach (Log log in pair.Value) {
					UILog uiLog = UIUtility.AddElement(logPrefab, scrollViewContent);
					uiLog.Set(log);
				}
			}
		}
		
	}
}

