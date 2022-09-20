using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject player;

    private void Start()
    {
        var pos = new Vector2(UnityEngine.Random.Range(1, 19), UnityEngine.Random.Range(1, 9));
        PhotonNetwork.Instantiate(player.name, pos, Quaternion.identity);
        PhotonPeer.RegisterType(typeof(Vector2Int), 242, SerializeVector2Int, DeserializeVector2Int);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left room");
    }

    public static byte[] SerializeVector2Int(object obj)
    {
        Vector2Int cast = (Vector2Int)obj;
        byte[] result = new byte[8];
        BitConverter.GetBytes(cast.x).CopyTo(result, 0);
        BitConverter.GetBytes(cast.y).CopyTo(result, 4);
        return result;
    }
    public static object DeserializeVector2Int(byte[] data)
    {
        var result = new Vector2Int();
        result.x = BitConverter.ToInt32(data, 0);
        result.y = BitConverter.ToInt32(data, 4);
        return result;
    }

}
