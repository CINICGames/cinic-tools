using System.Collections;
using UnityEngine;

namespace CinicGames.Tools.Report {
	public abstract class LogReaction : ScriptableObject {
		public abstract IEnumerator React(string msg, string stack, LogType logType, byte[] screenshot);
	}
}