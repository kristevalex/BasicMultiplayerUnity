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
    [SerializeField]
    float maxRotationVelocity;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableVector3 Direction = new NetworkVariableVector3(new NetworkVariableSettings
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
        SubmitMovementInDirectionRequestServerRpc(direction);
        if (NetworkManager.Singleton.IsServer)
            transform.position = Position.Value;
    }

    [ServerRpc]
    void SubmitMovementInDirectionRequestServerRpc(Vector3 targetDirection, ServerRpcParams rpcParams = default)
    {
        Direction.Value = Vector3.RotateTowards(Direction.Value, targetDirection, maxRotationVelocity, float.PositiveInfinity);
        Position.Value += Direction.Value.normalized * maxVelocity;
    }

    void Update()
    {
        transform.position = Position.Value;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, Direction.Value);
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
