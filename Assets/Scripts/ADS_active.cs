using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TwinCAT.Ads;

public class ADS_active : MonoBehaviour
{
    // Communication with twincat
    private int hButtonTask1, hButtonTask2, hButtonTask3;   // 任务句柄
    private bool bButtonTask1, bButtonTask2, bButtonTask3;      // 任务选择
    private int hButtonActive, hButtonPassive;      // 主被动模式句柄
    private bool bButtonActive, bButtonPassive;     // 主被动模式选择
    private int hButtonStart;
    private bool bButtonStart;  // 程序开始
    private int hButtonMove, hButtonStop, hButtonExit, hButtonZero;
    private bool bButtonMove, bButtonStop, bButtonExit, bButtonZero;    // 运动控制键
    private bool bLightOrigOn, bLightDestOn;       // 起点和终点指示灯
    public Int16 nInBoundaryX { get; set; }   // x方向到达边界，1为正方向，2为负方向
    public Int16 nInBoundaryY { get; set; }   // y方向到达边界，1为正方向，2为负方向
    private float x_end, y_end; // 机械臂末端位置
    public float xPos { private set; get; } //转换后机械臂末端位置
    public float yPos { private set; get; }
    private float scaleTureMonitor = 1.6f;     // 显示屏与真实距离的比例
    public bool bAllowMoing { private set; get; }    // 点击开始运动键后允许运动

    //private readonly string AMSNETID = "192.168.102.24.1.1";//笔记本的AMSnetid
    private readonly string AMSNETID = "169.254.176.110.1.1";//控制器的AMSnetid
    private readonly int PORT = 501, PORTMAIN = 851;//目标的端口号
    private TcAdsClient adsClient = new TcAdsClient(), adsClientMain = new TcAdsClient();//ads客户端

    // 状态选择
    private bool bTaskChoose;   // 已选择任务
    private bool bModeChoose;   // 已选择模式
    private int iTask;           // 选择的任务
    public static string FormalSubjectNumber { set; get; } = "test";  // 受试者编号
    private string SubjectNumberKey;    //Key

