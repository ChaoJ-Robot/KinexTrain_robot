using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ReachSceneYep : MonoBehaviour
{
    public Recording recording;
    public GameObject endPoint;
    private Dictionary<int, Vector3> keyPosPairs = new Dictionary<int, Vector3>();
    private bool canRanPos;
    private void Awake()
    {
        keyPosPairs.Add(1, new Vector3(0, 0, 0)); 
        keyPosPairs.Add(2, new Vector3((float)169.71, (float)-70.29, 0));
        keyPosPairs.Add(3, new Vector3(240,-240, 0));
        keyPosPairs.Add(4, new Vector3((float)169.71, (float)-409.71, 0)); 
        keyPosPairs.Add(5, new Vector3(0,-480, 0)); 
        keyPosPairs.Add(6, new Vector3((float)-169.71, (float)-409.71, 0)); 
        keyPosPairs.Add(7, new Vector3(-240,-240, 0)); 
        keyPosPairs.Add(8, new Vector3((float)-169.71, (float)-70.29, 0));
        canRanPos = true;//是否允许随机数生成的状态值，用于控制在单次recording内仅能生成一个随机位置；
    }
    // Update is called once per frame
    void Update()
    {
        if(recording.recordingState == true && canRanPos == true)
        {
            endPoint.transform.localPosition = keyPosPairs[Random.Range(1, 8)];//随机生成一个位置；
            recording.recordingNum++;//每随机生成一个位置，意味着开始了一个新的recording，试次数加一；
            Debug.Log(recording.recordingNum);
            canRanPos = false;
            endPoint.SetActive(true);
        }
        if (recording.recordingState != true)
        {
            endPoint.SetActive(false);
            canRanPos = true;
        }
    }
}
