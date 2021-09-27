using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class PairTargetNetworkFunctionality : NetworkBehaviour
{
    GameObject target;
    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Position.Value = this.transform.position;
            //Debug.Log(Position.Value);
        }

        if (NetworkManager.Singleton.IsClient)
        {
            this.transform.position = Position.Value;
            //Debug.Log(Position.Value);
        }

    }

}
