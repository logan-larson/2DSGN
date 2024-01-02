using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject HostLobbyMenu;
    public GameObject JoinLobbyMenu;

    public UserInfo UserInfo;

    public void OpenHostLobbyMenu()
    {
        HostLobbyMenu.SetActive(true);
    }

    public void CloseHostLobbyMenu()
    {
        HostLobbyMenu.SetActive(false);
    }

    public void OpenJoinLobbyMenu()
    {
        JoinLobbyMenu.SetActive(true);
    }

    public void CloseJoinLobbyMenu()
    {
        JoinLobbyMenu.SetActive(false);
    }
    
    public void OpenFreeplayGame()
    {
        UserInfo.Username = "Me";
        SceneManager.LoadScene("BigMap");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
