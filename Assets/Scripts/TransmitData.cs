using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwinCAT.Ads;
using UnityEngine.UI;
using System;

public class TransmitData : MonoBehaviour
{
    //============定义============
    private int hPersonalNumber, hNumOfTest; //personalNumber及NumOfTest的句柄
    private int iPersonalNumber = 0, iNumberOfTest = 0; //编号及实验次数
    private int hHMI_Control;   // 模式选择等汇总控制句柄
    private bool bHMI_Control = false;  // 模式选择等汇总控制
    private int hAxisAngle;  // Axis1,2,3角度句柄
    private float[] fAxisAngle = new float[3];  //Axis1,2,3的角度
    private int hRepeatTimes;   // 重复次数句柄
    private int iRepeatTimes;   // 重复次数
    private int hAxisArrayNew;  // 轴排列
    private bool bAxisArrayNew; // 确定轴排列

    private readonly string AMSNETID = "192.168.0.103.1.1";//目标的AMSnetid
    private readonly int PORTMAIN = 851;//目标的端口号
    private TcAdsClient adsClientMain = new TcAdsClient();//ads客户端

    private void Awake()
    {
        // 连接ADS客户端
        adsClientMain.Connect(AMSNETID, PORTMAIN);
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("11");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // 写入数据
    public void Transmit_PersonalNumber()
    {
        try
        {
            hPersonalNumber = adsClientMain.CreateVariableHandle("MAIN.Page1_PersonalNumber");
            iPersonalNumber = int.Parse(GameObject.Find("PersonalNumber").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hPersonalNumber, Convert.ToInt16(iPersonalNumber));
            Debug.Log("Transmit PersonalNumber!");
            adsClientMain.DeleteVariableHandle(hPersonalNumber);
            iPersonalNumber = 0;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_NumOfTest()
    {
        try
        {
            hNumOfTest = adsClientMain.CreateVariableHandle("MAIN.NumOfTest");
            iNumberOfTest = int.Parse(GameObject.Find("NumberOfTest").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hNumOfTest, Convert.ToInt16(iNumberOfTest));
            Debug.Log("Transmit Number of Test!");
            adsClientMain.DeleteVariableHandle(hNumOfTest);
            iNumberOfTest = 0;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_HMI_Control()
    {
        try
        {
            hHMI_Control = adsClientMain.CreateVariableHandle("MAIN.bHMI_Control");
            bHMI_Control = true;
            adsClientMain.WriteAny(hHMI_Control, bHMI_Control);
            Debug.Log("Transmit bHMI_Control!");
            adsClientMain.DeleteVariableHandle(hHMI_Control);
            bHMI_Control = false;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_Axis1Angle()
    {
        try
        {
            hAxisAngle = adsClientMain.CreateVariableHandle("MAIN.SVAxisAngleSerise[1]");
            fAxisAngle[0] = float.Parse(GameObject.Find("Axis1").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hAxisAngle, fAxisAngle[0]);
            Debug.Log("Transmit Axis1_Angle");
            adsClientMain.DeleteVariableHandle(hAxisAngle);
            fAxisAngle[0] = 0;
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_Axis2Angle()
    {
        try
        {
            hAxisAngle = adsClientMain.CreateVariableHandle("MAIN.SVAxisAngleSerise[2]");
            fAxisAngle[1] = float.Parse(GameObject.Find("Axis2").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hAxisAngle, fAxisAngle[1]);
            Debug.Log("Transmit Axis2_Angle");
            adsClientMain.DeleteVariableHandle(hAxisAngle);
            fAxisAngle[1] = 0;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_Axis3Angle()
    {
        try
        {
            hAxisAngle = adsClientMain.CreateVariableHandle("MAIN.SVAxisAngleSerise[3]");
            fAxisAngle[2] = float.Parse(GameObject.Find("Axis3").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hAxisAngle, fAxisAngle[2]);
            Debug.Log("Transmit Axis3_Angle");
            adsClientMain.DeleteVariableHandle(hAxisAngle);
            fAxisAngle[2] = 0;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_RepeatTime()
    {
        try
        {
            hRepeatTimes = adsClientMain.CreateVariableHandle("MAIN.SVAngleRepetNum");
            iRepeatTimes = int.Parse(GameObject.Find("RepeatTime").GetComponent<InputField>().text);
            adsClientMain.WriteAny(hRepeatTimes, Convert.ToInt16(iRepeatTimes));
            Debug.Log("Transmit RepeatTime");
            adsClientMain.DeleteVariableHandle(hRepeatTimes);
            iRepeatTimes = 0;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    public void Transmit_AxisArraySet()
    {
        try
        {
            hAxisArrayNew = adsClientMain.CreateVariableHandle("MAIN.SVAxisArrayNew");
            bAxisArrayNew = true;
            adsClientMain.WriteAny(hAxisArrayNew, bAxisArrayNew);
            Debug.Log("Transmit AxisArrayNew!");
            adsClientMain.DeleteVariableHandle(hAxisArrayNew);
            bAxisArrayNew = false;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
