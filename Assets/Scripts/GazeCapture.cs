using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Text;

public class GazeCapture : MonoBehaviour
{
    public static GazeCapture Instance = null;
    Vector3 previousGazeValue;
    Vector3 tempGazeVector;
    double tempSecret;
    Vector3 GazeData;
    public int errorThreshold;
    public string sharedSecret;
    public float timeRemaining = 6;
    bool timerIsRunning = false;
    bool startGazeCapture = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        timerIsRunning = true;

    }


    // Update is called once per frame
    void Update()
    {

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                //We want the "heart" of the gaze collection, so we only start capture after a second, to allow user to find and track target
                if(timeRemaining < timeRemaining-1 && startGazeCapture)
                {
                    previousGazeValue = CoreServices.InputSystem.EyeGazeProvider.GazeDirection + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                    startGazeCapture = false;
                }
            }
            else
            {
                //We end capture before end of target hologram's lifespan, to ensure we don't mistrack
                GazeData = CoreServices.InputSystem.EyeGazeProvider.GazeDirection + CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                UpdateSharedSecret(previousGazeValue, GazeData);
                timerIsRunning = false;
            }
        }



    }

    public void UpdateSharedSecret(Vector3 previousGazeValue, Vector3 GazeData)
    {

        tempGazeVector = GazeData + previousGazeValue;
        tempSecret = Math.Atan2(tempGazeVector.y, tempGazeVector.x);
        if (tempSecret > 0)
        {
            sharedSecret = ((int)(((tempSecret * 180 / Math.PI) + errorThreshold - 1) / errorThreshold)).ToString();
        }
        else
        {
            sharedSecret = ((int)((((tempSecret + (2 * Math.PI)) * 180 / Math.PI) + errorThreshold - 1) / errorThreshold)).ToString();
        }

    }
}
