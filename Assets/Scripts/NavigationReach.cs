using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class NavigationReach : MonoBehaviour
{
    private GameObject StartPointGameObject = null;
    private GameObject EndPointGameObject = null;
    private GameObject RobotPointGameObject = null;
    private GameObject CameraGameObject = null;
    private GameObject DisplayObject = null;
    private GameObject CommunicationGameObject = null;  // 用于传输数据

    private Transform RobotPointTransform = null;
    private Color StartPointInitialColor;   // 开始点原始颜色
    private Color cRobotPointInitialColor;   // 机器人末端原始颜色
    private Color DisplayInitialColor;      // 显示部分的原始颜色
    private bool bTransparent;              // 是否隐身

    private readonly float MoveSpeed = 90f;  // 末端点移动速度
    private float RobotStartDistance = 0f;  // 机器人末端与开始位置距离
    private float RobotEndDistance = 0f;    // 机器人末端与末端位置距离

    private bool bStartMove = false;        // 开始移动指示
    private bool bMoveDone = false;         // 一次到达任务完成指示
    private int nCollisionTimes = 0;             // 碰撞次数

    private float StartTimer = 0f;          // 机器末端在开始位置停留的时间
    private float EndTimer = 0f;            // 机器末端在末端位置停留的时间
    private readonly float LastTime = 1.5f;   // 停留判断的时长
    private float TaskDoneTimer = 0f;       // 机器人任务结束时标志变色时间
    private float CollisionTimer = 0f;         // 机器人末端跟随移动标志点时间
    private float TaskFinishTimer = 0f;     // 实际完成一次实验的时间
    private readonly float TaskDoneMarkTime = 1f;   // 结束时标示变色的时长

    public AudioSource StartAudio;      // 开始运动音效
    public AudioSource CollisionAudio;     // 碰撞音效
    public AudioSource FinishAudio;     // 任务完成音效

    private static int TestNum = 15;    // 进行随机实验次数
    private int[] TestList = new int[TestNum];   // 记录随机实验顺序
    private int TestSequence = 0;       // 正在进行的实验次序
    private bool bFormalTest = false;        // 开始正式实验

    private static System.IO.StreamWriter TestWriter;   // 记录实验数据
    private int nWriteTimes = 0;    // 记录了多少次
    private bool bStartWrite = false;       // 开始记录数据
    private string SubjectNumber = "test";   // 受试者编号
    private string SubjectNumberKey;    //Key


    private float xPosition, yPosition; // 传入的机器人末端位置

    private void Awake()
    {
        // 寻找object
        try
        {
            RobotPointGameObject = GameObject.Find("/NavigationCanvas/RobotPoint");
            RobotPointTransform = RobotPointGameObject.GetComponent<Transform>();
            StartPointGameObject = GameObject.Find("/NavigationCanvas/StartPoint");
            EndPointGameObject = GameObject.Find("/NavigationCanvas/EndPoint");
            CameraGameObject = GameObject.Find("/Main Camera");
            DisplayObject = GameObject.Find("/NavigationCanvas/TaskButton/DisplayTask");

            StartPointInitialColor = StartPointGameObject.GetComponent<Image>().color;
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
    private void Update()
    {
        try
        {
            //**************** 机器人操控 *************
            // 移动机器人末端点
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
            // 同步显示机器人位置
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
                    TestWriter = new System.IO.StreamWriter("D:/PPT/files/RobotTestData/NavigationReach_" + SubjectNumber + "_" + nWriteTimes + ".csv");
                    TestWriter.WriteLine("SubjectNumber, TestSequence, TestTask, Hour, Minute, Second, Millisecond, bStartMove, X_robotPoint, Y_robotPoint, CollisionTimer, nCollisionTimes, TaskFinishTimer");
                    bStartWrite = true;
                }
                else
                {
                    TestWriter.WriteLine(SubjectNumber + "," + TestSequence + "," + (TestList[TestSequence] % 5) + "," + DateTime.Now.Hour + "," + DateTime.Now.Minute + "," + DateTime.Now.Second + "," + DateTime.Now.Millisecond + "," + bStartMove + "," + xPosition + "," + yPosition + "," + CollisionTimer + "," + nCollisionTimes + "," + TaskFinishTimer);
                }

                // *** 阶段一
                if (TestList[TestSequence] % 5 == 0)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bTransparent = false;
                    }
                    else
                    {
                        bTransparent = false;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();
                }
                // *** 阶段二
                if (TestList[TestSequence] % 5 == 1)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bTransparent = true;
                        cRobotPointInitialColor.a = 0f / 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                    }
                    else
                    {
                        bTransparent = false;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段三
                if (TestList[TestSequence] % 5 == 2)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bTransparent = true;
                        cRobotPointInitialColor.a = 0f / 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
                        CameraGameObject.GetComponent<Camera>().backgroundColor = Color.white;

                    }
                    else
                    {
                        bTransparent = false;
                    }
                    DisplayObject.GetComponent<Image>().color = DisplayInitialColor;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();

                }
                // *** 阶段四
                if (TestList[TestSequence] % 5 == 3)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bTransparent = false;
                    }
                    else
                    {
                        bTransparent = false;
                    }
                    CameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    DisplayObject.GetComponent<Image>().color = Color.cyan;
                    DisplayObject.GetComponentInChildren<Text>().text = (TestSequence + 1).ToString();
                }
                // *** 阶段五
                if (TestList[TestSequence] % 5 == 4)
                {
                    if (bStartMove && !bMoveDone)        // 开始时没有变化
                    {
                        bTransparent = true;
                        cRobotPointInitialColor.a = 0f / 255f;
                        RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;

                    }
                    else
                    {
                        bTransparent = false;
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
            RobotEndDistance = (RobotPointTransform.position - EndPointGameObject.transform.position).magnitude;
            // 点击运动开始计时
            if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
            {
                // ********************** 开始运动 ************************
                if (!bStartMove && RobotStartDistance < 10f)
                {
                    // 当点击开始运动后才可以运动，才会变绿
                    if (CommunicationGameObject.GetComponent<ADS_active>().bAllowMoing == true)
                    {
                        // 到达目标位置停留3s标志开始运动
                        StartTimer += Time.deltaTime;
                        if (StartTimer >= LastTime)
                        {
                            StartPointGameObject.GetComponent<Image>().color = Color.green;
                            bStartMove = true;

                            StartTimer = 0f;
                            CollisionTimer = 0f;   // 碰撞时间初始化
                            nCollisionTimes = 0;     // 碰撞次数初始化
                            TaskFinishTimer = 0f;   // 实际完成任务总时间
                            bMoveDone = false;  // 此时开始完成任务
                            StartAudio.Play();  // 播放开始音效
                        }
                    }
                }
                else
                {
                    StartTimer = 0f;
                }

                // 记录任务总时间
                if (bStartMove)
                {
                    TaskFinishTimer += Time.deltaTime;
                }

                // 开启开始运动，并在目标位置停留1.5s，标志到达运动完成
                if (bStartMove && RobotEndDistance < 10f)
                {
                    bMoveDone = true;   // 一次实验完成
                    bStartMove = false;
                    CollisionTimer = 0f;
                    nCollisionTimes = 0;
                    bTransparent = false;
                    cRobotPointInitialColor.a = 255f;
                    RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;

                    // ******* 正式测试  ********
                    if (bFormalTest)
                    {
                        TestSequence += 1;
                    }
                }
                if (bMoveDone)
                {
                    TaskDoneTimer += Time.deltaTime;
                    EndPointGameObject.GetComponent<Image>().color = Color.yellow;
                    if (!FinishAudio.isPlaying)
                    {
                        FinishAudio.Play(); // 播放完成音效
                    }
                    if (TaskDoneTimer >= TaskDoneMarkTime)
                    {
                        StartPointGameObject.GetComponent<Image>().color = StartPointInitialColor;
                        EndPointGameObject.GetComponent<Image>().color = Color.red;
                        TaskDoneTimer = 0f;
                        bMoveDone = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    // Collision碰撞，碰撞体collider is trigger都去掉
    // 碰撞开始
    void OnCollisionEnter2D(Collision2D collision)  // 可以同时触碰两处墙壁
    {
        if (!bTransparent)
        {
            RobotPointGameObject.GetComponent<Image>().color = Color.magenta;
        }
        if (!CollisionAudio.isPlaying)
        {
            CollisionAudio.Play(); // 播放跟随音效
        }
        if (bStartMove)
        {
            nCollisionTimes += 1;
        }
        //communication
        if (collision.GetContact(0).normal.y == -1)//从上方碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryX = 2;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryX();
            Debug.Log("1");
        }
        else if (collision.GetContact(0).normal.y == 1)//从下方碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryX = 1;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryX();
            Debug.Log("2");
        }

        if (collision.GetContact(0).normal.x == -1)//右边碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryY = 1;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryY();
            Debug.Log("3");

        }
        else if (collision.GetContact(0).normal.x == 1)//左边碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryY = 2;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryY();
            Debug.Log("4");

        }
    }

    // 碰撞持续中
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!bTransparent)
        {
            RobotPointGameObject.GetComponent<Image>().color = Color.magenta;
        }
        if (!CollisionAudio.isPlaying)
        {
            CollisionAudio.Play(); // 播放跟随音效
        }
        if (bStartMove)
        {
            CollisionTimer += Time.deltaTime;   // 记录碰撞时间
        }
        // communication
        if (collision.GetContact(0).normal.y == -1)//从上方碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryX = 2;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryX();
            Debug.Log("5");

        }
        else if (collision.GetContact(0).normal.y == 1)//从下方碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryX = 1;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryX();
            Debug.Log("6");

        }

        if (collision.GetContact(0).normal.x == -1)//右边碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryY = 1;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryY();
            Debug.Log("7");

        }
        else if (collision.GetContact(0).normal.x == 1)//左边碰撞
        {
            CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryY = 2;
            CommunicationGameObject.GetComponent<ADS_active>().InBoundaryY();
            Debug.Log("8");

        }

    }

    // 碰撞结束
    void OnCollisionExit2D(Collision2D collision)
    {
        if (!bTransparent)
        {
            RobotPointGameObject.GetComponent<Image>().color = cRobotPointInitialColor;
        }
        CollisionAudio.Stop();  //停止播放
        // communication
        CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryX = 0;
        CommunicationGameObject.GetComponent<ADS_active>().InBoundaryX();
        CommunicationGameObject.GetComponent<ADS_active>().nInBoundaryY = 0;
        CommunicationGameObject.GetComponent<ADS_active>().InBoundaryY();
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
            nCollisionTimes = 0;
            CollisionTimer = 0f;
            TaskFinishTimer = 0f;
        }
    }
}


