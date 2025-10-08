using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BulletFollower : MonoBehaviour
{
    [SerializeField] float followFrecuence;

    private void Start()
    {
        StartCoroutine(FollowTick());
    }

    IEnumerator FollowTick()
    {
        while (true)
        {
            Follow();
            yield return new WaitForSeconds(followFrecuence);
        }
    }

    void Follow()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            Vector3 fixedDirection = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            Vector3 direction = fixedDirection - transform.position;
            transform.forward = direction;
        }
    }
}
