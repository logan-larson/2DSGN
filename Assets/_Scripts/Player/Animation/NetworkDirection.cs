using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Transporting;

/**
<summary>
NetworkDirection is responsible for syncing the direction of the player.
</summary>
*/
public class NetworkDirection : NetworkBehaviour
{

    [SerializeField]
    private InputSystem InputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SyncVar(OnChange = nameof(OnChangeDirection))]
    public bool IsFacingRight = true;

    private void OnChangeDirection(bool oldValue, bool newValue, bool isServer)
    {
        if (newValue)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            _input = InputSystem.InputValues;

            ServerSetDirection(true);
        }
    }

    /* This was my attempt at syncing the direction of the player when they join the game.
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Listen for new connections.
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        // When a new connection is made, send the current mode to the new connection so they can sync.
        TargetSetMode(conn, IsFacingRight);
    }

    [TargetRpc]
    private void TargetSetMode(NetworkConnection conn, bool isFacingRight)
    {
        IsFacingRight = isFacingRight;
    }
    */

    private void Update()
    {
        if (base.IsOwner)
        {
            if (_input.HorizontalMovementInput > 0f)
                ServerSetDirection(true);
            else if (_input.HorizontalMovementInput < 0f)
                ServerSetDirection(false);
        }
    }

    [ServerRpc]
    public void ServerSetDirection(bool isFacingRight)
    {
        IsFacingRight = isFacingRight;
    }

    [ObserversRpc]
    private void ChangeDirectionObservers(bool isFacingRight)
    {
        IsFacingRight = isFacingRight;
    }
}
