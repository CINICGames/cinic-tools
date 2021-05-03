using UnityEngine;

[CreateAssetMenu(menuName = "Data/Git/Commands", fileName = "git_commands")]
public class GitCommands : ScriptableObject {
	
	public string AuthorizedBranch => authorizedBranch;
	[SerializeField] private string authorizedBranch = default;
	
	public string BuildBranch => buildBranch;
	[SerializeField] private string buildBranch = default;
	
	public string PlaytestBranch => playtestBranch;
	[SerializeField] private string playtestBranch = default;
	
	// [InfoBox("Use {push_branch} and {authorized_branch} labels in commands instead of branch names")]
	public string[] list;

	
}