using System.Collections;
using RestSharp;
using UnityEngine;
using UnityEngine.Networking;

namespace CinicGames.Tools.Report.HackNPlan {
	public static class HackNPlanUtility {
		
		public static IEnumerator Send(HackNPlanData data, HackNPlanData.Task task, byte[] screenshot) {
			string jsonBody = JsonUtility.ToJson(task);
			UnityWebRequest www = UnityWebRequest.Put($"https://api.hacknplan.com/v0/projects/{data.ProjectId}/workitems", jsonBody);
			www.method = "POST";
			www.SetRequestHeader("Content-Type", "application/json");
			www.SetRequestHeader("Authorization", $"ApiKey {data.ApiKey}");
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError) {
				Debug.LogError(www.error);
			}
			else {
				Debug.Log(www.downloadHandler.text);
				var newTask = (HackNPlanData.Task)JsonUtility.FromJson(www.downloadHandler.text, typeof(HackNPlanData.Task));
				// attachment
				yield return new WaitForEndOfFrame();
				var client = new RestClient($"https://api.hacknplan.com/v0/projects/{newTask.projectId}/workitems/{newTask.workItemId}/attachments")
				{
					Timeout = -1
				};
				var request = new RestRequest(Method.POST);
				request.AddHeader("Content-Type", "multipart/form-data");
				request.AddHeader("Authorization", $"ApiKey {data.ApiKey}");
				request.AddFile("screenshot", screenshot, "screenshot.png");
				client.Execute(request);
			}
		}
	}
}