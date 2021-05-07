using System.Collections;
using UnityEngine;

namespace CinicGames.Tools.Report.HackNPlan {
	[CreateAssetMenu(fileName = "hacknplan_report", menuName = "Data/QA/HackNPlan/HackNPlan Report")]
	public class HackNPlanReport : ReportReaction {
		public HackNPlanData hackNPlanData;

		public override IEnumerator React(string title, string msg, byte[] screenshot) {
			var task = new HackNPlanData.Task
			{
				boardId = int.Parse(hackNPlanData.BoardId),
				title = $"v{Application.version} - {title}",
				description = "-----\n" +
				              $"v{Application.version} - {msg}",
				assignedUserIds = hackNPlanData.GetTaskUsers()
			};
			yield return HackNPlanUtility.Send(hackNPlanData, task, screenshot);
		}
	}
}