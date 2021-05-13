using TMPro;
using UnityEngine;

namespace CinicGames.Tools.Changelog {
	public class UILogTitle : MonoBehaviour {
		[SerializeField] private TMP_Text title = default;

		public void Set(string newTitle) {
			title.text = newTitle;
		}
	}
}