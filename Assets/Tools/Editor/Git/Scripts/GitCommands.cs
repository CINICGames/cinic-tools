using UnityEngine;

[CreateAssetMenu(menuName = "Data/Git/Commands", fileName = "git_commands")]
public class GitCommands : ScriptableObject {
	
	public string AuthorizedBranch => authorizedBranch;
	[SerializeField] private string authorizedBranch = "master";
	
	public string BuildBranch => buildBranch;
	[SerializeField] private string buildBranch = "builds";
	
	public string PlaytestBranch => playtestBranch;
	[SerializeField] private string playtestBranch = "playtest";

	public string[] Branches => new[] {buildBranch, playtestBranch};
	
	// [InfoBox("Use {push_branch} and {authorized_branch} labels in commands instead of branch names")]
	public string[] list = {
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