using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class GazePairCandidate : NetworkBehaviour
{
    Camera maincam;
    public void Start()
    {

        maincam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        // WritePermission = NetworkVariablePermission.ServerOnly,
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart()
    {
        Move();
    }

    public void Move()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            transform.position = maincam.transform.position;
            Position.Value = maincam.transform.position;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        transform.position = Position.Value;
        Position.Value = maincam.transform.position;
    }


    void Update()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
                    out var networkedClient))
        {
            var player = networkedClient.PlayerObject.GetComponent<GazePairCandidate>();
            if (player)
            {
                player.Move();
            }
        }
    }
}
