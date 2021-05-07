using System.Collections;
using CinicGames.Tools.Utilities;
using TMPro;
using UnityEngine;

namespace Changelog {
	public class Log {
		public int row;
		public bool check;
		public string log;
		public string cellRef;

		public override string ToString() {
			return $"[{row}], [{check}], [{log}]";
		}
	}

	public class UIChangelogManager : MonoBehaviour {
		[SerializeField] private string sheetID = default;
		[SerializeField] private string worksheet = default;

		private static UIChangelogManager instance;
		public static UIChangelogManager Instance => instance;
		[SerializeField] private UIChangelog changelog = default;
		// [SerializeField] private UIReport report = default;
		[SerializeField] private KeyCode key = default;
		public CanvasGroup canvasGroup = default;
		public TMP_Text closeText;
		private bool showing = true;

		private void Awake() {
			if (instance == null) {
				instance = this;
				DontDestroyOnLoad(gameObject);
				return;
			}
			Destroy(gameObject);
		}

		public void Start() {
			closeText.text = $"Press {key} To close.";
			StartCoroutine(FirstInit());
		}

		private IEnumerator FirstInit() {
			yield return SetSheet();
			changelog.SetLogs(ChangelogParser.GetLogs());
		}

		private IEnumerator SetSheet() {
			while (!ChangelogParser.Initialized) {
				ChangelogParser.SetSheet(sheetID, worksheet);
				yield return new WaitForSeconds(0.1f);
			}
		}

		private void Update() {
			if (Input.GetKeyDown(key)) {
				ChangeVisibility();
			}
		}

		public void ChangeVisibility() {
			showing = !showing;
			StartCoroutine(ChangeState(showing));
		}

		private IEnumerator ChangeState(bool showing) {
			canvasGroup.Show(showing);
			Freeze(showing);
			if (showing) {
				yield return OnShow();
			}
			else OnHide();
		}

		public void Freeze(bool value) {
			Time.timeScale = (!value).ToInt();
		}

		private IEnumerator OnShow() {
			ChangelogParser.Initialized = false;
			yield return SetSheet();
			changelog.SetLogs(ChangelogParser.GetLogs());
		}

		private void OnHide() {
			// report.Clear();
			changelog.Clear();
		}

		public void ShowView(GameObject obj) {
			// report.Show(obj == report.gameObject);
			changelog.Show(obj == changelog.gameObject);
		}
	}
}