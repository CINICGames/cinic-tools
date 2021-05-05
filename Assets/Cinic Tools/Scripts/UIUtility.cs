using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

	public static class UIUtility {
		public static void Clear(Transform transform) {
			if (transform != null) {
				foreach (Transform child in transform) {
					var button = child.GetComponent<Button>();
					if (button) {
						button.onClick.RemoveAllListeners();
					}
					child.gameObject.SetActive(false);
				}
			}
		}

		public static T AddElement<T>(T original, Transform parent, Action<T> set = null, Action onClick = null) where T : MonoBehaviour {
			T newObject = null;
			foreach (Transform child in parent) {
				if (!child.gameObject.activeSelf) {
					newObject = child.GetComponent<T>();
					if (newObject) {
						newObject.gameObject.SetActive(true);
					}
					break;
				}
			}
			if (!newObject) {
				newObject = Object.Instantiate(original, parent);
			}
			set?.Invoke(newObject);

			if (onClick != null) {
				var button = newObject.GetComponent<Button>();
				if (button) {
					button.onClick.RemoveAllListeners();
					button.onClick.AddListener(new UnityEngine.Events.UnityAction(onClick));
				}
			}
			return newObject;
		}

	}