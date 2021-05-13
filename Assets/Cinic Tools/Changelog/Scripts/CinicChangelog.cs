using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CinicGames.Tools.Changelog {
	public class CinicChangelog : MonoBehaviour {
		public static CinicChangelog Singleton { get; private set; }
		[SerializeField] private ChangelogData changelogData = default;
		public event Action<Dictionary<string, List<Log>>> LogsReady;
		
		private void Awake() {
			if (Singleton == null) {
				Singleton = this;
				DontDestroyOnLoad(gameObject);
				return;
			}
			Destroy(gameObject);
		}

		public void Start() {
			UpdateSheet();
		}

		public void UpdateSheet() {
			StartCoroutine(SetSheet());
		}
		
		private IEnumerator SetSheet() {
			if (changelogData.HasSetupError) {
				LogsReady?.Invoke(null);
				yield break;
			}
			ChangelogParser.Initialized = false;
			while (!ChangelogParser.Initialized) {
				ChangelogParser.SetSheet(changelogData.SheetID, changelogData.Worksheet);
				yield return new WaitForSeconds(0.1f);
			}
			LogsReady?.Invoke(ChangelogParser.GetLogs());
		}
	}
}