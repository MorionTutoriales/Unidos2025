using UnityEngine;

public class SigueCamMiniMap : MonoBehaviour
{
    public Vector3 offset;
    public Transform target;

    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
