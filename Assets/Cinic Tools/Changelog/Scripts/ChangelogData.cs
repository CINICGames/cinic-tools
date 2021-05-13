using CinicGames.Tools.Utilities;
using UnityEngine;

namespace CinicGames.Tools.Changelog {
	[CreateAssetMenu(fileName = "changelog_data", menuName = "Cinic Tools/Changelog/Changelog Data")]
	public class ChangelogData : ScriptableObject {
		[SerializeField] private string sheetID = default;
		public string SheetID => sheetID;
		
       	[SerializeField] private string worksheet = default;
        public string Worksheet => worksheet;

        public bool HasSetupError => sheetID.IsNullOrEmpty() || worksheet.IsNullOrEmpty();
	}
}