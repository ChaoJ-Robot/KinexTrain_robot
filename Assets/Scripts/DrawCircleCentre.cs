using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TwinCAT.Ads;

public class DrawCircleCentre : MonoBehaviour
{
    //private int hTraj, hRadius;//main.radius及三个轨迹的句柄
    private int iTraj;
    private readonly int iTrajRec = 1;
    private readonly int iTrajTri = 2;
    private readonly int iTrajCir = 3;
    //private readonly string AMSNETID = "192.168.0.103.1.1";//目标的AMSnetid
    //private readonly int PORT = 851;//目标的端口号
    private TcAdsClient adsClientMain = new TcAdsClient();
    private GameObject ads;

    private LineRenderer lineRenderer;
    public Color startColor, endColor;
    private Vector3 circlePos;
    private readonly float deltatheta = 0.5f;
    private int segment;
    public float radiusCircle = 130f;   //圆的半径
    private float radius = 60;//圆角半径
    private readonly float shrinkScale = 2f;
    // Start is called before the first frame update
    void Awake()
    {
        //连接ADS客户端
        //adsClientMain.Connect(AMSNETID, PORT);
        try
        {
            ads = GameObject.Find("/Canvas/Image/point");
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    void Start()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.numCapVertices = 5;
        // lineRenderer.numCornerVertices = 5;//加了曲线末端就有凸起
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
    }
    // Update is called once per frame
    void Update()
    {
        try
        {
            //hTraj = adsClientMain.CreateVariableHandle("MAIN.iTrajectory");
            //iTraj = Convert.ToInt32(adsClientMain.ReadAny(hTraj, typeof(int)));
            //adsClientMain.DeleteVariableHandle(hTraj);
            iTraj = ads.GetComponent<ADS>().iTrajAds;
            if (iTraj == iTrajCir)
            {
                drawCircle_Center();
            }
            else if (iTraj == iTrajRec)
            {
                //hRadius = adsClientMain.CreateVariableHandle("MAIN.radius");
                //radius = Convert.ToDouble(adsClientMain.ReadAny(hRadius, typeof(Double)));
                //adsClientMain.DeleteVariableHandle(hRadius);
                radius = Convert.ToSingle(ads.GetComponent<ADS>().radiusAds);
                drawRectangle();
            }
            else if (iTraj == iTrajTri)
            {
                //hRadius = adsClientMain.CreateVariableHandle("MAIN.radius");
                //radius = Convert.ToDouble(adsClientMain.ReadAny(hRadius, typeof(Double)));
                //adsClientMain.DeleteVariableHandle(hRadius);
                radius = Convert.ToSingle(ads.GetComponent<ADS>().radiusAds);
                drawTriangle();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            adsClientMain.Dispose();
        }
    }
    void drawCircle_Center()
    {
        float x, y, z = -20f;
        float angle = 0;
        segment = Convert.ToInt32(360 / deltatheta);
        lineRenderer.positionCount = segment;
        for (int index = 0; index < segment; index++)
        {
            x = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusCircle * (1 + shrinkScale * (radiusCircle - 130f) / 130f) + 172f;
            y = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusCircle * (1 + shrinkScale * (radiusCircle - 130f) / 130f) - 36f;
            circlePos = new Vector3(x, y, z);
            lineRenderer.SetPosition(index, circlePos);
            angle += deltatheta;
        }
    }
    void drawRectangle()
    {
        float x, y, z = -20f;
        float angle = 0;
        segment = Convert.ToInt32(360 / deltatheta) + 4;
        lineRenderer.positionCount = segment;
        for (int index = 0; index < segment; index++)
        {
            if (index >= 0 && index < (segment / 4))
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) + 130 - radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) + 130 - radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) - 36f;
            }
            else if (index >= (segment / 4) && index < (segment / 2))
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) - 130 + radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) + 130 - radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) - 36f;
            }
            else if (index >= (segment / 2) && index < (segment * 3 / 4))
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) - 130 + radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) - 130 + radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) - 36f;
            }
            else
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) + 130 - radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius + radiusCircle - 130) - 130 + radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f) - 36f;
            }
            circlePos = new Vector3(x, y, z);
            lineRenderer.SetPosition(index, circlePos);
            if (index == (segment / 4 - 1) || index == (segment / 2 - 1) || index == (segment * 3 / 4 - 1))//圆角矩形处在直边上，不加angle
            {
            }
            else
            {
                angle += deltatheta;
            }
        }
    }
    void drawTriangle()
    {
        float x, y, z = -20f;
        float angle = 0;
        segment = Convert.ToInt32(360 / deltatheta) + 3;
        lineRenderer.positionCount = segment;
        for (int index = 0; index < segment; index++)
        {
            if (index >= 0 && index < (segment / 3))
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130) + 130 - radius / Mathf.Sqrt(3)) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130) + 130 - radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) - 36f;//三角形放大程度多0.2
            }
            else if (index >= (segment / 3) && index < (segment * 2 / 3))
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130) + 130 - 130 * Mathf.Sqrt(3) + radius * 2 / Mathf.Sqrt(3)) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130)) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) - 36f;
            }
            else
            {
                x = (Mathf.Cos(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130) + 130 - radius / Mathf.Sqrt(3)) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) + 172f;
                y = (Mathf.Sin(Mathf.Deg2Rad * angle) * (radius / Mathf.Sqrt(3) + radiusCircle - 130) - 130 + radius) * (1 + shrinkScale * (radiusCircle - 130f) / 130f + 0.2f) - 36f;
            }
            circlePos = new Vector3(x, y, z);
            lineRenderer.SetPosition(index, circlePos);
            if (index == (segment / 3 - 1) || index == (segment * 2 / 3 - 1))//圆角矩形处在直边上，不加angle
            {
            }
            else
            {
                angle += deltatheta;
            }
        }
    }
}



