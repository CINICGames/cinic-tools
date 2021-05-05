using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Changelog {
	public class UILog : MonoBehaviour {
		[SerializeField] private Toggle toggle = default;
		[SerializeField] private TextMeshProUGUI description = default;
		private Log log;
		public Log Log => log;
		
		public void Set(Log log) {
			if (log == null || log.log == string.Empty) {
				Destroy(gameObject);
			}
			toggle.onValueChanged.RemoveAllListeners();
			this.log = log;
			toggle.isOn = log.check;
			description.text = log.log;
			toggle.onValueChanged.AddListener(OnChecked);
		}

		private void OnChecked(bool isChecked) {
			log.check = !log.check;
			ChangelogParser.UpdateValue(log);
		}
	}
}