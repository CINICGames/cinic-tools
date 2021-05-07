using UnityEngine;

namespace CinicGames.Tools.Git {

	[CreateAssetMenu(menuName = "Cinic Tools/Git/Commands", fileName = "GitCommands")]
	public class GitCommands : ScriptableObject {

		public string AuthorizedBranch => authorizedBranch;
		[SerializeField] private string authorizedBranch = "master";

		public string BuildBranch => buildBranch;
		[SerializeField] private string buildBranch = "builds";

		public string PlaytestBranch => playtestBranch;
		[SerializeField] private string playtestBranch = "playtest";

		public string[] Branches => new[] {playtestBranch, buildBranch};

		public string[] list =
		{
			"add .",
			"commit",
			"push",
			"checkout {push_branch}",
			"fetch",
			"pull",
			"merge {authorized_branch}",
			"push",
			"checkout {authorized_branch}",
		};
	}

}