using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Timers;
using TwinCAT.Ads;

public class ADS : MonoBehaviour
{
    private int hAxis1P;
    private int hAxis2P;
    private readonly int hAxis3P;
    private double Axis1ActPos;
    private double Axis2ActPos;
    private readonly double Axis3ActPos;
    private int hTraj, hRadius;//main.radius及三个轨迹的句柄
    private int iTraj;
    public int iTrajAds { private set; get; }
    public double radiusAds { private set; get; }
    private readonly int iTrajRec = 1;
    private readonly int iTrajTri = 2;
    private readonly int iTrajCir = 3;
    private readonly int hCommunication, hClose;//main.communication的句柄
    //private bool bCommunication = false, bClose = false;
    //private bool flag = false;
    private static StreamWriter writer;

    private readonly string AMSNETID = "192.168.0.103.1.1";//目标的AMSnetid
    private readonly int PORT = 501, PORTMAIN = 851;//目标的端口号
    private TcAdsClient adsClient = new TcAdsClient(), adsClientMain = new TcAdsClient();//ads客户端
    private readonly ArrayList notificationHandles = new ArrayList();//通知句柄
    private const double l1 = 292, l2 = 210;//机器人连杆
    private double theta1, theta2;
    private double x, y;//机器人末端平面坐标
    private double distance;

    private LineRenderer lineRenderer;
    private readonly Vector3[] pointPos = new Vector3[2];
    private double radius = 60;//圆角半径

    private readonly double shrinkScale = 1;
    void Awake()
    {
        //连接ADS客户端
        adsClient.Connect(AMSNETID, PORT);
        adsClientMain.Connect(AMSNETID, PORTMAIN);

        // Time.fixedDeltaTime = 0.002f;
    }
    void Start()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startColor = UnityEngine.Color.green;
        lineRenderer.endColor = UnityEngine.Color.green;
        lineRenderer.startWidth = 0.8f;
        lineRenderer.endWidth = 0.8f;
        lineRenderer.numCapVertices = 90;
        // lineRenderer.numCornerVertices = 5;//加了曲线末端就有凸起
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 2;

