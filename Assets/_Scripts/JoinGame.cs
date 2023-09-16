using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/**
<summary>
JoinGame is responsible for passing the username to the Game scene and loading the game scene.
TODO: This should be refactored into two separate scripts. One for passing the username and one for loading the game scene.
</summary>
*/
public class JoinGame : MonoBehaviour
{
    public UserInfo UserInfo;
    public NetworkInfo NetworkInfo;
    public bool IsServerBuild;

    [SerializeField]
    private TMP_InputField _usernameInput;

    private void Start()
    {
        if (IsServerBuild)
        {
            NetworkInfo.IsServerBuild = IsServerBuild;
            SceneManager.LoadScene("Playground");
        }
    }

    public void OnJoinGame()
    {
        UserInfo.Username = _usernameInput.text;
        NetworkInfo.IsServerBuild = false;
        SceneManager.LoadScene("Playground");
    }
}
