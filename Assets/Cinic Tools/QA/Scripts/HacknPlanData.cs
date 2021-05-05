using System;
using System.Collections.Generic;
using CinicGames.Tools.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "hacknplan_data", menuName = "Data/QA/Hacknplan Data")]
public class HacknPlanData : ScriptableObject {
	[Serializable]
	public struct UserEntry {
		public string name;
		public int id;
		public bool assignToReportTask;
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
		public int boardId = 327934;
		public string startDate = Utility.GetCurrentJsonDate();
		public string dueDate = "";
	}

	[SerializeField] private UserEntry[] users = default;

	public int GetUserID(string user) => Array.Find(users, u => u.name == user).id;

	public int[] GetReportTaskUsers() {
		var ids = new List<int>();
		foreach (UserEntry user in users) {
			if(user.assignToReportTask)
				ids.Add(user.id);
		}
		return ids.ToArray();
	}
}