        //写文件
        StreamReader countReader = new StreamReader("D:/PPPPPPT/biology/files/ADS_Ex_Time/count.txt");
        string countStr = countReader.ReadLine();
        countReader.Close();
        int countNum = 0;
        if (!int.TryParse(countStr, out countNum))
        {
            countNum = 1;
        }
        writer = new StreamWriter("D:/PPPPPPT/biology/files/ADS_Ex_Time/C#_ADS_Time" + countNum + ".csv");
        writer.WriteLine("Minute, Second, Flag");
        countNum++;
        StreamWriter countWriter = new StreamWriter("D:/PPPPPPT/biology/files/ADS_Ex_Time/count.txt");
        countWriter.WriteLine(countNum);
        countWriter.Close();
    }
    void Update()
    {
        try
        {
            //以变量名方式
            //hAxis1P = adsClient.CreateVariableHandle("Axes.Axis 1.ActPos");//获取轴1的句柄
            //Axis1ActPos = Convert.ToDouble(adsClient.ReadAny(hAxis1P, typeof(Double)));
            //adsClient.DeleteVariableHandle(hAxis1P);

            //hAxis2P = adsClient.CreateVariableHandle("Axes.Axis 2.ActPos");
            //Axis2ActPos = Convert.ToDouble(adsClient.ReadAny(hAxis2P, typeof(Double)));
            //adsClient.DeleteVariableHandle(hAxis2P);
            //以地址方式
            Axis1ActPos = Convert.ToDouble(adsClient.ReadAny(0x4101, 0x10002, typeof(Double)));
            Axis2ActPos = Convert.ToDouble(adsClient.ReadAny(0x4102, 0x10002, typeof(Double)));
            //Debug.Log("Axis1ActPos: " + Axis1ActPos + "  Axis2ActPos: " + Axis2ActPos);


            //读取数据
            theta1 = Axis1ActPos * Math.PI / 180;
            theta2 = Axis2ActPos * Math.PI / 180;
            x = l1 * Math.Cos(theta1) + l2 * Math.Cos(theta1 + theta2);
            y = l1 * Math.Sin(theta1) + l2 * Math.Sin(theta1 + theta2);


            //hTraj = adsClientMain.CreateVariableHandle("MAIN.iTrajectory");
            //iTraj = Convert.ToInt32(adsClientMain.ReadAny(hTraj, typeof(int)));
            //iTrajAds = iTraj;
            //adsClientMain.DeleteVariableHandle(hTraj);

            iTraj = Convert.ToInt32(adsClientMain.ReadAny(0x4040, 0x62666, typeof(Int16)));
            iTrajAds = iTraj;
            // Debug.Log(iTraj);

            if (iTraj == iTrajCir)
            {
                drawPointCircle();
            }
            else if (iTraj == iTrajRec)
            {
                //hRadius = adsClientMain.CreateVariableHandle("MAIN.radius");
                //radius = Convert.ToDouble(adsClientMain.ReadAny(hRadius, typeof(Double)));
                //radiusAds = radius;
                //adsClientMain.DeleteVariableHandle(hRadius);
                radius = Convert.ToDouble(adsClientMain.ReadAny(0x00004040, 0x00062648, typeof(Double)));
                radiusAds = radius;
                drawPointRec();
            }
            else if (iTraj == iTrajTri)
            {
                radius = Convert.ToDouble(adsClientMain.ReadAny(0x00004040, 0x00062648, typeof(Double)));
                radiusAds = radius;
                drawPointTri();
            }
            //hCommunication = adsClientMain.CreateVariableHandle("MAIN.bCommunication");
            //bCommunication = Convert.ToBoolean(adsClientMain.ReadAny(hCommunication, typeof(Boolean)));
            //adsClientMain.DeleteVariableHandle(hCommunication);
            //if (bCommunication == true)
            //{
            //    flag = true;
            //    bCommunication = false;
            //}
            //flag = false;
            try
            {
                writer.WriteLine(DateTime.Now.Minute + "," + DateTime.Now.Second + "." + DateTime.Now.Millisecond.ToString().PadLeft(3, '0') + "," + DateTime.Now);
            }
            catch (Exception exWriter)
            {
                Debug.Log(exWriter.Message);
            }

            //hClose = adsClientMain.CreateVariableHandle("MAIN.bClose");
            //bClose = Convert.ToBoolean(adsClientMain.ReadAny(hClose, typeof(Boolean)));
            //adsClientMain.DeleteVariableHandle(hClose);
            //if (bClose == true)
            //{
            //    writer.Flush();
            //    writer.Close();
            //}
            // Debug.Log("x: " + x + " y: " + y);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            writer.Flush();
            writer.Close();
            adsClient.Dispose();
            adsClientMain.Dispose();
            return;
        }
    }
    private void drawPointCircle()
    {
        distance = Math.Sqrt(Math.Pow((x - 310), 2.0) + Math.Pow((y - 10), 2.0));
        x = (x - 310) * (1 + shrinkScale * (distance - 130) / 130) + 172;
        y = (y - 10) * (1 + shrinkScale * (distance - 130) / 130) - 36;
        //画点
        pointPos[0] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        pointPos[1] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        lineRenderer.SetPositions(pointPos);
    }
    private void drawPointRec()
    {
        if ((y - 10) <= (130 - radius) && (y - 10) >= (-130 + radius))
        {
            distance = Math.Abs(x - 310);
            x = (x - 310) * (1 + shrinkScale * (distance - 130) / 130) + 172;
            y = (y - 10) - 36;
        }
        else if ((x - 310) <= (130 - radius) && (x - 310) >= (-130 + radius))
        {
            distance = Math.Abs(y - 10);
            x = (x - 310) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - 130) / 130) - 36;
        }
        else
        {
            distance = Math.Sqrt(Math.Pow((Math.Abs(x - 310) - (130 - radius)), 2.0) + Math.Pow((Math.Abs(y - 10) - (130 - radius)), 2.0));
            x = (x - 310) * (1 + shrinkScale * (distance - radius) / 130) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius) / 130) - 36;//此时scale为distance-radius
        }
        pointPos[0] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        pointPos[1] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        lineRenderer.SetPositions(pointPos);
    }
    private void drawPointTri()
    {
        if ((y - 10) <= (130 - radius) && (y - 10) >= (-130 + radius) && (x - 310) >= (130 - radius / Math.Sqrt(3)))
        {
            distance = Math.Abs(x - 310);
            x = (x - 310) * (1 + shrinkScale * (distance - 130) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + 0.2) - 36;
        }
        else if ((y - 10) > (130 - radius) && (y - 10) >= (-Math.Sqrt(3) * ((x - 310) - (130 - radius / Math.Sqrt(3))) + 130 - radius))
        {
            distance = Math.Sqrt(Math.Pow(((x - 310) - (130 - radius / Math.Sqrt(3))), 2.0) + Math.Pow(((y - 10) - (130 - radius)), 2.0));
            x = (x - 310) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) - 36;//此时scale为distance-radius
        }
        else if ((y - 10) >= 0 && (y - 10) < (-Math.Sqrt(3) * ((x - 310) - (130 - radius / Math.Sqrt(3))) + 130 - radius) && (y - 10) >= (-Math.Sqrt(3) * ((x - 310) - (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)))))
        {
            distance = Math.Abs(Math.Sqrt(3) * (y - 10) - (x - 310) + 130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)) / 2;
            x = (x - 310) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) - 36;//此时scale为distance-radius
        }
        else if ((x - 310) <= (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)) && (y - 10) < (-Math.Sqrt(3) * ((x - 310) - (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)))) && (y - 10) >= (Math.Sqrt(3) * ((x - 310) - (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)))))
        {
            distance = Math.Sqrt(Math.Pow(((x - 310) - (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3))), 2.0) + Math.Pow(((y - 10) - 0), 2.0));
            x = (x - 310) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) - 36;//此时scale为distance-radius
        }
        else if ((y - 10) <= 0 && (y - 10) < (Math.Sqrt(3) * ((x - 310) - (130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)))) && (y - 10) >= (Math.Sqrt(3) * ((x - 310) - (130 - radius / Math.Sqrt(3))) - 130 + radius))
        {
            distance = Math.Abs(-Math.Sqrt(3) * (y - 10) - (x - 310) + 130 - 130 * Math.Sqrt(3) + 2 * radius / Math.Sqrt(3)) / 2;
            x = (x - 310) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) - 36;//此时scale为distance-radius
        }
        else if ((y - 10) < (-130 + radius) && (y - 10) < (Math.Sqrt(3) * ((x - 310) - (130 - radius / Math.Sqrt(3))) - 130 + radius))
        {
            distance = Math.Sqrt(Math.Pow(((x - 310) - (130 - radius / Math.Sqrt(3))), 2.0) + Math.Pow(((y - 10) - (-130 + radius)), 2.0));
            x = (x - 310) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) + 172;
            y = (y - 10) * (1 + shrinkScale * (distance - radius / Math.Sqrt(3)) / 130 + 0.2) - 36;//此时scale为distance-radius
        }
        pointPos[0] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        pointPos[1] = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), -20f);
        lineRenderer.SetPositions(pointPos);
    }

}
