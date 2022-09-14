using Photon.Pun;
using UnityEngine;

public class Bones : MonoBehaviour
{
    MapController map;
    Vector2Int position;
    double lastMoveStepTime;

    public void InitBones(MapController map, double photonTime, Vector2Int position)
    {
        this.map = map;
        lastMoveStepTime = photonTime;
        this.position = position;
    }

    private void Update()
    {
        if (map.isHaveEmptyCellBelow(position) && PhotonNetwork.Time > lastMoveStepTime + .5f)
        {
            lastMoveStepTime = PhotonNetwork.Time;
            position += Vector2Int.down;
        }
        transform.position = Vector3.Lerp(new Vector3(position.x, position.y, 0), transform.position, .5f);
    }
}
