using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPunCallbacks {
    public PhotonView pv;
    int dir;

    private void Start() => Destroy(gameObject, 3.5f);
    private void Update() => transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Ground") pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
        if (!pv.IsMine && other.tag == "Player" && other.GetComponent<PhotonView>().IsMine) {//느린쪽에 맞춰서 HIT판정
            other.GetComponent<Player>().Hit();
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);
}
