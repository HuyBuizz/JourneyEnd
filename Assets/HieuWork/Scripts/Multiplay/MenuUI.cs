using Unity.Netcode;
using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public void HostBtnClick()
    {
        Debug.Log("Host button clicked");
        NetworkManager.Singleton.StartHost();
        // Add logic to start hosting a game
    }
    public void JoinBtnClick()
    {
        Debug.Log("Join button clicked");
        NetworkManager.Singleton.StartClient();
        // Add logic to join a game
    }
}
