using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    void LateUpdate()
    {
        this.transform.position = player.position + offset;
    }
}
