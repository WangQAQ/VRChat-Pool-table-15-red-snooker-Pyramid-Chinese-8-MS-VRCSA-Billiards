#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Editor;
using VRC.SDKBase;
using VRC.Core;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using VRC.SDKBase.Editor.Api;
using System.Security.Cryptography;

namespace WangQAQ.Plug
{
	public class KeyUpload : MonoBehaviour, IEditorOnly
	{

		private const string urlAPI = "https://www.wangqaq.com/AspAPI/table/UploadScore";
		private const string keyAPI = "https://www.wangqaq.com/AspAPI/table/UploadMapKey";

		private const string keyFilePath = "Assets/";
		private const string keyFileName = "VRChatPoolMapKey.txt";

		private static bool isNeedUploadKey = false;

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
				builder.OnSdkUploadSuccess += OnUploadSuccess;
			}
		}

		#region Func

		private static bool isKeyFileHas(string GUID)
		{
			if (File.Exists(keyFilePath + GUID + keyFileName))
			{
				return true;
			}
			return false;
		}

		private static bool createKeyFile(string GUID, string Key)
		{
			string path = keyFilePath + GUID + keyFileName;
			File.WriteAllText(path, Key + "||" + GUID);

			return true;
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

		#endregion

		#region SDK_Event

		private static void OnBuildStarted(object sender, object target)
		{
			if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkWorldBuilderApi>(out var builder))
			{
				var pipelineOBJ = FindObjectsOfType<PipelineManager>().SingleOrDefault();
				if (pipelineOBJ == null || pipelineOBJ.GetType() != typeof(PipelineManager))
				{
					builder.CancelUpload();
					return;
				}

				var tmp = pipelineOBJ.blueprintId.Split("_", StringSplitOptions.RemoveEmptyEntries);
				if (tmp.Length == 2)
				{
					//�����Ƿ���ڸ� GUID key �ļ�
					if (!isKeyFileHas(tmp[1]))
					{
						isNeedUploadKey = true;
					}
					return;
				}
				else
				{
					isNeedUploadKey = true;                 // û���ҵ� WorldGUID �ȴ��ϴ���ɺ��ϴ���Կ
					return;
				}
			}
		}

		private static async void OnUploadSuccess(object sender, object target)
		{
			if (VRCSdkControlPanel.TryGetBuilder<IVRCSdkWorldBuilderApi>(out var builder))
			{
				if (isNeedUploadKey)
				{
					isNeedUploadKey = false;

					// ��ʼ������
					HttpClient httpClient = new HttpClient();
					httpClient.Timeout = TimeSpan.FromSeconds(15);
					httpClient.DefaultRequestHeaders.Add("User-Agent", "UnityPlayer");
					var pipelineOBJ = FindObjectsOfType<PipelineManager>().SingleOrDefault();

					var GUID = "";
					var key = GenerateRandomKey(32);
					string Name = string.Empty;

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

					// ��ȡ��������
					var vrcWorldOBJ = await VRCApi.GetWorld(pipelineOBJ.blueprintId);

					if (vrcWorldOBJ.Name != null)
					{
						Name = vrcWorldOBJ.Name;
					}
					else
					{
						return;
					}

					if (!isKeyFileHas(GUID))
					{
						var formContent = new FormUrlEncodedContent(new[]
						{
							new KeyValuePair<string, string>("Name", Name),
							new KeyValuePair<string, string>("WorldGUID", GUID),
							new KeyValuePair<string, string>("Key",key)
						});

						var response = await httpClient.PostAsync(keyAPI, formContent);

						if (response.StatusCode != HttpStatusCode.OK)
						{
							return;
						}

						createKeyFile(GUID, key);
					}

					await builder.BuildAndUpload(vrcWorldOBJ);

					return;
				}
			}
		}

		#endregion

	}
}
#endif