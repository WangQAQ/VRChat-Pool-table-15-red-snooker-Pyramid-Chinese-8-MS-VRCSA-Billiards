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
		GetWindow<UploadMapKey>("̨��Key�ϴ�");
	}

	// VRCAPIһ��ʺ�����Ƿ������ҳ����ģ������ĵ���û, �������������

	private async void OnGUI()
	{
		HttpClient httpClient = new HttpClient();
		var mapState = httpClient.GetAsync(KeyAPI);

		GUILayout.Label("��ʼ�������ؼ�������������MapKey", EditorStyles.largeLabel);

		GUILayout.Label("API�����޸�", EditorStyles.boldLabel);
		UrlAPI = EditorGUILayout.TextField("UrlKey", UrlAPI);
		KeyAPI = EditorGUILayout.TextField("UrlKey", KeyAPI);

		GUILayout.Label(message, EditorStyles.boldLabel);

		if (GUILayout.Button("�ϴ�����������"))
		{
			var state = await OnButtonClick(); // ��ť�����ʱ���ûص�����

			switch (state)
			{
				case 0:
					message = "�ϴ��ɹ�����ȴ����";
					break;
				case 1:
					message = "��������״��ϴ�";
					return;
				case 2:
					message = "�ϴ�ʧ�ܣ��Ƿ����ϴ���";
					return;
				default:
					message = "δ֪����";
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

	// ��ť�����Ļص�����
	// VRCAPI�Ķ������Ƿ���������ģ��������ð�
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
		// ÿ���ַ����Ա�ʾΪ 4 λ�������ƣ����� 8 λ��ASCII���������� Base64 ����
		byte[] randomBytes = new byte[length];
		using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(randomBytes);
		}

		// ʹ�� Base64 ���룬ʹ������ӿɶ�
		return Convert.ToBase64String(randomBytes).Substring(0, length);
	}
}
