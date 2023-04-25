using UnityEngine;

[CreateAssetMenu(fileName = "UserInfo", menuName = "Persistence/UserInfo", order = 1)]
public class UserInfo : ScriptableObject
{
    public string Username;
    public int Wins;
    public int Kills;
    public int Deaths;
}