using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerControls : MonoBehaviour, IPunObservable, IOnEventCallback
{
    public Vector2Int Direction;
    public Vector2Int GamePosition;


    [SerializeField] private float moveCulldown = .9f;
    [SerializeField] private Transform ladder;
    [SerializeField] private GameObject bonesPrefab;

    private bool   isRight = false;
    private double lastTickTime;

    private Vector2    touchStarted;


    public PhotonView photonView;
    private MapController map;
    private SpriteRenderer spriteRender;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //            string massage = JsonUtility.ToJson(new netStream() { isRight = this.isRight, position = this.GamePosition });
            //            stream.SendNext(massage);
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
        //if (isRight) 
        //    spriteRender.flipX = false;
        //else 
        //    spriteRender.flipX = true;
        
        //if (Direction != GamePosition)
        //{
        //    setLadderLength(map.MesureLadderLength(GamePosition));
        //    Direction = GamePosition;
        //}

        if (photonView.IsMine)
        {
            //if (Input.GetKeyDown(KeyCode.W)) Direction = Vector2Int.up;
            //if (Input.GetKeyDown(KeyCode.A)) Direction = Vector2Int.left;
            //if (Input.GetKeyDown(KeyCode.S)) Direction = Vector2Int.down;
            //if (Input.GetKeyDown(KeyCode.D)) Direction = Vector2Int.right;
            HandleInput();
        }

        if (Direction == Vector2Int.left)  spriteRender.flipX = false;
        if (Direction == Vector2Int.right) spriteRender.flipX = true;

        transform.position = Vector3.Lerp(transform.position, (Vector2)GamePosition,Time.deltaTime*3);


        //if (map.isHavePlayerBelow(this) && PhotonNetwork.NetworkClientState != ClientState.Leaving) die();

        //transform.position = Vector3.Lerp(new Vector3(GamePosition.x, GamePosition.y, 0), transform.position, .5f);

        //if (PhotonNetwork.Time < lastTickTime + moveCulldown) return;
        //if (PhotonNetwork.CurrentRoom.PlayerCount != 2) return;

        //var beforMovePos = GamePosition;
        //#region input and clamp pos
        //if (Input.GetKeyDown(KeyCode.W))
        //    GamePosition += Vector2Int.up;
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    GamePosition += Vector2Int.left;
        //    isRight = false;
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //    GamePosition += Vector2Int.down;
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    GamePosition += Vector2Int.right;
        //    isRight = true;
        //}

        //if (GamePosition.x < 0) GamePosition.x = 0;
        //if (GamePosition.x > 19) GamePosition.x = 19;
        //if (GamePosition.y < 0) GamePosition.y = 0;
        //if (GamePosition.y > 9) GamePosition.y = 9;
        //#endregion
        //if (GamePosition == beforMovePos) return;

        //lastTickTime = PhotonNetwork.Time;

        //RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //ReceiverGroup.Others };
        //SendOptions sendOptions = new SendOptions { Reliability = true };
        //PhotonNetwork.RaiseEvent(42, GamePosition, options, sendOptions);
        //            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
        //            new SendOptions() { Reliability = true });
    }

    private void HandleInput()
    {

        if (Input.GetKey(KeyCode.LeftArrow)) Direction = Vector2Int.left;
        if (Input.GetKey(KeyCode.RightArrow)) Direction = Vector2Int.right;
        if (Input.GetKey(KeyCode.UpArrow)) Direction = Vector2Int.up;
        if (Input.GetKey(KeyCode.DownArrow)) Direction = Vector2Int.down;

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

                    if (swipe.x > 0) Direction = Vector2Int.right;
                    //if (Input.GetKeyDown(KeyCode.D))
                    //{
                    //    GamePosition += Vector2Int.right;
                    //    isRight = true;
                    //}
                    else 
                        Direction = Vector2Int.left;
                    ////if (Input.GetKeyDown(KeyCode.A))
                    //{
                    //    GamePosition += Vector2Int.left;
                    //    isRight = false;
                    //}
                }
                else
                {
                    if (swipe.y > 0) Direction = Vector2Int.up;
                    //if (Input.GetKeyDown(KeyCode.W))
                        //GamePosition += Vector2Int.up;
                    else Direction = Vector2Int.down;
                    //if (Input.GetKeyDown(KeyCode.S))
                        //GamePosition += Vector2Int.down;
                }
            }
        }
    }


    void setLadderLength(int length)
    {
        for (int i = 0; i < ladder.childCount; i++)
            ladder.GetChild(i).gameObject.SetActive(i < length);

        while (ladder.childCount < length)
            Instantiate(ladder.GetChild(0), ladder.position + Vector3.down * (ladder.childCount +1), Quaternion.identity, ladder);
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