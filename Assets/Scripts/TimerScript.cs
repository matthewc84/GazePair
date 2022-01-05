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
    public GameObject SaltPrefab;
    GameObject SaltInstance;
    GameObject hollowTarget;
    GameObject gazeLocationCapture;
    public GameObject GazeLocationCapturePrefab;
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
            //Since The Random.Range only makes integers, and we want to randomize the start position of the target hologram by .5 meters, we take a random integer, and divide by 2 later to get the desired range.
            randomX = UnityEngine.Random.Range(-3, 3);
            randomY = UnityEngine.Random.Range(-2, 2);
            randomZ = UnityEngine.Random.Range(3, 5);
            //Based on the z value, scale the size of the target to conform with ARETT parameters to ensure accuracy of gaze and hit data
            if (randomZ == 2)
            {
                targetScale = new Vector3(.04f, .04f, .04f);
            }
            if (randomZ == 3)
            {
                targetScale = new Vector3(.05f, .05f, .05f);
            }
            if (randomZ == 4)
            {
                targetScale = new Vector3(.06f, .06f, .06f);
            }
            if (randomZ == 5)
            {
                targetScale = new Vector3(.1f, .1f, .1f);
            }
            //The randomized position with range -2 to 2 on X plane ( in .5m increments), -1 to 2 on Y plane ( in .5m increments), and 1 to 2.5 on Z plane (in .5m increments)
            randomPosition = new Vector3((float)randomX/2, (float)randomY/2, (float)randomZ/2);
            hollowTarget = Instantiate(HollowTargetPrefab, randomPosition, Quaternion.identity);
            SaltInstance = Instantiate(SaltPrefab);
            hollowTarget.GetComponent<NetworkObject>().Spawn();
            SaltInstance.GetComponent<NetworkObject>().Spawn();
        }

        gazeLocationCapture = Instantiate(GazeLocationCapturePrefab);
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
                    //If the time desired for the hollow orientation target is elapsed, detroy it and instantiate the real target at its location
                    hollowTarget.GetComponent<NetworkObject>().Despawn(true);
                    GameObject target = Instantiate(targetPrefab, randomPosition, Quaternion.identity);
                   // target.transform.localScale = targetScale;
                    target.GetComponent<NetworkObject>().Spawn();
                    target.GetComponent<PairTargetNetworkFunctionality>().Scale.Value = targetScale;
                    
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
