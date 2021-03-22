using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D rg;
    public Animator an;
    public SpriteRenderer sr;
    public PhotonView pv;
    public Text nickNameText;
    public Image healthImage;

    bool isGround;
    Vector3 curPos;

    void Awake() {
        //닉네임
        nickNameText.text = pv.IsMine ? PhotonNetwork.NickName : pv.Owner.NickName;
        nickNameText.color = pv.IsMine ? Color.green : Color.red;

        if (pv.IsMine) {
            var cm = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            cm.Follow = transform;
            cm.LookAt = transform;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine) {
            float axis = Input.GetAxisRaw("Horizontal");
            rg.velocity = new Vector2(4 * axis, rg.velocity.y);

            if (axis != 0) {
                an.SetBool("walk", true);
                pv.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);//재접속시 filpX를 동기화해주기 위해서 AllBuffered

            } else an.SetBool("walk", false);

            //점프, 바닥 체크
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            an.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGround) pv.RPC("JumpRpc", RpcTarget.All);

            if (Input.GetKeyDown(KeyCode.Space)) {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(sr.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, sr.flipX ? -1 : 1);
                an.SetTrigger("shot");
            }

        }
        //isMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    public void Hit() {
        healthImage.fillAmount -= 0.1f;
        if(healthImage.fillAmount <= 0) {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pv.RPC("DestoryRPC", RpcTarget.AllBuffered);//AllBuffered해라 제대로 사라져 복제버그가 안 생긴다.
        }
    }

    [PunRPC]
    void DestoryRPC() => Destroy(gameObject);
    [PunRPC]
    void FlipXRPC(float axis) => sr.flipX = axis == -1;

    [PunRPC]
    void JumpRpc() {
        rg.velocity = Vector2.zero;
        rg.AddForce(Vector2.up * 700);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        } else {
            curPos = (Vector3)stream.ReceiveNext();
            healthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