    private void Awake()
    {
        // connect ADS 客户端
        adsClient.Connect(AMSNETID, PORT);
        adsClientMain.Connect(AMSNETID, PORTMAIN);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            PlayerPrefs.SetString(SubjectNumberKey, FormalSubjectNumber);
            x_end = Convert.ToSingle(adsClientMain.ReadAny(0x00004040, 0x00064EA0, typeof(Double)));
            y_end = Convert.ToSingle(adsClientMain.ReadAny(0x00004040, 0x00064EA8, typeof(Double)));
            xPos = y_end * scaleTureMonitor;
            yPos = (-x_end + 285) * scaleTureMonitor;   // 45/scaleY为显示屏与真实情况外壳的中心的距离
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
        // escape退出
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            adsClientMain.Dispose();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
    }
    //****************************** Task *********************
    public void ActTask1()
    {
        try
        {
            bButtonTask1 = true;
            hButtonTask1 = adsClientMain.CreateVariableHandle("MAIN.bButtonTask1");
            adsClientMain.WriteAny(hButtonTask1, bButtonTask1);
            adsClientMain.DeleteVariableHandle(hButtonTask1);
            bButtonTask1 = false;
            bTaskChoose = true;
            iTask = 1;
            Debug.Log("Task1");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActTask2()
    {
        try
        {
            bButtonTask2 = true;
            hButtonTask2 = adsClientMain.CreateVariableHandle("MAIN.bButtonTask2");
            adsClientMain.WriteAny(hButtonTask2, bButtonTask2);
            adsClientMain.DeleteVariableHandle(hButtonTask2);
            bButtonTask2 = false;
            bTaskChoose = true;
            iTask = 2;
            Debug.Log("Task2");

        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActTask3()
    {
        try
        {
            bButtonTask3 = true;
            hButtonTask3 = adsClientMain.CreateVariableHandle("MAIN.bButtonTask3");
            adsClientMain.WriteAny(hButtonTask3, bButtonTask3);
            adsClientMain.DeleteVariableHandle(hButtonTask3);
            bButtonTask3 = false;
            bTaskChoose = true;
            iTask = 3;
            Debug.Log("Task3");

        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    //*************************** Motion Control ***********************
    public void ActMove()
    {
        try
        {
            bButtonMove = true;
            hButtonMove = adsClientMain.CreateVariableHandle("MAIN.bButtonMove");
            adsClientMain.WriteAny(hButtonMove, bButtonMove);
            adsClientMain.DeleteVariableHandle(hButtonMove);
            bButtonMove = false;
            bAllowMoing = true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActStop()
    {
        try
        {
            bButtonStop = true;
            hButtonStop = adsClientMain.CreateVariableHandle("MAIN.bButtonStop");
            adsClientMain.WriteAny(hButtonStop, bButtonStop);
            adsClientMain.DeleteVariableHandle(hButtonStop);
            bButtonStop = false;
            bAllowMoing = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActExit()
    {
        try
        {
            bButtonExit = true;
            hButtonExit = adsClientMain.CreateVariableHandle("MAIN.bButtonExit");
            adsClientMain.WriteAny(hButtonExit, bButtonExit);
            adsClientMain.DeleteVariableHandle(hButtonExit);
            bButtonExit = false;
            SceneManager.LoadScene("Menu");
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActZero()
    {
        try
        {
            bButtonZero = true;
            hButtonZero = adsClientMain.CreateVariableHandle("MAIN.bButtonZero");
            adsClientMain.WriteAny(hButtonZero, bButtonZero);
            adsClientMain.DeleteVariableHandle(hButtonZero);
            bButtonZero = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    //************************ Mode ******************************
    public void ActActive()
    {
        try
        {
            bButtonActive = true;
            hButtonActive = adsClientMain.CreateVariableHandle("MAIN.bButtonActive");
            adsClientMain.WriteAny(hButtonActive, bButtonActive);
            adsClientMain.DeleteVariableHandle(hButtonActive);
            bButtonActive = false;
            bModeChoose = true;
            Debug.Log("Active");

        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    public void ActPassive()
    {
        try
        {
            bButtonPassive = true;
            hButtonPassive = adsClientMain.CreateVariableHandle("MAIN.bButtonPassive");
            adsClientMain.WriteAny(hButtonPassive, bButtonPassive);
            adsClientMain.DeleteVariableHandle(hButtonPassive);
            bButtonPassive = false;
            bModeChoose = true;
            Debug.Log("Passive");

        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    // ************************ Start ****************************
    public void ActStart()
    {
        try
        {
            if (bTaskChoose && bModeChoose)
            {
                if (iTask == 1)
                {
                    SceneManager.LoadScene("ReachSceneYep");
                    bTaskChoose = false;
                    bModeChoose = false;
                }
                if (iTask == 2)
                {
                    SceneManager.LoadScene("NavigationScene");
                    bTaskChoose = false;
                    bModeChoose = false;
                }
                if (iTask == 3)
                {
                    SceneManager.LoadScene("ReachSceneYep");
                    bTaskChoose = false;
                    bModeChoose = false;
                }
                bButtonStart = true;
                hButtonStart = adsClientMain.CreateVariableHandle("MAIN.bStart");
                adsClientMain.WriteAny(hButtonStart, bButtonStart);
                adsClientMain.DeleteVariableHandle(hButtonStart);
                bButtonStart = false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    //********************** Light ***************************8
    public void ActLightOrigOn()
    {
        try
        {
            bLightOrigOn = true;
            adsClientMain.WriteAny(0x00004040, 0x00064F38, bLightOrigOn);
            bLightOrigOn = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void ActLightDestOn()
    {
        try
        {
            bLightDestOn = true;
            adsClientMain.WriteAny(0x00004040, 0x00064F39, bLightDestOn);
            bLightDestOn = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void InBoundaryX() // 机器人x方向
    {
        try
        {
            adsClientMain.WriteAny(0x00004040, 0x00064F1A, nInBoundaryX);
            nInBoundaryX = 0;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void InBoundaryY()   // 机器人y方向
    {
        try
        {
            adsClientMain.WriteAny(0x00004040, 0x00064F1C, nInBoundaryY);
            nInBoundaryY = 0;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void ActFinish()
    {
        try
        {
            adsClientMain.Dispose();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
}
