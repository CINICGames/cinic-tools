using System.Collections;
using UnityEngine;

namespace CinicGames.Tools.Report {
	public abstract class ReportReaction : ScriptableObject {
		public abstract IEnumerator React(string title, string msg, byte[] screenshot);
	}
}