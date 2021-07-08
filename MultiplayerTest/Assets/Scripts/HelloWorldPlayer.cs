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
    [SerializeField]
    Transform modelTransform;
    [SerializeField]
    GameObject playerCamera;

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

        if (IsLocalPlayer)
            playerCamera.SetActive(true);
        else
            playerCamera.SetActive(false);

        if (IsLocalPlayer)
        {
            GameObject defaulfCamera = GameObject.FindGameObjectWithTag("GlobalCamera");
            if (defaulfCamera)
                defaulfCamera.SetActive(false);
            else
                Debug.LogWarning("No default camera found");
        }
    }

    void SpawnPlayer()
    {

    }

    void MoveInDirection(Vector3 direction)
    {
        if (!IsLocalPlayer)
            return;
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
        modelTransform.rotation = Quaternion.FromToRotation(Vector3.up, Direction.Value);
    }

    void FixedUpdate()
    {
        Vector3 mousePos = ScreenPosition.ZeroFromBottomLeftToCenter(Input.mousePosition);
        if (mousePos.sqrMagnitude > minMoveMouseDistThreshhold * minMoveMouseDistThreshhold)
        {
            MoveInDirection(mousePos);
        }
    }
}
