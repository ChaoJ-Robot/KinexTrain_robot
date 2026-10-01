//本代码挂载在表示起止靶点的GameObject上，用于检测Player是否触碰以本GameObject为圆心的预设半径的圆形的虚拟碰撞体，Player需要勾选trigger
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    [Header("检测参数")]
    public float checkRadius;//碰撞体半径和目标区域半径的确定方式：两圆内切时圆心距离（即两园半径差），平均分配给碰撞体半径和目标区域半径即可；
    public LayerMask playerLayer;//碰撞检测层；
    [Header("状态")]
    [SerializeField] protected bool isTouched;//用于表征是否碰撞；
    protected Collider2D player;//通过碰撞检测获取Player，用于读取和修改Player上挂载的“记录”组件；
    protected bool recordingState;//Player上挂载的“记录”组件中的记录状态；
    
    private void Update()
    {
        Check();
    }
    protected virtual void Check()
    {
        //isTouched = Physics2D.OverlapCircle(transform.position, checkRadius, playerLayer);
        //以transform.position为中心，以checkRadius为半径，是否碰撞到了playerLayer层的碰撞体，返回值强制转换为布尔值。
        //Debug.Log(Physics2D.OverlapCircle(transform.position, checkRadius, playerLayer)?.GetComponent<CircleCollider2D>().radius);
        //该方法的返回值是Collider，可以以上述方式调用该Collider返回值。注意以？进行保护，避免报错。
        if ((player = Physics2D.OverlapCircle(transform.position, checkRadius, playerLayer)) != null)
        {
            isTouched = true;
            recordingState = player.GetComponent<Recording>().recordingState;
        }
        else
            isTouched = false;
    }

    private void OnDrawGizmosSelected()//显示碰撞区域的辅助线
    {
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }

}
