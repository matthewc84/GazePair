using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.Text;

public class GazeLocationCaptureButtonClick : MonoBehaviour
{
    Vector3 testHit;
    bool initialPositionCapture = false;
    Vector3 initialGazeHitPosition;

    [Tooltip("Euler angles by which the object should be rotated by.")]
    [SerializeField]
    private Vector3 RotateByEulerAngles = Vector3.zero;

    [Tooltip("Rotation speed factor.")]
    [SerializeField]
    private float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator FinalizeSharedSecret(Vector3 initialGazeHitPosition)
    {
        //Updated Shared Secret with the binned x, y, and z values, then add the binned gaze direction and concatentate the binned, average gaze direction
        var sharedSecretComponent = GameObject.Find("SharedSecretCapture").GetComponent<SharedSecretCapture>();
        var gazeLocationCaptureGridComponent = GameObject.Find("GazeLocationCaptureGrid").GetComponent<GazeLocationCaptureGrid>();
        sharedSecretComponent.sharedSecret = sharedSecretComponent.sharedSecret + findBin(initialGazeHitPosition.x).ToString() + findBin(initialGazeHitPosition.y).ToString() + findBin(initialGazeHitPosition.z).ToString();
        gazeLocationCaptureGridComponent.clickCount += 1;
        yield return null;

    }


    public void onButtonClick()
    {
        this.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        this.transform.eulerAngles = transform.eulerAngles + RotateByEulerAngles * speed;
        testHit = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
        if (testHit != Vector3.zero && !initialPositionCapture)
        {
                initialGazeHitPosition = testHit;
                initialPositionCapture = true;
                StartCoroutine(FinalizeSharedSecret(initialGazeHitPosition));

        }
    }

    //very inelegant way to determine the bin for the hit position of the beginning of the gaze collection
    private int findBin(float input)
    {
        if (input >= -3.25f && input < -2.75f)
        {
            return -6;
        }
        if (input >= -2.75f && input < -2.25f)
        {
            return -5;
        }
        if (input >= -2.25f && input < -1.75f)
        {
            return -4;
        }
        if (input >= -1.75 && input < -1.25f)
        {
            return -3;
        }
        if (input >= -1.25 && input < -.75f)
        {
            return -2;
        }
        if (input >= -.75 && input < -.25f)
        {
            return -1;
        }
        if (input >= -.25 && input < .25f)
        {
            return 0;
        }
        if (input >= .25 && input < .75f)
        {
            return 1;
        }
        if (input >= .75 && input < 1.25f)
        {
            return 2;
        }
        if (input >= 1.25 && input < 1.75f)
        {
            return 3;
        }
        if (input >= 1.75 && input <= 2.25f)
        {
            return 4;
        }
        if (input >= 2.25 && input <= 2.75f)
        {
            return 5;
        }
        if (input >= 2.75 && input <= 3.25f)
        {
            return 6;
        }
        if (input >= 3.25 && input <= 3.75f)
        {
            return 7;
        }
        if (input >= 3.75 && input <= 4.25f)
        {
            return 8;
        }
        if (input >= 4.25 && input <= 4.75f)
        {
            return 9;
        }
        if (input >= 4.75 && input <= 5.25f)
        {
            return 10;
        }
        else
        {
            return -9;
        }
    }


}
