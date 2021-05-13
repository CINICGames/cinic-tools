using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CinicGames.Tools.Changelog {
	public class UILog : MonoBehaviour {
		[SerializeField] private Toggle toggle = default;
		[SerializeField] private TMP_Text description = default;
		private Log log;
		
		public void Set(Log newLog) {
			if (newLog == null || newLog.log == string.Empty) 
				Destroy(gameObject);
			
			toggle.onValueChanged.RemoveAllListeners();
			log = newLog;
			toggle.isOn = newLog.check;
			description.text = newLog.log;
			toggle.onValueChanged.AddListener(OnChecked);
		}

		private void OnChecked(bool isChecked) {
			log.check = !log.check;
			ChangelogParser.UpdateValue(log);
		}
	}
}