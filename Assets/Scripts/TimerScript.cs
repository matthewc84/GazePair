using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;

public class TimerScript : MonoBehaviour
{

    public float timeRemaining = 10;
    bool timerIsRunning = false;
    public GameObject timer;
    public GameObject targetPrefab;
    public GameObject LoggerPrefab;
    public GameObject HollowTargetPrefab;
    GameObject hollowTarget;
    public GameObject indicatorPrefab;
    GameObject indicator;

    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = true;
        if (NetworkManager.Singleton.IsHost)
        {
            hollowTarget = Instantiate(HollowTargetPrefab);
            hollowTarget.GetComponent<NetworkObject>().Spawn();
            indicator = Instantiate(indicatorPrefab);
            indicator.GetComponent<NetworkObject>().Spawn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    var targetObj = hollowTarget.GetComponent<NetworkObject>();
                    targetObj.Despawn(true);
                    var indicatorObj = indicator.GetComponent<NetworkObject>();
                    indicatorObj.Despawn(true);
                    GameObject target = Instantiate(targetPrefab);
                    target.GetComponent<NetworkObject>().Spawn();
                    
                    timeRemaining = 0;
                    timerIsRunning = false;
                    var obj = this.GetComponent<NetworkObject>();
                    obj.Despawn(true);
                }

            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        

        timer.GetComponent<TextMeshPro>().SetText("Countdown to target spawn: " + string.Format("{0:00}:{1:00}", minutes, seconds));
    }
}
