using System.Collections.Generic;
using CinicGames.Tools.Utilities;
using UnityEngine;

namespace Changelog {
	public class UIChangelog : MonoBehaviour {
		[SerializeField] private UILog logPrefab = default;
		[SerializeField] private UILogTitle titlePrefab = default;
		[SerializeField] private Transform scrollViewContent = default;
		private List<UILog> uiLogs;
		[SerializeField] private CanvasGroup canvasGroup = default;

		public void Show(bool value) {
			canvasGroup.Show(value);
		}
		
		public List<UILog> SetLogs(Dictionary<string, List<Log>> changelog) {
			uiLogs = new List<UILog>();
			foreach (KeyValuePair<string, List<Log>> pair in changelog) {
				UIUtility.AddElement(titlePrefab, scrollViewContent).Set(pair.Key);
				foreach (Log log in pair.Value) {
					UILog uiLog = UIUtility.AddElement(logPrefab, scrollViewContent);
					uiLog.Set(log);
					uiLogs.Add(uiLog);
				}
			}
			return uiLogs;
		}

		public void Clear() {
			UIUtility.Clear(scrollViewContent);
		}
	}
}

