using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;


public class HelloWorldPlayer : NetworkBehaviour
{
    [SerializeField]
    float minMoveMouseDistThreshhold;
    [SerializeField]
    float maxVelocity;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {

    }

    void MoveInDirection(Vector3 direction)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            transform.position += direction.normalized * maxVelocity;
            Position.Value = transform.position;
        }
        else
        {
            SubmitMovementInDirectionRequestServerRpc(direction);
        }
    }

    [ServerRpc]
    void SubmitMovementInDirectionRequestServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        Position.Value += direction.normalized * maxVelocity;
    }

    void Update()
    {
        transform.position = Position.Value;
    }

    void FixedUpdate()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (mousePos.sqrMagnitude > minMoveMouseDistThreshhold * minMoveMouseDistThreshhold)
        {
            MoveInDirection(mousePos);
        }
    }
}
