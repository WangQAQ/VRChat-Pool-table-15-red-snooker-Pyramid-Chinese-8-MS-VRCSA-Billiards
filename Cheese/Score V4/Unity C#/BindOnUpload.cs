#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Editor;
using VRC.SDKBase;

public class BindOnUpload : MonoBehaviour , IEditorOnly
{
	public const string UrlAPI = "https://www.wangqaq.com/AspAPI/table/UploadScore";

	[InitializeOnLoadMethod]
	public static void RegisterSDKCallback()
	{
		VRCSdkControlPanel.OnSdkPanelEnable += AddBuildHook;
	}

	private static void AddBuildHook(object sender, EventArgs e)
	{
		if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkWorldBuilderApi>(out var builder))
		{
			builder.OnSdkBuildStart += OnBuildStarted;
		}
	}

	private static void OnBuildStarted(object sender, object target)
	{
		// ��ʼ������
		var pipelineOBJ = FindObjectsOfType<PipelineManager>().SingleOrDefault();
		var GUID = "";

		// ��ȡ����GUID
		if (pipelineOBJ != null)
		{
			if (pipelineOBJ.GetType() == typeof(PipelineManager))
			{
				var tmp = pipelineOBJ.blueprintId.Split("_", StringSplitOptions.RemoveEmptyEntries);

				if (tmp.Length == 2)
					GUID = tmp[1];
				else
					return;
			}
			else
			{
				return;
			}
		}
		else
		{
			return;
		}

		string path = "Assets/" + GUID + "VRChatPoolMapKey.txt";
		if (File.Exists(path))
		{
			var uploadOBJ = Resources.FindObjectsOfTypeAll<RankingSystem>().ToList();
			var tmp = File.ReadAllText(path).Split("||");

			if (tmp.Length != 2)
			{
				return;
			}

			var tmpKey = tmp[0];
			var tmpGuid = tmp[1];

			foreach (var obj in uploadOBJ)
			{
				obj.useV2API = true;
				obj.hashKey = tmpKey;
				obj.ScoreUploadBaseURL = UrlAPI;
				obj.WorldGUID = tmpGuid;
			}
		}
	}

}
#endif