using System;
using System.Linq;
using System.Text.RegularExpressions;
// using Sirenix.OdinInspector;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities;
// using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

public class GitWindow : EditorWindow, IPostBuildPlayerScriptDLLs {
	public enum VersionUpdate {
		Major,
		Minor,
		Patch
	}

	private string[] branches;

	private const string CommandsPath = "Assets/Data/Editor/git_commands.asset";
	
	// [OnInspectorGUI, PropertyOrder(0)]
	private void Title() {
		EditorUtilities.DrawTitle(dynamicTitle, 15, 5);
	}

	// [PropertySpace(10)]
	// [ShowIf(nameof(canPush)), ReadOnly, PropertyOrder(1)] 
	public GitCommands commands;

	// [PropertyOrder(1), ValueDropdown(nameof(branches)), OnValueChanged(nameof(ValidateBranch)), ShowIf(nameof(canPush))]
	public string branch = "playtest";
	
	// [ShowIf(nameof(canPush)), HorizontalGroup("Automatic"), PropertyOrder(2), OnValueChanged(nameof(UpdateCommit))]
	// [LabelWidth(110)]
	public bool upgradeVersion = true;

	// [PropertySpace(3), PropertyOrder(3), ShowIf("@canPush && upgradeVersion")]
	// [HorizontalGroup("Manual"), LabelText("Manual"), LabelWidth(110), ShowIf(nameof(upgradeVersion))]
	public bool manualChange;
	
	// [PropertySpace(3), PropertyOrder(3), ShowIf(nameof(canPush))]
	// [ShowIf("@canPush && upgradeVersion"), HideLabel, DisableIf("@manualChange == false"), HorizontalGroup("Manual"), ShowInInspector]
	// [ValidateInput(nameof(ValidateVersion), "Wrong Format: version must be in format *.*.* ")]
	public string manualVersion;
	
	// [PropertySpace(10)]
	// [ShowIf("@canPush && upgradeVersion"), HorizontalGroup("Automatic"), EnumToggleButtons, HideLabel, PropertyOrder(2)]
	// [OnValueChanged(nameof(UpdateCommit)), ShowInInspector]
	public VersionUpdate version = VersionUpdate.Patch;
	
	// [DisableIf(nameof(upgradeVersion)), HideIf("@canPush == false || manualChange"), PropertyOrder(4)] 
	// [LabelWidth(110), ShowInInspector]
	public string commitMessage;

	private string dynamicTitle;
	private string currentBranch;
	private bool canPush;
	private string previousCommitMessage;
	private bool enabled;
	public int callbackOrder => 0;
	private Color buildResult = Color.yellow;

	[MenuItem("Tools/Git/Update Playtest #p")]
	private static void OpenWindow() {
		var window = GetWindow<GitWindow>();
		// window.position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 240);
		window.Init();
		Git.RunGitCommand("fetch origin");

	}

	private void Init() {
		commands = AssetDatabase.LoadAssetAtPath<GitCommands>(CommandsPath);
		branch = commands.PlaytestBranch;
		branches = new[] {commands.PlaytestBranch, commands.BuildBranch};
		UpdateReferences();
		commitMessage = $"Version Upgrade: {VersionPreview()}";
	}

	private void UpdateReferences() {
		currentBranch = Git.GetCurrentBranchName();
		manualVersion = PlayerSettings.bundleVersion;
		canPush = CheckIfCanPush(out dynamicTitle);
	}

	private bool CheckIfCanPush(out string titleText) {
		if (currentBranch != commands.AuthorizedBranch) {
			titleText = $"Error: Current branch [{currentBranch}] - Authorized branch [{commands.AuthorizedBranch}]";
			return false;
		}
		string current = Git.CurrentCommitHash();
		string last = Git.BranchLastCommitHash(currentBranch);
		if (current != last) {
			titleText = $"Error: Current commit [{current}] - Last commit [{last}]. Pull first";
			return false;
		}
		titleText = $"Current version [{PlayerSettings.bundleVersion}] - Current branch [{currentBranch}]";
		return true;
	}

	// [PropertySpace(10)]
	// [Button(ButtonSizes.Medium), ShowIf(nameof(canPush)), PropertyOrder(5), GUIColor(nameof(GetColor))]
	public void Execute() {
		if (!EditorUtility.DisplayDialog("Tested build", "Have you tried to manually build?", "Yes", "No"))
			return;
			
		if (manualChange) {
			EditorUtility.DisplayDialog("Manual Updating Version", "Please Uncheck \"Manual\" before pressing Execute", "Confirm");
			return;
		}
		AssetDatabase.SaveAssets();
		if (EditorUtility.DisplayDialog("Automatic Branch Update", GetDialogText(), "Confirm", "Cancel")) {
			if (upgradeVersion) {
				Git.UpdateBundleVersion(version==VersionUpdate.Major, version==VersionUpdate.Minor, version==VersionUpdate.Patch);
			}
		
			string x = ExecuteCommands();

			EditorUtility.ClearProgressBar();
			if(!string.IsNullOrEmpty(x)) {
				Debug.Log(x);
			}
			EditorUtility.DisplayDialog("Complete", 
				"Current version: " + PlayerSettings.bundleVersion + "\n" +
				"Current Commit: " + Git.CurrentCommitHash(), 
				"Close");
			UpdateReferences();
		}
		previousCommitMessage = string.Empty;
		GUIUtility.ExitGUI();
	}

