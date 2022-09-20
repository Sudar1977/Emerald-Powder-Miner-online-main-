using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] private GameObject cellPrefab;

    private double lastTickTime;

    private GameObject[,] cells;
    private List<PlayerControls> players = new List<PlayerControls>();
    //private Vector2Int[] directions;

    public void AddPlayer(PlayerControls player)
    {
        players.Add(player);
        cells[player.GamePosition.x, player.GamePosition.y].SetActive(false);
    }
    private void Start()
    {
        cells = new GameObject[20, 10];

        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                cells[i, j] = Instantiate(cellPrefab, new Vector3Int(i, j, 0), Quaternion.identity, transform);
            }
        }
    }

    private void Update()
    {
        if(PhotonNetwork.Time > lastTickTime + 1 &&
           PhotonNetwork.IsMasterClient &&
           PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            //разослать всем событие
            Vector2Int[] directions = players
                .OrderBy(p => p.photonView.Owner.ActorNumber)
                .Select(p => p.Direction)
                .ToArray();
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //ReceiverGroup.Others };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent(42, directions, options, sendOptions);

            //сделать шаг игры
            PerformTick(directions);
        }
    }

    public int MesureLadderLength(Vector2Int position)
    {
        int res = 0;
        while (position.y - res > 0 && !cells[position.x, position.y - res - 1].activeSelf)
            res++;
        return res;
    }

    public bool isHavePlayerBelow(PlayerControls player)
    {
        if (players.Count != 2) return false;
        var another = players.First(p => p != player);
        if (player.GamePosition.x != another.GamePosition.x) return false;
        if (player.GamePosition.y <= another.GamePosition.y) return false;
        
        int i = 1;
        while (player.GamePosition.y-i > another.GamePosition.y)
        {
            if (cells[player.GamePosition.x, player.GamePosition.y - i].activeSelf) break;
            i++;
        }
        if (player.GamePosition.y - i == another.GamePosition.y) return true;
        return false;
    }
    public bool isHaveEmptyCellBelow(Vector2Int position)
    {
        if (position.y > 0 && cells[position.x, position.y - 1].activeSelf) return false;
        else return true;
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 42:
                //var pos = (Vector2Int)photonEvent.CustomData;
                //cells[pos.x, pos.y].SetActive(false);
                Vector2Int[] directions = (Vector2Int[])photonEvent.CustomData;
                PerformTick(directions);
                break;
        } 
    }

    private void PerformTick(Vector2Int[] directions)
    {
        if (players.Count != directions.Length) return;
        int i = 0;
        foreach(var player in players.OrderBy(p=>p.photonView.Owner.ActorNumber))
        {
            player.Direction = directions[i++];
            player.GamePosition += player.Direction;
            if (player.GamePosition.x < 0)  player.GamePosition.x = 0;
            if (player.GamePosition.y < 0)  player.GamePosition.y = 0;
            if (player.GamePosition.x > 19) player.GamePosition.x = cells.GetLength(0)-1;
            if (player.GamePosition.y > 9)  player.GamePosition.y = cells.GetLength(1)-1;
            cells[player.GamePosition.x, player.GamePosition.y].SetActive(false);
        }
        lastTickTime = PhotonNetwork.Time;
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

}
