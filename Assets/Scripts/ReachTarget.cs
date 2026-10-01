using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class ReachTarget : MonoBehaviour
{
    private GameObject StartPointGameObject = null;
    private GameObject EndPointGameObject = null;
    private GameObject MovePointGameObject = null;
    private GameObject RobotPointGameObject = null;
    private GameObject CommunicationGameObject = null;  // 用于传输数据
    private GameObject CameraGameObject = null;
    private GameObject DisplayObject = null;
    private Transform RobotPointTransform = null;
    private Color StartPointInitialColor;   // 开始点原始颜色
    private Color MovePointInitialColor;    // 移动标志点的原始颜色
    private Color cRobotPointInitialColor;   // 机器人末端点的颜色
    private Color DisplayInitialColor;      // 显示部分的原始颜色

    private float MoveSpeed = 50f;  // 末端点移动速度
    private float MovePointSpeed = 40f;      // 移动标志点移动的速度
    private float RobotStartDistance = 0f;  // 机器人末端与开始位置距离
    private float RobotMoveDistance = 0f;   // 机器人末端与移动标志点距离
    private float MoveStartDistance = 0f;    // 移动标志点与开始位置距离
    private float MoveEndDistance = 0f;    // 移动标志点与末端位置距离

    private int nStartMove = 0;        // 开始移动指示，0 未移动 1 向终点运动 2 向起点运动
    private bool bMoveDone = false;     // 完成一次运动
    private bool bDrop = false;         // 脱离状态
    private int nDropTimes = 0;         // 脱离跟随运动次数

    private float StartTimer = 0f;          // 机器末端在开始位置停留的时间
    private float EndTimer = 0f;            // 机器末端在末端位置停留的时间
    private float TaskDoneTimer = 0f;       // 机器人任务结束时标志变色时间
    private float FollowTimer = 0f;         // 机器人末端跟随移动标志点时间
    private float TaskFinishTimer = 0f;     // 实际完成一次实验的时间
    private readonly float LastTime = 3f;   // 停留判断的时长
    private readonly float EndStopTime = 1f;   // 目标位置停留时长
    private readonly float TaskDoneMarkTime = 1f;   // 结束时标示变色的时长

    private float xPosition, yPosition; // 传入的机器人末端位置

    public AudioSource StartAudio;      // 开始运动音效
    public AudioSource FollowAudio;     // 跟随状态音效
    public AudioSource FinishAudio;     // 任务完成音效

    private static int TestNum = 18;    // 进行随机实验次数
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
        // 寻找object
        try
        {
            StartPointGameObject = GameObject.Find("/ReachCanvas/StartPoint");
            EndPointGameObject = GameObject.Find("/ReachCanvas/EndPoint");
            MovePointGameObject = GameObject.Find("/ReachCanvas/MovePoint");
            RobotPointGameObject = GameObject.Find("/ReachCanvas/RobotPoint");
            CameraGameObject = GameObject.Find("/Main Camera");
            DisplayObject = GameObject.Find("/ReachCanvas/TaskButton/DisplayTask");
            RobotPointTransform = RobotPointGameObject.GetComponent<Transform>();

            StartPointInitialColor = StartPointGameObject.GetComponent<Image>().color;
            MovePointInitialColor = MovePointGameObject.GetComponent<Image>().color;
            cRobotPointInitialColor = RobotPointGameObject.GetComponent<Image>().color;
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
            //**************** 机器人操控_键鼠 *************
            //// 移动机器人末端点
            if (Input.GetKey(KeyCode.UpArrow))
            {
                RobotPointTransform.Translate(Vector3.up * MoveSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                RobotPointTransform.Translate(Vector3.down * MoveSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                RobotPointTransform.Translate(Vector3.left * MoveSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                RobotPointTransform.Translate(Vector3.right * MoveSpeed * Time.deltaTime, Space.World);
            }

            // 机器人操控————同步显示机器人位置
            xPosition = RobotPointTransform.localPosition.x;
            yPosition = RobotPointTransform.localPosition.y;
            //**************** 机器人操控_下位机 *************
            //xPosition = CommunicationGameObject.GetComponent<ADS_active>().xPos;
            //yPosition = CommunicationGameObject.GetComponent<ADS_active>().yPos;
            //**************** 机器人操控_通用执行 *************
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
                    TestWriter = new System.IO.StreamWriter("D:/PPT/files/RobotTestData/ReachTarget_" + SubjectNumber + "_" + nWriteTimes + ".csv");
                    TestWriter.WriteLine("SubjectNumber, TestSequence, TestTask, Hour, Minute, Second, Millisecond, nStartMove, X_robotPoint, Y_robotPoint, RobotMoveDistance, FollowTimer, MoveStartDistance, nDropTimes, TaskFinishTimer");
                    bStartWrite = true;
                }
                else
                {
                    TestWriter.WriteLine(SubjectNumber + "," + TestSequence + "," + (TestList[TestSequence] % 6) + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute + "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + nStartMove + "," + xPosition + "," + yPosition + "," + RobotMoveDistance + "," + FollowTimer + "," + MoveStartDistance + "," + nDropTimes + "," + TaskFinishTimer);
                }

                // *** 阶段一
                if (TestList[TestSequence] % 6 == 0)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 40f;
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();
                }
                // *** 阶段二
                if (TestList[TestSequence] % 6 == 1)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 80f;
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段三
                if (TestList[TestSequence] % 6 == 2)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 40f;
                        cRobotPointInitialColor.a = 0f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段四
                if (TestList[TestSequence] % 6 == 3)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 40f;
                        cRobotPointInitialColor.a = 0f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                        CameraGameObject.GetComponent<Camera>().backgroundColor = Color.white;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                        CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    }
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段五
                if (TestList[TestSequence] % 6 == 4)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 40f;
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = Color.cyan;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段六
                if (TestList[TestSequence] % 6 == 5)
                {
                    if (nStartMove != 0 && !bMoveDone)        // 开始时没有变化
                    {
                        MovePointSpeed = 40f;
                        cRobotPointInitialColor.a = 0f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        cRobotPointInitialColor.a = 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = Color.cyan;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }

            }

            if (TestSequence >= TestNum)
            {
                bFormalTest = false;
                TestSequence = 0;
                MovePointSpeed = 40f;
                cRobotPointInitialColor.a = 255f;
                RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                DisplayObject.GetComponentInChildren<Text>().text = 0.ToString();

                // 对实验数组随机排序
                Shuffle(TestList);

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
            RobotMoveDistance = (RobotPointTransform.position - MovePointGameObject.transform.position).magnitude;
            MoveStartDistance = (MovePointGameObject.transform.position - StartPointGameObject.transform.position).magnitude;
            MoveEndDistance = (MovePointGameObject.transform.position - EndPointGameObject.transform.position).magnitude;

            // 点击运动开始计时
            //Debug.Log(CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing);
            if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
            {
                // ********************** 开始运动 ************************
                if (nStartMove == 0 && MoveStartDistance == 0f && RobotStartDistance < 10f)
                {
                    // 点击运动开始计时
                    if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
                    {
                        // 到达目标位置停留3s标志开始运动
                        StartTimer += Time.deltaTime;
                        if (StartTimer >= LastTime)
                        {
                            MovePointGameObject.GetComponent<Image>().color = Color.green;
                            StartTimer = 0f;
                            FollowTimer = 0f;   // 跟随时间初始化
                            bDrop = false;
                            nDropTimes = 0;     // 脱离跟随次数初始化
                            TaskFinishTimer = 0f;   // 实际完成任务总时间
                            bMoveDone = false;  // 此时开始完成任务
                            nStartMove = 1; // 向终点运动
                            StartAudio.Play();  // 播放开始音效
                                                //CommunicationGameObject.GetComponent<ADS_active>().ActLightOrigOn();
                        }
                    }
                }
                else if (nStartMove != 0)
                {
                    TaskFinishTimer += Time.deltaTime;
                    StartTimer = 0f;
                }
                else
                {
                    StartTimer = 0f;
                }

                // 开始运动，如果离开移动标志点
                if (nStartMove != 0 && RobotMoveDistance < 10f && MoveStartDistance > MovePointSpeed * Time.deltaTime)
                {
                    MovePointGameObject.GetComponent<Image>().color = Color.green;
                    if (!FollowAudio.isPlaying)
                    {
                        FollowAudio.Play(); // 播放跟随音效
                    }
                    FollowTimer += Time.deltaTime;  // 记录跟随时间
                    bDrop = false;
                }
                else if (nStartMove != 0 && RobotMoveDistance < 10f && MoveStartDistance <= MovePointSpeed * Time.deltaTime)
                {
                    MovePointGameObject.GetComponent<Image>().color = MovePointInitialColor;
                    FollowAudio.Stop(); // 离开则停止播放
                    bDrop = false;
                }
                else if (nStartMove != 0 && RobotMoveDistance >= 10f)
                {
                    MovePointGameObject.GetComponent<Image>().color = MovePointInitialColor;
                    FollowAudio.Stop(); // 离开则停止播放
                    if (!bDrop)
                    {
                        nDropTimes += 1;
                        bDrop = true;   // 已脱离
                    }
                }

                // **************** 到目标位置 ***********

                // 开启开始运动，到达目标位置时，标志到达运动完成
                if (nStartMove == 1 && MoveEndDistance < MovePointSpeed * Time.deltaTime)
                {
                    if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
                    {
                        MovePointGameObject.transform.position = EndPointGameObject.transform.position;
                        EndTimer += Time.deltaTime;
                        if (EndTimer >= EndStopTime)
                        {
                            nStartMove = 2; // 向起点运动
                            EndTimer = 0f;
                        }
                        //CommunicationGameObject.GetComponent<ADS_active>().ActLightDestOn();
                    }
                }
                else if (nStartMove == 1 && MoveEndDistance > 0f)
                {
                    // 向目标位置持续运动
                    MovePointGameObject.transform.Translate(Vector3.up * MovePointSpeed * Time.deltaTime, Space.World);
                }

                // ***************** 返回起点 ****************

                // 到达运动完成，且机器人末端远离初始和末端位置，则重新开始
                if (nStartMove == 2 && MoveStartDistance < MovePointSpeed * Time.deltaTime)
                {
                    if (!bMoveDone)
                    {
                        TaskDoneTimer += Time.deltaTime;
                        MovePointGameObject.transform.position = StartPointGameObject.transform.position;
                        MovePointGameObject.GetComponent<Image>().color = Color.yellow;
                        if (!FinishAudio.isPlaying)
                        {
                            FinishAudio.Play(); // 播放完成音效
                        }
                        if (TaskDoneTimer >= TaskDoneMarkTime)
                        {
                            MovePointGameObject.GetComponent<Image>().color = MovePointInitialColor;
                            TaskDoneTimer = 0f;
                            bMoveDone = true;   // 完成一次任务
                        }
                    }
                    else if (RobotStartDistance < 10f)
                    {
                        nStartMove = 0; // 向终点运动
                        nDropTimes = 0;
                        FollowTimer = 0f;
                        TaskFinishTimer = 0f;
                        // ******* 正式测试  ********
                        if (bFormalTest)
                        {
                            TestSequence += 1;
                        }
                    }
                }
                else if (nStartMove == 2 && MoveStartDistance > 0f)
                {
                    // 向开始位置持续运动
                    MovePointGameObject.transform.Translate(Vector3.down * MovePointSpeed * Time.deltaTime, Space.World);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
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
            nStartMove = 0;
            nDropTimes = 0;
            FollowTimer = 0f;
            TaskFinishTimer = 0f;
        }
    }
}
