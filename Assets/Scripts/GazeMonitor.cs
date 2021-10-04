using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GazeMonitor : NetworkBehaviour
{
    private Vector3 prevGazeDirectionVector; 
    private Vector3 localGazeDirectionChange;

    public NetworkVariableVector3 GazeDirectionChange = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    void Start()
    {
        var prevGazeDirectionVector = CoreServices.InputSystem?.EyeGazeProvider.GazeDirection;

    }

    [ServerRpc]
    void SubmitGazeDirectionRequestServerRpc(Vector3 gazeDirectionChange, ServerRpcParams rpcParams = default)
    {
            GazeDirectionChange.Value = gazeDirectionChange;

    }

    void Update()
    {
        var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;


        if (NetworkManager.Singleton.IsServer)
        {
            if (eyeGazeProvider != null)
            {
                GazeDirectionChange.Value = eyeGazeProvider.GazeDirection - prevGazeDirectionVector;

            }
        }
        else
        {

            SubmitGazeDirectionRequestServerRpc((eyeGazeProvider.GazeDirection - prevGazeDirectionVector));
        }

        prevGazeDirectionVector = eyeGazeProvider.GazeDirection;
    }

}
