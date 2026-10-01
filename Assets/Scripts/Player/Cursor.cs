using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    //将鼠标位置从屏幕坐标转变为虚拟场景坐标
    //private Vector3 mouseWorldPos => Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    //原代码的物体实现均在UI画布上，坐标均为屏幕坐标，无需变换为虚拟场景坐标
    private Vector3 mouseWorldPos => Input.mousePosition;
    private bool canClick;
    // Update is called once per frame
    private void Update()
    {
        canClick = ObjectAtMousePosition();
        if (canClick && Input.GetMouseButtonDown(0))//检测鼠标是否按下
        {
            ClickAction(ObjectAtMousePosition().gameObject);
        }
    }
    /// <summary>
    /// 检测经场景坐标变换的鼠标所指向的碰撞体，并返回
    /// 碰撞体需要勾选trigger
    /// </summary>
    /// <returns></returns>
    private Collider2D ObjectAtMousePosition()
    {
        return Physics2D.OverlapPoint(mouseWorldPos);
    }
    /// <summary>
    /// 当鼠标指向trigger碰撞体并点击左键后，根据碰撞体标签的不同，执行对应代码
    /// 本程序仅用于控制Player碰撞体移动，理论上存在更简单的代码方式，但本代码结构更加通用
    /// </summary>
    /// <param name="clickObject"></param>
    private void ClickAction(GameObject clickObject)
    {
        switch (clickObject.tag)
        {
            case"Player":
                StartCoroutine(MoveWithMouse(clickObject));     
                break;
        }
    }
    /// <summary>
    /// 本协程用于Player碰撞体跟随鼠标移动，记得使用StartCoroutine启动
    /// </summary>
    /// <param name="clickObject"></param>
    /// <returns></returns>
    private IEnumerator MoveWithMouse(GameObject clickObject)
    {
        var offset = mouseWorldPos - clickObject.transform.position;
        while(Input.GetMouseButton(0))//检测鼠标是否按住
        {
            clickObject.transform.position = mouseWorldPos+ offset;
            yield return null;
        }
    }

}
