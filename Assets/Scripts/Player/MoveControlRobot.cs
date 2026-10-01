using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MoveControlRobot : MonoBehaviour
{
    [SerializeField] private float xPosition, yPosition; // 传入的机器人末端位置
    private void FixedUpdate()
    {
        Move();
    }
    public void Move()
    {
        //通过ADS_active组件获取xy值；
        xPosition = GetComponent<ADS_active>().xPos;
        yPosition = GetComponent<ADS_active>().yPos;
        transform.localPosition = new Vector3(xPosition, yPosition, 0);
    }
}
