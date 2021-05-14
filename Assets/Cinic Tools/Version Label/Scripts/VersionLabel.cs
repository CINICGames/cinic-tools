using TMPro;
using UnityEngine;

namespace CinicGames.Tools.Version {
    public class VersionLabel : MonoBehaviour {
        [SerializeField] private TMP_Text label = default;

        private void Awake() {
            label.text = $"Version {Application.version}";
        }
    }
}
