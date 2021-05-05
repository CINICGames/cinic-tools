using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;

namespace CinicGames.Tools.Git {

	public class GitWindow : EditorWindow, IPostBuildPlayerScriptDLLs {
		public enum VersionUpdate {
			Major = 0,
			Minor = 1,
			Patch = 2
		}

		private const string CommandsLocation = "Assets/Data/Editor/";
		private const string CommandsFileName = "git_commands.asset";

		private static readonly Vector2 WindowSize = new Vector2(500, 205);

		private void Title() {
			EditorUtilities.DrawTitle(dynamicTitle, 15, 5);
		}

		public GitCommands commands;

		public string branch = "playtest";
		private int selectedIndex = 0;

		public bool upgradeVersion = true;

		public bool manualChange;

		public string manualVersion;

		public VersionUpdate version = VersionUpdate.Patch;

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
			window.minSize = WindowSize;
			window.maxSize = WindowSize;
			Git.RunGitCommand("fetch origin");
		}

		private void Init() {
			string commandsPath = Path.Combine(CommandsLocation, CommandsFileName);
			commands = AssetDatabase.LoadAssetAtPath<GitCommands>(commandsPath);

			// If no asset found create it
			if (commands == null) {
				commands = CreateInstance<GitCommands>();
				Directory.CreateDirectory(CommandsLocation);
				AssetDatabase.CreateAsset(commands, commandsPath);
				EditorUtility.SetDirty(commands);
				AssetDatabase.SaveAssets();
			}

			branch = commands.PlaytestBranch;
			UpdateReferences();
			FixVersion();
			commitMessage = $"Version Upgrade: {VersionPreview()}";
		}

