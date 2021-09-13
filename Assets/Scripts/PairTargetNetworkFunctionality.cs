using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class PairTargetNetworkFunctionality : NetworkBehaviour
{

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart()
    {
        Move();
    }

    public void Move()
    {

            Position.Value = transform.position;


    }


    void Update()
    {
        //transform.position = Position.Value;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
    out var networkedClient))
        {
            var player = networkedClient.PlayerObject.GetComponent<PairTargetNetworkFunctionality>();
            if (player)
            {
                player.Move();
            }
        }
    }

}
