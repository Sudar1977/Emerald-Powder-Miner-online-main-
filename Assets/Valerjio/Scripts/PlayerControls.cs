using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerControls : MonoBehaviour, IPunObservable, IOnEventCallback
{
    public Vector2Int Position;

    [SerializeField] Transform ladder;
    [SerializeField] float moveCulldown = .9f;
    [SerializeField] GameObject bonesPrefab;

    Vector2Int lastFramePosition;
    PhotonView view;
    MapController map;
    SpriteRenderer render;
    double lastTick;
    bool isRight = false;
    private Vector2 touchStarted;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            string massage = JsonUtility.ToJson(new netStream() { isRight = this.isRight, position = this.Position });
            stream.SendNext(massage);
        }
        else if (stream.IsReading)
        {
            var mail = JsonUtility.FromJson<netStream>((string)stream.ReceiveNext());
            isRight = mail.isRight;
            Position = mail.position;
        }
    }

    private void Start()
    {
        view = GetComponent<PhotonView>();
        map = FindObjectOfType<MapController>();
        render = GetComponent<SpriteRenderer>();

        Position = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        FindObjectOfType<MapController>().AddPlayer(this);

        if (!view.IsMine) render.color = Color.red; // enemy
    }

    private void Update()
    {
        if (isRight) render.flipX = false;
        else render.flipX = true;
        
        if (lastFramePosition != Position)
        {
            setLadderLength(map.MesureLadderLength(Position));
            lastFramePosition = Position;
        }

        if (!view.IsMine) return;

        if (map.isHavePlayerBelow(this) && PhotonNetwork.NetworkClientState != ClientState.Leaving) die();

        transform.position = Vector3.Lerp(new Vector3(Position.x, Position.y, 0), transform.position, .5f);

        if (PhotonNetwork.Time < lastTick + moveCulldown) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount != 2) return;

        var beforMovePos = Position;
        #region input and clamp pos
        if (Input.GetKeyDown(KeyCode.W))
            Position += Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.A))
        {
            Position += Vector2Int.left;
            isRight = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
            Position += Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.D))
        {
            Position += Vector2Int.right;
            isRight = true;
        }
        //       HandleInput();

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

                    if (swipe.x > 0) //Direction = Vector2int.right;
                    //if (Input.GetKeyDown(KeyCode.D))
                    {
                        Position += Vector2Int.right;
                        isRight = true;
                    }
                    else //Direction = Vector2Int.left;
                    //if (Input.GetKeyDown(KeyCode.A))
                    {
                        Position += Vector2Int.left;
                        isRight = false;
                    }
                }
                else
                {
                    if (swipe.y > 0) //Direction = Vector2Int.up:
                                     //if (Input.GetKeyDown(KeyCode.W))
                        Position += Vector2Int.up;
                    else //Direction = Vector2Int.down;
                         //if (Input.GetKeyDown(KeyCode.S))
                        Position += Vector2Int.down;
                }
            }
        }




        if (Position.x < 0) Position.x = 0;
        if (Position.x > 19) Position.x = 19;
        if (Position.y < 0) Position.y = 0;
        if (Position.y > 9) Position.y = 9;
        #endregion
        if (Position == beforMovePos) return;

        lastTick = PhotonNetwork.Time;

        PhotonNetwork.RaiseEvent(42, Position,
            new RaiseEventOptions() { Receivers = ReceiverGroup.All },
            new SendOptions() { Reliability = true });
    }

    private void HandleInput()
    {

        //if (Input.GetKey(KeyCode.LeftArrow)) Direction = Vector2Int.left;
        //if (Input.GetKey(KeyCode.RightArrow)) Direction = Vector2Int.right;
        //if (Input.GetKey(KeyCode.UpArrow)) Direction = Vector2Int.up;
        //if (Input.GetKey(KeyCode.DownArrow)) Direction = Vector2Int.down;

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

                    if (swipe.x > 0) //Direction = Vector2int.right;
                    //if (Input.GetKeyDown(KeyCode.D))
                    {
                        Position += Vector2Int.right;
                        isRight = true;
                    }
                    else //Direction = Vector2Int.left;
                    //if (Input.GetKeyDown(KeyCode.A))
                    {
                        Position += Vector2Int.left;
                        isRight = false;
                    }
                }
                else
                {
                    if (swipe.y > 0) //Direction = Vector2Int.up:
                    //if (Input.GetKeyDown(KeyCode.W))
                        Position += Vector2Int.up;
                    else //Direction = Vector2Int.down;
                    //if (Input.GetKeyDown(KeyCode.S))
                        Position += Vector2Int.down;
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
        PhotonNetwork.RaiseEvent(34, Position,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            new SendOptions { Reliability = true });
        PhotonNetwork.LeaveRoom();
    }
    // Важно! на другом компе вызывается два раза событие и не локальным убитым, и локальным убийцей
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != 34) return;
        var pos = (Vector2Int)photonEvent.CustomData;
        if (Position != pos) return;
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