	private string ExecuteCommands() {
		string x = "";
		for (int index = 0; index < commands.list.Length; index++) {
			string command = commands.list[index];
			
			if (command.Contains("{push_branch}")) 
				command = command.Replace("{push_branch}", branch);
			
			if (command.Contains("{authorized_branch}")) 
				command = command.Replace("{authorized_branch}", commands.AuthorizedBranch);
			
			if (command.Contains("commit")) {
				if (upgradeVersion) {
					command += $" -m \"Version Upgrade: {PlayerSettings.bundleVersion}\"";
				}
				else {
					command += $" -m \"{commitMessage}\"";
				}
			}

			EditorUtility.DisplayProgressBar("Executing commands", command, (float)index/(commands.list.Length-1));
			x += Git.RunGitCommand(command) + "\n";
		}
		return x;
	}

	// [PropertySpace(3), PropertyOrder(3)]
	// [Button, ShowIf("@canPush && upgradeVersion"), DisableIf("@manualChange == false"), HorizontalGroup("Manual", MinWidth = 100), LabelText("Change")]
	public void ManualVersionChange() {
		if (ValidateVersion(manualVersion)) {
			PlayerSettings.bundleVersion = manualVersion;
			UpdateReferences();
			UpdateCommit();
			manualChange = false;
		}
	}
	
	private string GetDialogText() {
		string comm = string.Join("\n    ", commands.list);
		comm = comm.Replace("{branch}", branch);
		return $"Are you sure to perform this commands?\n    {comm}";
	}

	private void UpdateCommit() {
		if (upgradeVersion) {
			previousCommitMessage = commitMessage;
			commitMessage = commitMessage = $"Version Upgrade: {VersionPreview()}";
			return;
		}
		commitMessage = previousCommitMessage;
	}

	private bool ValidateVersion(string toValidate) {
		const string regexPattern = "^\\d+.\\d+.\\d+$";
		return Regex.IsMatch(toValidate, regexPattern);
	}

	private void ValidateBranch(string toValidate) {
		if (toValidate == commands.BuildBranch) {
			if (EditorUtility.DisplayDialog("Push Official Build",
				$"Confirm you want to push in {commands.BuildBranch} branch?",
				"Ok", "No")) {
				branch = commands.BuildBranch;
				return;
			}
			branch = commands.PlaytestBranch;
		}
	}

	private string VersionPreview() {
		int major, minor, patch;
		string currentVersion = PlayerSettings.bundleVersion;
		major = Convert.ToInt32(currentVersion.Split('.')[0]);
		minor = Convert.ToInt32(currentVersion.Split('.')[1]);
		patch = Convert.ToInt32(currentVersion.Split('.')[2]);
		if (version == VersionUpdate.Major) {
			major++;
			minor = 0;
			patch = 0;
		}
		else if (version == VersionUpdate.Minor) {
			minor++;
			patch = 0;
		}
		else if (version == VersionUpdate.Patch) {
			patch++;
		}
		return string.Format("{0}.{1}.{2}", major, minor, patch);
	}
	
	// [PropertySpace(10), Button, PropertyOrder(4), HorizontalGroup("TryBuild"), ShowIf("@canPush")]
	public void TryBuild () {
		enabled = true;
		EditorUtility.DisplayProgressBar("Try Build", "Trying to build", 0);
		var buildPlayerOptions = new BuildPlayerOptions
		{
			scenes = new []{"Assets/Scenes/Test/git_test_build.unity"},
			locationPathName = "Build", 
			target = BuildTarget.StandaloneWindows, 
			options = BuildOptions.BuildScriptsOnly
		};
		CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}
	
	// [PropertySpace(10), OnInspectorGUI, PropertyOrder(4), HorizontalGroup("TryBuild"), ShowIf("@canPush")]
	private void BuildResult() {
		EditorUtilities.DrawBox(new Vector2(16,16), buildResult, 10);
	}

	private void OnCompilationFinished(string arg1, CompilerMessage[] arg2) {
		if (arg2.ToList().Exists(m => m.type == CompilerMessageType.Error)) {
			CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
			EditorUtility.DisplayDialog("Build Result", "Scripts building fail, fix errors before updating Playtest", "OK");
			buildResult = Color.red;
		}
		else if (arg1.Contains("Assembly-CSharp.")) {
			CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
			buildResult = Color.green;
			enabled = false;
			EditorUtility.DisplayDialog("Build Result", "Scripts built correctly, you can update Playtest", "OK");
			throw new BuildFailedException("Building canceled, NO Compilation ERRORS");
		}
	}

	public void OnPostBuildPlayerScriptDLLs(BuildReport report) {
		if (!enabled) 
			return;
		
		enabled = false;
		CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
		buildResult = Color.green;
		EditorUtility.DisplayDialog("Build Result", "Scripts built correctly, you can update Playtest", "OK");
		throw new BuildFailedException("Building canceled, NO Compilation ERRORS");
	}

	private Color GetColor() {
		return branch == commands.BuildBranch ? Color.red : Color.white;
	}
}