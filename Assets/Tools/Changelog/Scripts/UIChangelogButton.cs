using UnityEngine;

namespace Changelog {
	public class UIChangelogButton : MonoBehaviour {
		[SerializeField] private GameObject objectToShow = default;
		
		public void OnClick() {
			UIChangelogManager.Instance.ShowView(objectToShow);
		}
	}
}