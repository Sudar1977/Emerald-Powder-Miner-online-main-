using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerControls : MonoBehaviour, IPunObservable, IOnEventCallback
{
    public Vector2Int Direction;
    public Vector2Int GamePosition;
    public PhotonView photonView;

//    [SerializeField] private float moveCulldown = .9f;
    [SerializeField] private Transform Ladder;
    [SerializeField] private GameObject bonesPrefab;

    private bool   isRight = false;
    private double lastTickTime;

    private Vector2    touchStarted;

    private MapController map;
    private SpriteRenderer spriteRender;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //string massage = JsonUtility.ToJson(new netStream() { isRight = this.isRight, position = this.GamePosition });
            //stream.SendNext(massage);
            stream.SendNext(Direction);

        }
        else if (stream.IsReading)
        {
            //var mail = JsonUtility.FromJson<netStream>((string)stream.ReceiveNext());
            //isRight = mail.isRight;
            //GamePosition = mail.position;
            Direction = (Vector2Int)stream.ReceiveNext();
        }
    }

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        map = FindObjectOfType<MapController>();
        spriteRender = GetComponent<SpriteRenderer>();

        GamePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        FindObjectOfType<MapController>().AddPlayer(this);

        if (!photonView.IsMine) spriteRender.color = Color.red; // enemy
    }

    private void Update()
    {
        //if (PhotonNetwork.Time < lastTick + moveCulldown) return;
        if (photonView.IsMine)
            HandleInput();

        if (Direction == Vector2Int.left)  spriteRender.flipX = false;
        if (Direction == Vector2Int.right) spriteRender.flipX = true;

        transform.position = Vector3.Lerp(transform.position, (Vector2)GamePosition,Time.deltaTime*3);

        //if (Position == beforMovePos) return;
        //PhotonNetwork.RaiseEvent(42, GamePosition, //options, sendOptions);
        //            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
        //            new SendOptions() { Reliability = true });
    }

    private void HandleInput()
    {

        if (Input.GetKey(KeyCode.A)) Direction = Vector2Int.left;
        if (Input.GetKey(KeyCode.D)) Direction = Vector2Int.right;
        if (Input.GetKey(KeyCode.W)) Direction = Vector2Int.up;
        if (Input.GetKey(KeyCode.S)) Direction = Vector2Int.down;

        if (Input.GetMouseButtonDown(0))
        {
            touchStarted = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {

            Vector2 touchEnded = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 swipe = touchEnded - touchStarted;
            if (swipe.magnitude > 2)
            {

                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                {
                    if (swipe.x > 0) 
                        Direction = Vector2Int.right;
                    else              
                        Direction = Vector2Int.left;
                }
                else
                {
                    if (swipe.y > 0) 
                        Direction = Vector2Int.up;
                    else 
                        Direction = Vector2Int.down;
                }
            }
        }
    }


    public void setLadderLength(int length)
    {
        for (int i = 0; i < Ladder.childCount; i++)
            Ladder.GetChild(i).gameObject.SetActive(i < length);

        while (Ladder.childCount < length)
            Instantiate(Ladder.GetChild(0), Ladder.position + Vector3.down * (Ladder.childCount +1), Quaternion.identity, Ladder);
    }

    void die()
    {
        Debug.Log("Death");
        PhotonNetwork.RaiseEvent(34, GamePosition,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true });
        PhotonNetwork.LeaveRoom();
    }
    // Важно! на другом компе вызывается два раза событие и не локальным убитым, и локальным убийцей
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != 34) return;
        var pos = (Vector2Int)photonEvent.CustomData;
        if (GamePosition != pos) return;
        var bonesObj = Instantiate(bonesPrefab, new Vector3Int(pos.x, pos.y, 0), Quaternion.identity);
        bonesObj.GetComponent<Bones>().InitBones(map, PhotonNetwork.Time, pos);
    }
    void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

}

public struct netStream
{
    public bool isRight;
    public Vector2Int position;
}