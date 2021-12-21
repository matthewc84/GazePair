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
    public GameObject targetPrefab;
    public GameObject HollowTargetPrefab;
    GameObject hollowTarget;
    public GameObject indicatorPrefab;
    GameObject indicator;
    private int randomX;
    private int randomY;
    private int randomZ;
    Vector3 randomPosition;
    Vector3 targetScale;
    // Start is called before the first frame update
    void Start()
    {
        timerIsRunning = true;
        if (NetworkManager.Singleton.IsHost)
        {
            randomX = UnityEngine.Random.Range(-4, 4);
            randomY = UnityEngine.Random.Range(-2, 4);
            randomZ = UnityEngine.Random.Range(1, 4);
            if (randomZ == 1)
            {
                targetScale = new Vector3(.04f, .04f, .04f);
            }
            if (randomZ == 2)
            {
                targetScale = new Vector3(.06f, .06f, .06f);
            }
            if (randomZ == 3)
            {
                targetScale = new Vector3(.1f, .1f, .1f);
            }
            if (randomZ == 4)
            {
                targetScale = new Vector3(.15f, .15f, .15f);
            }
            randomPosition = new Vector3((float)randomX/2, (float)randomY/2, (float)randomZ/2);
            hollowTarget = Instantiate(HollowTargetPrefab, randomPosition, Quaternion.identity);
            hollowTarget.transform.localScale = targetScale;
            hollowTarget.GetComponent<NetworkObject>().Spawn();
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
                    hollowTarget.GetComponent<NetworkObject>().Despawn(true);
                    GameObject target = Instantiate(targetPrefab, randomPosition, Quaternion.identity);
                    target.transform.localScale = targetScale;
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

        

        this.GetComponent<TextMeshPro>().SetText("Find the white target outline, countdown to target spawn: " + string.Format("{0:00}:{1:00}", minutes, seconds));
    }
}
