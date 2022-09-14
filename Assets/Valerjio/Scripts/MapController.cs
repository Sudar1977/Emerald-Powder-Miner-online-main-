using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : MonoBehaviour, IOnEventCallback
{
    [SerializeField] GameObject cellPrefab;

    GameObject[,] cells = new GameObject[20, 10];
    List<PlayerControls> players = new List<PlayerControls>();

    private void Start()
    {
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                cells[i, j] = Instantiate(cellPrefab, new Vector3Int(i, j, 0), Quaternion.identity, transform);
            }
        }

        PhotonPeer.RegisterType(typeof(Vector2Int), 242, SerializeV2Int, DeserializeV2Int);
    }

    public void AddPlayer(PlayerControls player)
    {
        players.Add(player);
        cells[player.Position.x, player.Position.y].SetActive(false);
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
        if (player.Position.x != another.Position.x) return false;
        if (player.Position.y <= another.Position.y) return false;
        
        int i = 1;
        while (player.Position.y-i > another.Position.y)
        {
            if (cells[player.Position.x, player.Position.y - i].activeSelf) break;
            i++;
        }
        if (player.Position.y - i == another.Position.y) return true;
        return false;
    }
    public bool isHaveEmptyCellBelow(Vector2Int position)
    {
        if (position.y > 0 && cells[position.x, position.y - 1].activeSelf) return false;
        else return true;
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != 42) return;
        var pos = (Vector2Int)photonEvent.CustomData;
        cells[pos.x, pos.y].SetActive(false);
    }
    public void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    public void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    public static byte[] SerializeV2Int(object obj)
    {
        Vector2Int cast = (Vector2Int)obj;
        byte[] result = new byte[8];
        BitConverter.GetBytes(cast.x).CopyTo(result, 0);
        BitConverter.GetBytes(cast.y).CopyTo(result, 4);
        return result;
    }
    public static object DeserializeV2Int(byte[] data)
    {
        var result = new Vector2Int();
        result.x = BitConverter.ToInt32(data, 0);
        result.y = BitConverter.ToInt32(data, 4);
        return result;
    }
}
