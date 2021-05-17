using TMPro;
using UnityEngine;

namespace CinicGames.Tools.Version {
    public class VersionLabel : MonoBehaviour {
        [SerializeField] private TMP_Text label = default;
        [SerializeField] private string prefixText = "Version";

        private void Awake() {
            label.text = $"{prefixText} {Application.version}";
        }
    }
}
