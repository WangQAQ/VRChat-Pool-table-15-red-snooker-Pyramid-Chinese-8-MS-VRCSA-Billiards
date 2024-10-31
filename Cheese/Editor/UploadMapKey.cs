using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase.Editor.Api;
using static VRC.Core.ApiInfoPushSystem;

public class UploadMapKey : EditorWindow
{ 
	[HideInInspector] public string UrlAPI = "https://www.wangqaq.com/AspAPI/table/UploadScore";
	[HideInInspector] public string KeyAPI = "https://www.wangqaq.com/AspAPI/table/UploadMapKey";

	private string message;
	private string key;
	private Guid WorldGuid = Guid.Empty;

	[MenuItem("MS-VRCSA/Upload Map Key")]
	static void OpenMenu()
	{
		GetWindow<UploadMapKey>("台球Key上传");
	}

	// VRCAPI一坨屎，都是反编译找出来的，连个文档都没, 懒得整理代码了

	private async void OnGUI()
	{
		HttpClient httpClient = new HttpClient();
		var mapState = httpClient.GetAsync(KeyAPI);

		GUILayout.Label("初始化分数控件和项服务器添加MapKey", EditorStyles.largeLabel);

		GUILayout.Label("API参数修改", EditorStyles.boldLabel);
		UrlAPI = EditorGUILayout.TextField("UrlKey", UrlAPI);
		KeyAPI = EditorGUILayout.TextField("UrlKey", KeyAPI);

		GUILayout.Label(message, EditorStyles.boldLabel);

		if (GUILayout.Button("上传分数并生成"))
		{
			var state = await OnButtonClick(); // 按钮被点击时调用回调方法

			switch (state)
			{
				case 0:
					message = "上传成功，请等待审核";
					break;
				case 1:
					message = "请先完成首次上传";
					return;
				case 2:
					message = "上传失败，是否已上传过";
					return;
				default:
					message = "未知错误";
					return;
			}

			var uploadOBJ = FindObjectsOfType<RankingSystem>().ToList();

			foreach (var obj in uploadOBJ)
			{
				obj.hashKey = key;
				obj.ScoreUploadBaseURL = UrlAPI;
				obj.WorldGUID = WorldGuid.ToString();
			}
		}
	}

	// 按钮点击后的回调方法
	// VRCAPI的东西都是反编译出来的，将就着用吧
	private async Task<int> OnButtonClick()
	{
		HttpClient httpClient = new HttpClient();
		httpClient.DefaultRequestHeaders.Add("User-Agent", "UnityPlayer");
		var pipelineOBJ = FindObjectsOfType<PipelineManager>().SingleOrDefault();

		key = GenerateRandomKey(32);
		string Name = string.Empty;

		// GUID
		if(pipelineOBJ.GetType() == typeof(PipelineManager))
		{
			var tmp = pipelineOBJ.blueprintId.Split("_",StringSplitOptions.RemoveEmptyEntries);

			if (tmp.Length == 2)
				WorldGuid = Guid.Parse(tmp[1]);
			else
				return 1;
		}
		else
		{
			return 1;
		}

		// Name
		var vrcWorldOBJ = await VRCApi.GetWorld(pipelineOBJ.blueprintId);

		if(vrcWorldOBJ.Name != null)
		{
			Name = vrcWorldOBJ.Name;
		}
		else
		{
			return -1;
		}

		var formContent = new FormUrlEncodedContent(new[]
		{
			new KeyValuePair<string, string>("Name", Name),
			new KeyValuePair<string, string>("WorldGUID", WorldGuid.ToString()),
			new KeyValuePair<string, string>("Key",key)
		});

		var response = await httpClient.PostAsync(KeyAPI, formContent);

		if(response.StatusCode != HttpStatusCode.OK)
		{
			return 2;
		}

		return 0;
	}

	public static string GenerateRandomKey(int length)
	{
		// 每个字符可以表示为 4 位（二进制）或者 8 位（ASCII），我们用 Base64 编码
		byte[] randomBytes = new byte[length];
		using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(randomBytes);
		}

		// 使用 Base64 编码，使结果更加可读
		return Convert.ToBase64String(randomBytes).Substring(0, length);
	}
}
