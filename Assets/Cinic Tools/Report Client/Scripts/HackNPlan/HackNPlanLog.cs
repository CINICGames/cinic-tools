using System.Collections;
using UnityEngine;

namespace CinicGames.Tools.Report.HackNPlan {
	[CreateAssetMenu(fileName = "hacknplan_log", menuName = "Cinic Tools/Report/HackNPlan/HackNPlan Log")]
	public class HackNPlanLog : LogReaction {
		public HackNPlanData hackNPlanData;

		public override IEnumerator React(string msg, string stack, LogType logType, byte[] screenshot) {
			var task = new HackNPlanData.Task
			{
				boardId = int.Parse(hackNPlanData.BoardId),
				title = $"v{Application.version} - {msg}",
				description = "-----\n" +
				              $"v{Application.version} - {msg}\n" +
				              $"```\n{stack}\n```",
				assignedUserIds = hackNPlanData.GetTaskUsers()
			};
			yield return HackNPlanUtility.Send(hackNPlanData, task, screenshot);
		}
	}
}