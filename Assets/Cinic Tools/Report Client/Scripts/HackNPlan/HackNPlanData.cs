using System;
using System.Collections.Generic;
using System.Linq;
using CinicGames.Tools.Utilities;
using UnityEngine;

namespace CinicGames.Tools.Report.HackNPlan {
	[CreateAssetMenu(fileName = "hacknplan_data", menuName = "Cinic Tools/Report/HackNPlan/HackNPlan Data")]
	public class HackNPlanData : ScriptableObject {
		[Serializable]
		public struct UserEntry {
			public string name;
			public int id;
		}
	
		public class Task {
			public string projectId;
			public string workItemId;
			public int[] assignedUserIds;

			public string title = "Unity";
			public string description = "WIP";
			public int categoryId = 1;
			public float estimatedCost = 0f;
			public int importanceLevelId = 3;
			public int boardId = 0;
			public string startDate = Utility.GetCurrentJsonDate();
			public string dueDate = "";
		}

		[SerializeField] private string apiKey = "<API_KEY>";
		public string ApiKey => apiKey;
		
		[SerializeField] private string projectId = "<PROJECT_ID>";
		public string ProjectId => projectId;
		
		[SerializeField] private string boardId = "<BOARD_ID>";
		public string BoardId => boardId;

		
		[SerializeField] private UserEntry[] users = default;
		
		public int GetUserID(string user) => Array.Find(users, u => u.name == user).id;

		public int[] GetTaskUsers() => users.Select(user => user.id).ToArray();
		

	}
}
