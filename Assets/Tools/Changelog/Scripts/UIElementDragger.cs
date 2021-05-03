using UnityEngine;
using UnityEngine.EventSystems;

namespace Changelog {
	public class UIElementDragger : MonoBehaviour, IDragHandler {
		public float speed;
		private RectTransform rectTransform;
		private void Start() {
			rectTransform = GetComponent<RectTransform>();
		}

		public void OnDrag(PointerEventData eventData) {
			rectTransform.anchoredPosition += (eventData.delta)*speed;
		}
	}
}