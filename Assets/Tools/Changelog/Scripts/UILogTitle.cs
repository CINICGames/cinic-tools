using TMPro;
using UnityEngine;

namespace Changelog {
	public class UILogTitle : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI title = default;

		public void Set(string title) {
			this.title.text = title;
		}
	}
}