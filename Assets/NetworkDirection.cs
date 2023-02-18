using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class NetworkDirection : NetworkBehaviour
{

    [SerializeField]
    private InputSystem InputSystem;

    [SerializeField]
    private PlayerInputValues _input;

    [SyncVar(OnChange = nameof(OnChangeDirection))]
    public bool IsFacingRight;

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
        }
    }

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
}
