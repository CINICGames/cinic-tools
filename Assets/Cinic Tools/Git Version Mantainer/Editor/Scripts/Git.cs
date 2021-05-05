using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CinicGames.Tools.Git {

    public static class Git {
        public static string RunGitCommand(string gitCommand) {
            // Strings that will catch the output from our process.
            string output = "no-git";
            string errorOutput = "no-git";

            // Set up our processInfo to run the git command and log to output and errorOutput.
            ProcessStartInfo processInfo = new ProcessStartInfo("git", @gitCommand)
            {
                CreateNoWindow = true, // We want no visible pop-ups
                UseShellExecute = false, // Allows us to redirect input, output and error streams
                RedirectStandardOutput = true, // Allows us to read the output stream
                RedirectStandardError = true // Allows us to read the error stream
            };

            // Set up the Process
            Process process = new Process
            {
                StartInfo = processInfo
            };

            try {
                process.Start(); // Try to start it, catching any exceptions if it fails
            } catch (Exception e) {
                // For now just assume its failed cause it can't find git.
                Debug.LogError("Git is not set-up correctly, required to be on PATH, and to be a git project.");
                throw e;
            }

            // Read the results back from the process so we can get the output and check for errors
            output = process.StandardOutput.ReadToEnd();
            errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit(); // Make sure we wait till the process has fully finished.
            process.Close(); // Close the process ensuring it frees it resources.

            // Check for failure due to no git setup in the project itself or other fatal errors from git.
            if (output.Contains("fatal") || output == "no-git" /* || output == "" */) {
                throw new Exception("Command: git " + @gitCommand + " Failed\n" + output + errorOutput);
            }
            // Log any errors.
            if (errorOutput != "") {
                Debug.LogWarning("Git: " + errorOutput);
            }

            return output; // Return the output from git.
        }

        public static string CurrentCommitHash() {
            string result = RunGitCommand("rev-parse --short --verify HEAD");
            // Clean up whitespace around hash. (seems to just be the way this command returns :/ )
            result = string.Join("", result.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return result;
        }

        public static string BranchLastCommitHash(string branch) {
            string result = RunGitCommand($"rev-parse --short {branch}");
            // Clean up whitespace around hash. (seems to just be the way this command returns :/ )
            result = string.Join("", result.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
            return result;
        }

        public static string GetCurrentBranchName() {
            string output = RunGitCommand("branch --show-current");
            return output.Remove(output.Length - 1, 1);
        }

        public static void UpdateBundleVersion(bool isMajor, bool isMinor, bool isPatch) {
            int major, minor, patch;

            try {
                // Cut up the version into Major.Minor.Patch.Commit.
                string currentVersion = PlayerSettings.bundleVersion;
                major = Convert.ToInt32(currentVersion.Split('.')[0]);
                minor = Convert.ToInt32(currentVersion.Split('.')[1]);
                patch = Convert.ToInt32(currentVersion.Split('.')[2]);
            } catch (Exception e) {
                // Most likely failed to get major/minor/patch in correct format.
                Debug.LogErrorFormat("Unexpected bundleVersion Format: {0}\nExpected format: *.*.* ", PlayerSettings.bundleVersion);
                throw e;
            }
            if (isMajor) {
                major++;
                minor = 0;
                patch = 0;
            } else if (isMinor) {
                minor++;
                patch = 0;
            } else if (isPatch) {
                patch++;
            }
            // Update to the new version
            PlayerSettings.bundleVersion = string.Format("{0}.{1}.{2}", major, minor, patch);
            PlayerSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

            Debug.LogFormat("Updated Game Version: {0}.{1}.{2}", major, minor, patch);
            // This is to ensure that it is saved properly, if you were to build afterwards it would reset to library setting.
            AssetDatabase.SaveAssets();
            // This Ensures that it will be used if we build directly after calling the function.
            AssetDatabase.Refresh();
        }
    }

}