		private void OnGUI() {
			Title();

			if (canPush) {
				GUI.enabled = false;
				EditorGUILayout.Space();
				EditorGUILayout.ObjectField("Commands", commands, typeof(GitCommands), false);
				GUI.enabled = true;

				int newIndex = EditorGUILayout.Popup("Branch", selectedIndex, commands.Branches);
				if (newIndex != selectedIndex) {
					ValidateBranch(newIndex);
				}

				// Draw version upgrade
				Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
				var toggleRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
				var toolbarRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);
				upgradeVersion = EditorGUI.Toggle(toggleRect, "Upgrade Version", upgradeVersion);
				if (upgradeVersion) {
					manualChange = false;
					version = (VersionUpdate)GUI.Toolbar(toolbarRect, (int)version, Enum.GetNames(typeof(VersionUpdate)));
					UpdateCommit();
				}

				// Draw manual version upgrade
				rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
				toggleRect = new Rect(rect.x, rect.y, rect.width / 2, rect.height);
				var textRect = new Rect(rect.x + rect.width / 2, rect.y, rect.width / 4, rect.height);
				var buttonRect = new Rect(rect.x + rect.width * 3 / 4, rect.y, rect.width / 4, rect.height);
				manualChange = EditorGUI.Toggle(toggleRect, "Manual", manualChange);
				if (manualChange) {
					upgradeVersion = false;
					manualVersion = EditorGUI.TextField(textRect, manualVersion);
					if (GUI.Button(buttonRect, "Change")) {
						ManualVersionChange();
					}
				}

				// Commit message
				GUI.enabled = false;
				EditorGUILayout.TextField("Commit message", commitMessage);
				GUI.enabled = true;

				// Try build
				EditorGUILayout.Space();
				rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
				buttonRect = new Rect(rect.x, rect.y, rect.width / 4, rect.height);
				var colorRect = new Rect(rect.x + rect.width / 4 + 10, rect.y, rect.height, rect.height);
				if (GUI.Button(buttonRect, "Try Build")) {
					TryBuild();
				}

				EditorGUI.DrawRect(colorRect, buildResult);

				// Execute
				EditorGUILayout.Space();
				if (GUILayout.Button("Execute")) {
					Execute();
				}
			}
		}

		private void UpdateReferences() {
			currentBranch = Git.GetCurrentBranchName();
			manualVersion = VersionPreview();
			canPush = CheckIfCanPush(out dynamicTitle);
		}

		private void FixVersion() {
			if (!ValidateVersion(PlayerSettings.bundleVersion)) {
				string[] versionParts = PlayerSettings.bundleVersion.Split('.');

				// Get version if valid, fix it otherwise
				int major = versionParts.Length > 0 ? Convert.ToInt32(versionParts[0]) : 0;
				int minor = versionParts.Length > 1 ? Convert.ToInt32(versionParts[1]) : 0;
				int patch = versionParts.Length > 2 ? Convert.ToInt32(versionParts[2]) : 0;

				PlayerSettings.bundleVersion = $"{major}.{minor}.{patch}";
			}
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

		public void Execute() {
			if (!EditorUtility.DisplayDialog("Tested build", "Have you tried to manually build?", "Yes", "No")) {
				return;
			}

			if (manualChange) {
				EditorUtility.DisplayDialog("Manual Updating Version", "Please Uncheck \"Manual\" before pressing Execute", "Confirm");
				return;
			}
			AssetDatabase.SaveAssets();
			if (EditorUtility.DisplayDialog("Automatic Branch Update", GetDialogText(), "Confirm", "Cancel")) {
				if (upgradeVersion) {
					Git.UpdateBundleVersion(version == VersionUpdate.Major, version == VersionUpdate.Minor, version == VersionUpdate.Patch);
				}

				string x = ExecuteCommands();

				EditorUtility.ClearProgressBar();
				if (!string.IsNullOrEmpty(x)) {
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

				if (command.Contains("{push_branch}")) {
					command = command.Replace("{push_branch}", branch);
				}

				if (command.Contains("{authorized_branch}")) {
					command = command.Replace("{authorized_branch}", commands.AuthorizedBranch);
				}

				if (command.Contains("commit")) {
					if (upgradeVersion) {
						command += $" -m \"Version Upgrade: {PlayerSettings.bundleVersion}\"";
					} else {
						command += $" -m \"{commitMessage}\"";
					}
				}

				EditorUtility.DisplayProgressBar("Executing commands", command, (float)index / (commands.list.Length - 1));
				x += Git.RunGitCommand(command) + "\n";
			}
			return x;
		}

		public void ManualVersionChange() {
			if (ValidateVersion(manualVersion)) {
				PlayerSettings.bundleVersion = manualVersion;
				UpdateReferences();
				UpdateCommit();
				manualChange = false;
			} else {
				EditorUtility.DisplayDialog("Wrong Format", "Version must be in format *.*.*", "Ok");
				manualVersion = PlayerSettings.bundleVersion;
				GUI.FocusControl(null);
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

		private void ValidateBranch(int toValidate) {
			if (commands.Branches[toValidate] == commands.BuildBranch) {
				if (EditorUtility.DisplayDialog("Push Official Build",
					$"Confirm you want to push in {commands.BuildBranch} branch?",
					"Ok", "No")) {
					branch = commands.BuildBranch;
					selectedIndex = 1;
					return;
				}
				branch = commands.PlaytestBranch;
				selectedIndex = 0;
			}
		}

		private string VersionPreview() {
			string[] versionParts = PlayerSettings.bundleVersion.Split('.');

			// Get version if valid, fix it otherwise
			int major = versionParts.Length > 0 ? Convert.ToInt32(versionParts[0]) : 0;
			int minor = versionParts.Length > 1 ? Convert.ToInt32(versionParts[1]) : 0;
			int patch = versionParts.Length > 2 ? Convert.ToInt32(versionParts[2]) : 0;

			switch (version) {
			case VersionUpdate.Major:
				major++;
				minor = 0;
				patch = 0;
				break;
			case VersionUpdate.Minor:
				minor++;
				patch = 0;
				break;
			case VersionUpdate.Patch:
				patch++;
				break;
			}

			return $"{major}.{minor}.{patch}";
		}

		public void TryBuild() {
			enabled = true;
			EditorUtility.DisplayProgressBar("Try Build", "Trying to build", 0);
			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = new[] {"Assets/Scenes/Test/git_test_build.unity"},
				locationPathName = "Build",
				target = BuildTarget.StandaloneWindows,
				options = BuildOptions.BuildScriptsOnly
			};
			CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
			BuildPipeline.BuildPlayer(buildPlayerOptions);
		}

		private void OnCompilationFinished(string arg1, CompilerMessage[] arg2) {
			if (arg2.ToList().Exists(m => m.type == CompilerMessageType.Error)) {
				CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
				EditorUtility.DisplayDialog("Build Result", "Scripts building fail, fix errors before updating Playtest", "OK");
				buildResult = Color.red;
			} else if (arg1.Contains("Assembly-CSharp.")) {
				CompilationPipeline.assemblyCompilationFinished -= OnCompilationFinished;
				buildResult = Color.green;
				enabled = false;
				EditorUtility.DisplayDialog("Build Result", "Scripts built correctly, you can update Playtest", "OK");
				throw new BuildFailedException("Building canceled, NO Compilation ERRORS");
			}
		}

		public void OnPostBuildPlayerScriptDLLs(BuildReport report) {
			if (!enabled) {
				return;
			}

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

}