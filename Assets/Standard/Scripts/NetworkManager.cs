using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

    [RPC]
    void GlobalChatText(string name, string msg)
    {
        Debug.Log(msg);
    }
}
