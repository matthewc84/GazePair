using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class GazePairCandidate : NetworkBehaviour
{

    Camera mainCamera;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableVector3 GazeDirectionChange = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetPositionOnPlane();
    }

    static Vector3 GetPositionOnPlane()
    {
        return GameObject.Find("Main Camera").transform.position;
    }

    void Update()
    {
        /* transform.position = Position.Value;
         if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
     out var networkedClient))
         {
             var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
             if (player)
             {
                 player.Move();
             }
         }*/

        if (NetworkManager.Singleton.IsServer)
        {
           Position.Value = GetPositionOnPlane();
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
    out var networkedClient))
            {
                var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
                if (player)
                {
                    player.SubmitPositionRequestServerRpc();
                }
            }

        }
        else
        {

            //SubmitPositionRequestServerRpc();
        }
    }

}
