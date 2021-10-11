using System.Collections.Generic;
using System.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using Object = UnityEngine.Object;
using TMPro;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

public class IntroHud : MonoBehaviour
{

    private SceneSwitchProgress m_SceneProgress;

    void Awake()
    {

    }

    void Update()
    {

    }

    public void nextScene()
    {

        SceneManager.LoadSceneAsync("Connection Scene");

    }


}
