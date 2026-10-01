using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Show : MonoBehaviour
{
    public GameObject Player;
    public bool ShowMode;//true跟随，false反相
    public Vector3 TransCenter;//反相时，中心对称点的坐标
    private void FixedUpdate()
    {
        switch (ShowMode) 
        {
            case true:
                transform.position = Player.transform.position;
                break;
            case false:
                //Player系列模块都是基于ReachCanvas画布放置的，因此position（基于世界中心）和localPosition（基于父节点中心）有差异；
                //后续修整代码如果将父节点剔除，则可以顺便修正下方代码，以避免差异。
                transform.localPosition = new Vector3( 2 * TransCenter.x - Player.transform.localPosition.x, 2 * TransCenter.y  - Player.transform.localPosition.y, 0);
                break;
        }

        
    }
}
