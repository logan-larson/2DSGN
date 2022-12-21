using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using TMPro;

public class PlayerName : NetworkBehaviour
{
    [SyncVar (OnChange = nameof(OnUsernameChanged))]
    public string Username;

    public UserInfo UserInfo;

    [SerializeField]
    private TMP_Text _usernameText;

    private void OnUsernameChanged(string oldValue, string newValue, bool isServer)
    {
        _usernameText.text = newValue;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!base.IsOwner)
        {
            _usernameText.color = Color.red;
            return;
        }
        
        _usernameText.color = Color.green;

        Username = UserInfo.Username;

        ServerSetUsername(Username);
    }

    [ServerRpc]
    public void ServerSetUsername(string username)
    {
        Username = username;
    }
}
