using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.ProgramSources;

public class ColorDownload : UdonSharpBehaviour
{

    /// <summary>
    /// 2024/9/27
    /// By WangQAQ
    /// </summary>

    //��ɫ��������URL
    [Header("URL")]
    [SerializeField] public VRCUrl[] url;

    //���ڱ�ʾ��ǰ���صڼ���URL 
    private int reloadStep = 0;

    /// <summary>
    /// ���������ɫ��
    /// NameΪ��������飬Color�������ɫ���±�һһ��Ӧ
    /// </summary>
    private string[] Name = null;                                                   //�����������
    private string[] Color = null;                                                  //�����ɫ��

    /// <summary>
    /// �¼ӣ����ڱ�ʾ�����Ƿ��ʼ��
    /// </summary>
    private bool isStringInit = false;

    void Start()
    {
        VRCStringDownloader.LoadUrl(url[0], (IUdonEventReceiver)this);
    }

    // �ַ������سɹ��ص�
    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        //����ַ�������;���
        //��ǰ�ַ�����Ӧ��Ϊ "Name","Color"
        string[] ListTmp = result.Result.Split(';', StringSplitOptions.RemoveEmptyEntries);

        //��ʼ������
        Name = new string[ListTmp.Length];
        Color = new string[ListTmp.Length];

        //����ڴ�����ɹ��������������ʼ������Ϊtrue
        if(Name != null && Color != null)
        {
            isStringInit = true;    
        }

        //ѭ�����������Ͳ�ɫ���� O(N)
        for (int i = 0;i < ListTmp.Length; i++)
        {
            //�п�
            if (ListTmp[i] != null)
            {
                //�� �� �ָ��ַ������ָ�Ϊ������Ͳ�ɫ����
                string[] ColorTmp = ListTmp[i].Split(',', StringSplitOptions.RemoveEmptyEntries);

                //DEBUG
                //Debug.Log("Name:" + ColorTmp.Length);

                //������� == 2 ��¼�� (Split���ܻ��һλ�����飬unity��bug)
                if (ColorTmp.Length == 2)
                {
                    Name[i] = ColorTmp[0];
                    Color[i] = ColorTmp[1];
                }
            }
        }
    }

    //�ַ�������ʧ�ܻص�
    public override void OnStringLoadError(IVRCStringDownload result)
    {
        //ѭ�����Լ���url���鼯�е�URL
        if (reloadStep < url.Length)
        {
            //���û�м��ص����һ��URL�������URL���鼯�е���һ��URL
            SendCustomEventDelayedSeconds("_AutoReloadColor", 10);
            reloadStep++;
        }
        else
        {
            //��������һ��URL����ӵ�һ��URL��ʼ����
            SendCustomEventDelayedSeconds("_AutoReloadColor", 10);
            reloadStep = 0;
        }
    }

    //���¼����ַ�������
    public void _AutoReloadColor()
    {
        //VRC����API
        VRCStringDownloader.LoadUrl(url[reloadStep], (IUdonEventReceiver)this);
    }

    //��ȡ��Ҷ�Ӧ��ɫID
    public string GetColorColor(string name)
    {
        if(Name != null && isStringInit == true)
        {
            //ѭ����ȡ��ɫ״̬ O(N)
            for (int i = 0; i < Name.Length; i++)
            {
                if (Name[i] == name)
                {
                    return Color[i];
                }
            }
        }
        return null;
    }
}
