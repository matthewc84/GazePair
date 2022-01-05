using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Text;

public class GazeLocationCapture : MonoBehaviour
{
    public static GazeLocationCapture Instance = null;
    Vector3 testHit;
    bool initialPositionCapture = false;
    Vector3 initialGazeHitPosition;
    public string sharedSecret;

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
        initialGazeHitPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        testHit = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        if (testHit != Vector3.zero)
        {
            if (!initialPositionCapture && CoreServices.InputSystem.EyeGazeProvider.HitInfo.collider.name == "HollowTarget(Clone)")
            {
                initialGazeHitPosition = testHit;
                initialPositionCapture = true;
                StartCoroutine(FinalizeSharedSecret(initialGazeHitPosition));
            }
        }



    }

    IEnumerator FinalizeSharedSecret(Vector3 initialGazeHitPosition)
    {
        //Updated Shared Secret with the binned x, y, and z values, then add the binned gaze direction and concatentate the binned, average gaze direction
        sharedSecret = findBin(initialGazeHitPosition.x).ToString() + findBin(initialGazeHitPosition.y).ToString() + findBin(initialGazeHitPosition.z).ToString();

        yield return null;

    }

    //very inelegant way to determine the bin for the hit position of the beginning of the gaze collection
    private int findBin(float input)
    {
        if (input >= -2.25f && input < -1.75f)
        {
            return 1;
        }
        if (input >= -1.75 && input < -1.25f)
        {
            return 2;
        }
        if (input >= -1.25 && input < -.75f)
        {
            return 3;
        }
        if (input >= -.75 && input < -.25f)
        {
            return 4;
        }
        if (input >= -.25 && input < .25f)
        {
            return 5;
        }
        if (input >= .25 && input < .75f)
        {
            return 6;
        }
        if (input >= .75 && input < 1.25f)
        {
            return 7;
        }
        if (input >= 1.25 && input < 1.75f)
        {
            return 8;
        }
        if (input >= 1.75 && input <= 2.25f)
        {
            return 9;
        }
        if (input >= 2.25 && input <= 2.75f)
        {
            return 9;
        }
        else
        {
            return 0;
        }
    }
}
