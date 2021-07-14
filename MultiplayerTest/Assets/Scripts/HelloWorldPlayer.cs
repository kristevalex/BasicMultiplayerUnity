using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;


public static class NetworkVariableSettingsTemplates
{
    public static readonly NetworkVariableSettings ServerOnlyWrite = new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    };

    public static readonly NetworkVariableSettings NoRestrictions = new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    };
}


public class HelloWorldPlayer : NetworkBehaviour
{
    [SerializeField]
    float minMoveMouseDistThreshhold;
    [SerializeField]
    float maxVelocity;
    [SerializeField]
    float maxRotationVelocity;
    [SerializeField]
    float maxHealth;
    [SerializeField]
    Transform modelTransform;
    [SerializeField]
    GameObject playerCamera;
    [SerializeField]
    Slider healthSlider;
    [SerializeField]
    Rigidbody2D body;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(NetworkVariableSettingsTemplates.ServerOnlyWrite);
    public NetworkVariableVector3 Direction = new NetworkVariableVector3(NetworkVariableSettingsTemplates.ServerOnlyWrite);
    public NetworkVariableFloat MaxHealth = new NetworkVariableFloat(NetworkVariableSettingsTemplates.ServerOnlyWrite);
    public NetworkVariableFloat Health = new NetworkVariableFloat(NetworkVariableSettingsTemplates.ServerOnlyWrite);
    public NetworkVariableBool PlayerSpawned = new NetworkVariableBool(NetworkVariableSettingsTemplates.ServerOnlyWrite);

    public override void NetworkStart()
    {
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

            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        if (!IsLocalPlayer)
            return;
        SubmitSpawnPlayerRequestServerRpc();
    }

    [ServerRpc]
    void SubmitSpawnPlayerRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        MaxHealth.Value = maxHealth;
        Health.Value = maxHealth;
        PlayerSpawned.Value = true;
    }

    void MoveInDirection(Vector3 direction)
    {
        if (!IsLocalPlayer)
            return;
        SubmitMovementInDirectionRequestServerRpc(direction);
    }

    [ServerRpc]
    void SubmitMovementInDirectionRequestServerRpc(Vector3 targetDirection, ServerRpcParams rpcParams = default)
    {
        Direction.Value = Vector3.RotateTowards(Direction.Value, targetDirection, maxRotationVelocity, float.PositiveInfinity);

        body.velocity = Direction.Value.normalized * maxVelocity;
    }

    void StopMoving()
    {
        if (!IsLocalPlayer)
            return;

        SubmitStopMovingRequestServerRpc();
    }

    [ServerRpc]
    void SubmitStopMovingRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        body.velocity = Vector2.zero;
    }

    public void ReciveHP(float hp)
    {
        SubmitReciveHPRequestServerRpc(hp);
    }

    [ServerRpc]
    public void SubmitReciveHPRequestServerRpc(float hp, ServerRpcParams rpcParams = default)
    {
        Health.Value = Mathf.Max(0f, Mathf.Min(MaxHealth.Value, Health.Value + hp));
    }

    [ServerRpc]
    void SubmitAOEDamageRequestServerRpc(Vector3 targetCenter, float radius, float damage, ulong clientId, ServerRpcParams rpcParams = default)
    {
        AOEDamage dm = new AOEDamage();
        dm.DamageInArea(transform.position, 100, 20, NetworkManager.Singleton.LocalClientId);
    }

    void Update()
    {
        if (!PlayerSpawned.Value)
            return;
        modelTransform.rotation = Quaternion.FromToRotation(Vector3.up, Direction.Value);
        healthSlider.value = Health.Value / MaxHealth.Value;

        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = transform.position;
        }
        else
        {
            transform.position = Position.Value;
        }

        if (IsLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SubmitAOEDamageRequestServerRpc(transform.position, 100, 20, NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer)
            return;
        Vector3 mousePos = ScreenPosition.ZeroFromBottomLeftToCenter(Input.mousePosition);
        if (mousePos.sqrMagnitude > minMoveMouseDistThreshhold * minMoveMouseDistThreshhold)
        {
            MoveInDirection(mousePos);
        }
        else
        {
            StopMoving();
        }
    }
}
