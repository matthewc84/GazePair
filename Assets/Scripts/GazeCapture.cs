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
    Vector3 currentGazeValue;
    Vector3 previousGazeValue;
    Vector3 tempGazeVector;
    double tempDirectionSecret;
    double tempSharedSecret = 0d;
    int sampleCounter = 0;
    public int errorThreshold;
    public float sampleRate;
    public string sharedSecret;
    public float lengthOfCapture;
    float lengthOfCaptureTimer;
    bool timerIsRunning = false;
    bool initialGazeCapture = false;



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
        lengthOfCaptureTimer = lengthOfCapture-1;
        
    }


    // Update is called once per frame
    void Update()
    {

        if (timerIsRunning)
        {
            if (lengthOfCaptureTimer > 0)
            {
                lengthOfCaptureTimer -= Time.deltaTime;

                //We want the "heart" of the gaze collection, so we only start capture after a second, to allow user to find and track target and take an initial gaze direction reading
                if (lengthOfCaptureTimer < lengthOfCapture && !initialGazeCapture)
                {
                    previousGazeValue = CoreServices.InputSystem.EyeGazeProvider.GazeDirection - CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                    lengthOfCapture -= sampleRate;
                    initialGazeCapture = true;
                }

                //take a sample of the gaze direction roughly every second
                if (lengthOfCaptureTimer < lengthOfCapture)
                {
                    currentGazeValue = CoreServices.InputSystem.EyeGazeProvider.GazeDirection - CoreServices.InputSystem.EyeGazeProvider.GazeOrigin;
                    StartCoroutine(UpdateSharedSecret(previousGazeValue, currentGazeValue));
                    previousGazeValue = currentGazeValue;
                    lengthOfCapture -= sampleRate;
                }
            }
            else
            {
                //We end capture before end of target hologram's lifespan, to ensure we don't mistrack
                StartCoroutine(FinalizeSharedSecret(sampleCounter));
                timerIsRunning = false;

            }
        }

    }

    IEnumerator UpdateSharedSecret(Vector3 previousGazeValue, Vector3 currentGazeValue)
    {
        //Update a temporary variable with the sum of all the sampled gaze directions, to be averaged later
        tempGazeVector = (currentGazeValue - previousGazeValue);
        tempDirectionSecret = Math.Atan2(tempGazeVector.y, tempGazeVector.x);
        if (tempDirectionSecret > 0)
        {
            tempSharedSecret = tempSharedSecret + (tempDirectionSecret * 180 / Math.PI);
        }
        else
        {
            tempSharedSecret = tempSharedSecret + ((tempDirectionSecret + (2 * Math.PI)) * 180 / Math.PI);
        }

        sampleCounter+=1;

        yield return null;
    }

    IEnumerator FinalizeSharedSecret(int sampleCounter)
    {
        sharedSecret = ((int)(((tempSharedSecret / sampleCounter) + errorThreshold - 1) / errorThreshold)).ToString();

        yield return null;

    }

 
}
