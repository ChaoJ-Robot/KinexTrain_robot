using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheckStart : PhysicsCheck
{
    private readonly float waitTime = 3f;
    [SerializeField] private float waitTimeCounter;
    [Header("音效")] public AudioSource startAudio;
    
    private void Awake()
    {
        waitTimeCounter = waitTime;
    }
    protected override void Check()
    {
        base.Check();
        if (recordingState != true)//判定试次是否已经启动，不允许重复启动；
        {
            Timecounter();
        }
    }
    /// <summary>
    /// 根据isTouched的布尔值，进行累计计时判定，用于计时启动
    /// </summary>
    public void Timecounter()
    {
        if (isTouched && waitTimeCounter > 0) //&& waitTimeCounter>0用于保护倒计时参数，避免无限减
        {
            waitTimeCounter -= Time.deltaTime;
            if (waitTimeCounter <= 0)
            {
                waitTimeCounter = waitTime; //用于本试次结束后的新的一轮启动计时；
                Debug.Log("启动");
                player.GetComponent<Recording>().recordingState = true;
                startAudio.Play();
            }
        }
        else if (!isTouched)//这部分代码用于清除累计计时，如果是强制及时需要剔除
        {
            waitTimeCounter = waitTime;//未累计够时间就脱离，需要重新计时；
        }
    }
}
