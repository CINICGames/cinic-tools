using System.Collections.Generic;
using GoogleSheetsToUnity;

namespace CinicGames.Tools.Changelog {
	public class Log {
		public int row;
		public bool check;
		public string log;
		public string cellRef;

		public override string ToString() {
			return $"[{row}], [{check}], [{log}]";
		}
	}
	
	public static class ChangelogParser {
		private static GstuSpreadSheet sheet;
		public static bool Initialized {
			get => sheet != null;
			set { if(!value)sheet = null; }
		}
		private static string sheetID;
		private static string worksheet;

		public static void SetSheet(string id, string sheetName) {
			sheetID = id;
			worksheet = sheetName;
			SpreadsheetManager.Read(new GSTU_Search(sheetID, worksheet), s=>sheet=s);
		}

		public static Dictionary<string, List<Log>> GetLogs() {
			List<GSTU_Cell> columnA = sheet.columns["A"];
			var changelog = new Dictionary<string, List<Log>>();
			string version = "";
			
			for (int i = 0; i < columnA.Count; i++) {
				
				if (columnA[i].value != "TRUE" && columnA[i].value != "FALSE") {
					changelog.Add(columnA[i].value, new List<Log>());
					version = columnA[i].value;
				}
				else {
					string cellRef = "B"+columnA[i].Row();
					changelog[version].Add(new Log
					{
						check = columnA[i].value == "TRUE",
						log = sheet[cellRef].value,
						row = i,
						cellRef = columnA[i].CellRef()
					});
				}
			}
			return changelog;
		}

		public static void UpdateValue(Log log) {
			GSTU_Cell x = sheet[log.cellRef];
			string value = log.check ? "TRUE" : "FALSE";
			x.UpdateCellValue(sheetID, worksheet, value);
		}

	}
}