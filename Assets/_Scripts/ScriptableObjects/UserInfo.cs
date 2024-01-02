using PlayFab.ClientModels;
using UnityEngine;

[CreateAssetMenu(fileName = "UserInfo", menuName = "Persistence/UserInfo", order = 1)]
public class UserInfo : ScriptableObject
{
    public string Username;
    public int Wins;
    public int Kills;
    public int Deaths;

    public string IP;
    public int Port;

    public bool IsHost;
    public string ConnectionString;

    public EntityKey EntityKey;
}