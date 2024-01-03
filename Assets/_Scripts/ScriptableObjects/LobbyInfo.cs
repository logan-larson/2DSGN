using PlayFab.ClientModels;
using UnityEngine;

[CreateAssetMenu(fileName = "LobbyInfo", menuName = "Persistence/LobbyInfo", order = 2)]
public class LobbyInfo : ScriptableObject
{
    public string LobbyID;
    public string ConnectionString;
}