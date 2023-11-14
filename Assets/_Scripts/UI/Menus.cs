using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menus : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject HostLobbyMenu;
    public GameObject JoinLobbyMenu;

    public void OpenHostLobbyMenu()
    {
        HostLobbyMenu.SetActive(true);
    }

    public void OpenJoinLobbyMenu()
    {
        JoinLobbyMenu.SetActive(true);
    }

    public void CloseHostLobbyMenu()
    {
        HostLobbyMenu.SetActive(false);
    }

    public void CloseJoinLobbyMenu()
    {
        JoinLobbyMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
