using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recording : MonoBehaviour
{
    public bool recordingState;
    public int recordingNum;//试次数；
    private float recordingTime;//试次内计时；
    private void Awake()
    {
        recordingNum = 0;//试次数归零；
        recordingTime = 0;//试次内计时归零；
    }
    void Update()
    {
        if (recordingState == true)
        {
            recordingTime += Time.deltaTime;
            //Debug.Log(recordingTime); 
            //Debug.Log(transform.position);
        }
        else if(recordingState != true)
        {
            recordingTime = 0;
        }
    }

}
