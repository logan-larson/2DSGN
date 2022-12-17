using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
            SceneManager.LoadScene("Main");
        }
    }

    public void OnJoinGame()
    {
        UserInfo.Username = _usernameInput.text;
        NetworkInfo.IsServerBuild = false;
        SceneManager.LoadScene("Main");
    }
}
