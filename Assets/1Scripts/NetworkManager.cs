using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public InputField nickNameInput;
    public GameObject disConnectPanel;
    public GameObject respawnPanel;

    void Awake() {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

    }
    public void Connet() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster() {
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null);

    }
    public override void OnJoinedRoom() {
        disConnectPanel.SetActive(false);
        StartCoroutine("DestoryBullet");
        Spawn();
    }
    
    IEnumerator DestoryBullet() {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet"))
            GO.GetComponent<PhotonView>().RPC("DestoryRPC", RpcTarget.All);
    }
    public void Spawn() {
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-6f, 19f),4,0), Quaternion.identity);
        respawnPanel.SetActive(false);
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause) {
        disConnectPanel.SetActive(true);
        respawnPanel.SetActive(false); 
    }
}
