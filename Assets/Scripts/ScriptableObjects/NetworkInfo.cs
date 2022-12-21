using UnityEngine;

[CreateAssetMenu(fileName = "NetworkInfo", menuName = "Persistence/NetworkInfo", order = 2)]
public class NetworkInfo : ScriptableObject
{
    public bool IsServerBuild;
}