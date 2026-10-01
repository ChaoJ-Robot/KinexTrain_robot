using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class RememberReachTarget : MonoBehaviour
{
    private GameObject StartPointGameObject = null;
    private GameObject[] EndPointGameObject = new GameObject[RectNum];
    private GameObject[] DisplayGameObject = new GameObject[RemNum];
    private GameObject RobotPointGameObject = null;
    private GameObject DisplayObject = null;

    private GameObject CommunicationGameObject = null;  // 用于传输数据

    private Transform RobotPointTransform = null;

    private Color StartPointInitialColor;   // 开始点原始颜色
    private Color EndPointInitialColor;     // 目标点原始颜色
    private Color DisplayInitialColor;      // 显示部分的原始颜色

    private float RobotStartDistance = 0f;  // 机器人末端与开始位置距离
    private float[] RobotEndDistance = new float[RectNum];    // 机器人末端与末端位置距离
    private float Min_RobotEndDistance = 0f;    // 机器人末端与末端位置最小距离
    private int Index_MinRobotEndDistance = 0;  // 机器人末端与末端位置最小距离的下标

    private readonly float MoveSpeed = 90f;  // 末端点移动速度
    private static readonly int RemNum = 3;        // 要记忆的方块数量
    private static readonly int RectNum = 8;         // 方块总数
    private int nRemTimes = 0;         // 记忆正确个数
    private int[] RemList = new int[RemNum];// 记录要记忆的方块
    private int[] ChooseList = new int[] { -1, -1, -1 };// 已选择的方块
    private int ChooseTimes = 0;            // 选择记忆方块的次数
    private int[] DisplayList = new int[RectNum];   // 随机显示各种数字/颜色
    private bool bNumColor = false;             // false数字 true颜色
    private Color[] RemColor = new Color[] { Color.red, Color.blue, Color.cyan, Color.gray, Color.magenta, Color.white, Color.yellow, Color.green };  // 要记忆的颜色列表 

    private bool bInitialStart = false;     // 使机器人开始状态初始化一次
    private bool bStartRandom = false;      // 开始随机选择方块
    private bool bStartRem = false;         // 开始记忆
    private bool bStartMove = false;        // 开始移动指示
    private bool bMoveDone = false;         // 一次到达任务完成指示

    private float StartTimer = 0f;          // 机器末端在开始位置停留的时间
    private float RemTimer = 0f;            // 记忆方块的时间
    private float EndTimer = 0f;            // 机器末端在末端位置停留的时间
    private float RestartTimer = 0f;        // 重新开始的时间
    private float TaskDoneTimer = 0f;       // 机器人任务结束时标志变色时间
    private float TaskFinishTimer = 0f;     // 实际完成一次实验的时间
    private readonly float EndStopTime = 1f;   // 目标位置停留时长
    private readonly float LastTime = 5f;   // 停留判断的时长
    private readonly float TaskDoneMarkTime = 1f;   // 结束时标示变色的时长

    private float xPosition, yPosition; // 传入的机器人末端位置

    public AudioSource StartAudio;      // 开始运动音效
    public AudioSource RightAudio;     // 判断正确音效
    public AudioSource FalseAudio;     // 判断错误音效
    public AudioSource FinishAudio;     // 任务完成音效

    private static int TestNum = 10;    // 进行随机实验次数
    private int[] TestList = new int[TestNum];   // 记录随机实验顺序
    private int TestSequence = 0;       // 正在进行的实验次序
    private bool bFormalTest = false;        // 开始正式实验

    private static System.IO.StreamWriter TestWriter;   // 记录实验数据
    private int nWriteTimes = 0;    // 记录了多少次
    private bool bStartWrite = false;       // 开始记录数据
    private string SubjectNumber = "test";   // 受试者编号
    private string SubjectNumberKey;    //Key


    private void Awake()
    {
        try
        {
            RobotPointGameObject = GameObject.Find("/RememberCanvas/RobotPoint");
            RobotPointTransform = RobotPointGameObject.GetComponent<Transform>();

            StartPointGameObject = GameObject.Find("/RememberCanvas/StartPoint");
            EndPointGameObject[0] = GameObject.Find("/RememberCanvas/EndPoint1");
            EndPointGameObject[1] = GameObject.Find("/RememberCanvas/EndPoint2");
            EndPointGameObject[2] = GameObject.Find("/RememberCanvas/EndPoint3");
            EndPointGameObject[3] = GameObject.Find("/RememberCanvas/EndPoint4");
            EndPointGameObject[4] = GameObject.Find("/RememberCanvas/EndPoint5");
            EndPointGameObject[5] = GameObject.Find("/RememberCanvas/EndPoint6");
            EndPointGameObject[6] = GameObject.Find("/RememberCanvas/EndPoint7");
            EndPointGameObject[7] = GameObject.Find("/RememberCanvas/EndPoint8");

            DisplayGameObject[0] = GameObject.Find("/RememberCanvas/DisplayButton1");
            DisplayGameObject[1] = GameObject.Find("/RememberCanvas/DisplayButton2");
            DisplayGameObject[2] = GameObject.Find("/RememberCanvas/DisplayButton3");

            DisplayObject = GameObject.Find("/RememberCanvas/TaskButton/DisplayTask");

            StartPointInitialColor = StartPointGameObject.GetComponent<Image>().color;
            EndPointInitialColor = EndPointGameObject[0].GetComponent<Image>().color;
            DisplayInitialColor = DisplayObject.GetComponent<Image>().color;

            CommunicationGameObject = GameObject.Find("/GameObject");

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // ******************** 伪随机实验 ********************
        try
        {
            for (int i = 0; i < TestNum; i++)
            {
                TestList[i] = i;
            }
            // 对实验数组随机排序
            Shuffle(TestList);

            for (int i = 0; i < RectNum; i++)
            {
                DisplayList[i] = i;
            }
            // 对颜色数字随机排序
            Shuffle(DisplayList);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

        // 传入填写的受试者编号
        SubjectNumber = PlayerPrefs.GetString(SubjectNumberKey);

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            //**************** 机器人操控 *************
            //// 移动机器人末端点
            //if (Input.GetKey(KeyCode.UpArrow))
            //{
            //    RobotPointTransform.Translate(Vector3.up * MoveSpeed * Time.deltaTime, Space.World);
            //}
            //if (Input.GetKey(KeyCode.DownArrow))
            //{
            //    RobotPointTransform.Translate(Vector3.down * MoveSpeed * Time.deltaTime, Space.World);
            //}
            //if (Input.GetKey(KeyCode.LeftArrow))
            //{
            //    RobotPointTransform.Translate(Vector3.left * MoveSpeed * Time.deltaTime, Space.World);
            //}
            //if (Input.GetKey(KeyCode.RightArrow))
            //{
            //    RobotPointTransform.Translate(Vector3.right * MoveSpeed * Time.deltaTime, Space.World);
            //}

            //// 同步显示机器人位置
            //xPosition = RobotPointTransform.localPosition.x;
            //yPosition = RobotPointTransform.localPosition.y;
            xPosition = CommunicationGameObject.GetComponent<ADS_active>().xPos;
            yPosition = CommunicationGameObject.GetComponent<ADS_active>().yPos;
            RobotPointTransform.localPosition = new Vector3(xPosition, yPosition, 0);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

        // ******************** 进行伪随机实验，添加控制条件 ********************
        try
        {
            if (bFormalTest && TestSequence < TestNum)
            {
                // *** 写文件
                if (!bStartWrite)
                {
                    TestWriter = new System.IO.StreamWriter("D:/PPT/files/RobotTestData/RememberReach_" + SubjectNumber + "_" + nWriteTimes + ".csv");
                    TestWriter.WriteLine("SubjectNumber, TestSequence, TestTask, Hour, Minute, Second, Millisecond, bInitialStart, bStartMove, X_robotPoint, Y_robotPoint, nRemTimes, TaskFinishTimer, RobotStartDistance");
                    bStartWrite = true;
                }
                else
                {
                    TestWriter.WriteLine(SubjectNumber + "," + TestSequence + "," + (TestList[TestSequence] % 2) + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute + "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + bInitialStart + "," + bStartMove + "," + xPosition + "," + yPosition + "," + nRemTimes + "," + TaskFinishTimer + "," + RobotStartDistance);
                }

                // *** 阶段一
                if (TestList[TestSequence] % 2 == 0)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bNumColor = true;
                    }
                    else
                    {
                        bNumColor = true;
                    }
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();
                }
                // *** 阶段二
                if (TestList[TestSequence] % 2 == 1)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bNumColor = false;
                    }
                    else
                    {
                        bNumColor = false;
                    }
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
            }

            if (TestSequence >= TestNum)
            {
                bFormalTest = false;
                TestSequence = 0;
                DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                DisplayObject.GetComponentInChildren<Text>().text = 0.ToString();

                // 对实验数组随机排序
                Shuffle(TestList);
                Shuffle(DisplayList);   // 对颜色数字随机排序

                TestWriter.Flush();
                TestWriter.Close();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }

        // ************************* 图像交互 *****************************
        try
        {
            // 计算机器人末端与开始位置、目标位置的距离
            RobotStartDistance = (RobotPointTransform.position - StartPointGameObject.transform.position).magnitude;
            // 点击运动开始计时
            if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
            {
                if (RobotStartDistance < 10f && !bInitialStart)
                {
                    if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
                    {
                        //到达目标位置停留3s标志开始运动
                        StartTimer += Time.deltaTime;
                        if (StartTimer >= 3f)
                        {
                            StartPointGameObject.GetComponent<Image>().color = Color.gray;
                            bStartRandom = true;
                            StartTimer = 0f;
                            nRemTimes = 0;
                            TaskFinishTimer = 0f;
                            bInitialStart = true;
                        }
                    }
                }
                else // 防止不确定的接触
                {
                    StartTimer = 0f;
                }

                if (bStartRandom)
                {
                    //产生随机数，选择要记忆的方块
                    RemList = GenerateRandom(0, RectNum, RemNum);
                    string s = RemList[0].ToString() + ' ' + RemList[1].ToString() + ' ' + RemList[2].ToString();
                    //Debug.Log(s);
                    bStartRandom = false;
                    bStartRem = true;
                }
                //短暂显示要记忆的方块
                if (bStartRem)
                {
                    if (bNumColor)  // 记忆颜色
                    {
                        for (int i = 0; i < RectNum; i++)
                        {
                            EndPointGameObject[i].GetComponent<Image>().color = RemColor[DisplayList[i]];
                        }
                    }
                    else // 记忆数字
                    {
                        for (int i = 0; i < RectNum; i++)
                        {
                            EndPointGameObject[i].GetComponentInChildren<Text>().text = DisplayList[i].ToString();
                        }
                    }
                    RemTimer += Time.deltaTime;
                    if (RemTimer >= LastTime)
                    {
                        for (int i = 0; i < RectNum; i++)
                        {
                            EndPointGameObject[i].GetComponent<Image>().color = EndPointInitialColor;
                            EndPointGameObject[i].GetComponentInChildren<Text>().text = null;
                        }
                        StartPointGameObject.GetComponent<Image>().color = Color.green;
                        RemTimer = 0f;
                        bStartRem = false;
                        bStartMove = true;
                        StartAudio.Play();  // 播放开始音效
                    }
                }
                if (bStartMove)
                {
                    for (int i = 0; i < RectNum; i++)
                    {
                        RobotEndDistance[i] = (RobotPointTransform.position - EndPointGameObject[i].transform.position).magnitude;
                        if (i == 0)
                        {
                            Min_RobotEndDistance = RobotEndDistance[i];
                            Index_MinRobotEndDistance = i;
                        }
                        else if (RobotEndDistance[i] < Min_RobotEndDistance)
                        {
                            Min_RobotEndDistance = RobotEndDistance[i];
                            Index_MinRobotEndDistance = i;
                        }
                    }

                    if (bNumColor)  // 记忆颜色
                    {
                        for (int i = 0; i < RemNum; i++)
                        {
                            DisplayGameObject[i].GetComponent<Image>().color = RemColor[DisplayList[RemList[i]]];
                        }
                    }
                    else // 记忆数字
                    {
                        for (int i = 0; i < RemNum; i++)
                        {
                            DisplayGameObject[i].GetComponentInChildren<Text>().text = DisplayList[RemList[i]].ToString();
                        }
                    }
                    TaskFinishTimer += Time.deltaTime;  // 总时间开始计时
                }
                else
                {
                    for (int i = 0; i < RemNum; i++)
                    {
                        DisplayGameObject[i].GetComponent<Image>().color = EndPointInitialColor;
                        DisplayGameObject[i].GetComponentInChildren<Text>().text = null;
                    }
                }
                //开启开始运动，并在目标位置停留3s，标志到达运动完成
                if (bStartMove && Min_RobotEndDistance < 10f)
                {
                    EndTimer += Time.deltaTime;
                    if (EndTimer >= EndStopTime)
                    {
                        if (Array.IndexOf(ChooseList, Index_MinRobotEndDistance) == -1) // 如果index不存在于chooselist
                        {
                            if (Array.IndexOf(RemList, Index_MinRobotEndDistance) == -1) // 如果index不存在于remlist
                            {
                                EndPointGameObject[Index_MinRobotEndDistance].GetComponent<Image>().color = Color.red;
                                EndTimer = 0f;
                                ChooseList[ChooseTimes] = Index_MinRobotEndDistance;
                                ChooseTimes += 1;
                                FalseAudio.Play();
                            }
                            else
                            {
                                EndPointGameObject[Index_MinRobotEndDistance].GetComponent<Image>().color = Color.green;
                                EndTimer = 0f;
                                ChooseList[ChooseTimes] = Index_MinRobotEndDistance;
                                ChooseTimes += 1;
                                RightAudio.Play();
                                nRemTimes += 1; // 记忆正确加一
                            }
                        }
                    }
                }
                else
                {
                    EndTimer = 0f;
                }
                // 记忆到达运动完成，且机器人末端远离初始和末端位置，则重新开始
                if (ChooseTimes >= RemNum)
                {
                    bStartMove = false;
                    TaskFinishTimer = 0f;
                    RestartTimer += Time.deltaTime;
                    StartPointGameObject.GetComponent<Image>().color = Color.yellow;
                    if (RestartTimer >= EndStopTime)
                    {
                        StartPointGameObject.GetComponent<Image>().color = StartPointInitialColor;
                        for (int i = 0; i < RectNum; i++)
                        {
                            EndPointGameObject[i].GetComponent<Image>().color = EndPointInitialColor;
                        }
                        RestartTimer = 0f;
                        ChooseTimes = 0;
                        bMoveDone = true;
                    }
                }
                if (bMoveDone)
                {
                    bInitialStart = false;
                    bMoveDone = false;
                    for (int i = 0; i < RemNum; i++)
                    {
                        ChooseList[i] = -1;
                    }
                    Shuffle(DisplayList);   // 对颜色数字随机排序
                    // ******* 正式测试  ********
                    if (bFormalTest)
                    {
                        TestSequence += 1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    // 产生不重复随机数的种子
    private static int GenerateRandomSeed()
    {
        return Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
    }
    // 产生随机数 randNum 为产生随机数的数目
    private int[] GenerateRandom(int minValue, int maxValue, int randNum)
    {
        System.Random ran = new System.Random(GenerateRandomSeed());
        int[] arr = new int[randNum];
        bool flag = true;
        // 用 do...while 循环来生成不重复的随机数
        for (int i = 0; i < randNum; i++)
        {
            do
            {
                int val = ran.Next(minValue, maxValue);
                if (!IsDuplicates(ref arr, val))
                {
                    arr[i] = val;
                    flag = false;
                }
            } while (flag);
            if (!flag)
                flag = true;
        }
        return arr;
    }
    // 查检当前生成的随机数是否重复
    private bool IsDuplicates(ref int[] arr, int currRandNum)
    {
        bool flag = false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == currRandNum)
            {
                flag = true;
                break;
            }
        }
        return flag;
    }
    // 对数组进行随机排序
    private void Shuffle(int[] intArray)
    {
        for (int i = 0; i < intArray.Length; i++)
        {
            int temp = intArray[i];
            int randomIndex = UnityEngine.Random.Range(0, intArray.Length);
            intArray[i] = intArray[randomIndex];
            intArray[randomIndex] = temp;
        }
    }

    // 点击开始正式实验
    public void FormalTest()
    {
        if (!bFormalTest)
        {
            bFormalTest = true;
            bStartMove = false;
            RestartTimer = 0f;
            ChooseTimes = 0;
            nRemTimes = 0;
            TaskFinishTimer = 0f;
            bInitialStart = false;
            bMoveDone = false;
            for (int i = 0; i < RemNum; i++)
            {
                ChooseList[i] = -1;
            }
            Shuffle(DisplayList);   // 对颜色数字随机排序
        }
    }
}



