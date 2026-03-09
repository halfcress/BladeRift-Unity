using UnityEngine;

public class BillboardFacing : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        // Make the object face the camera
        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f; // keep upright so it doesn't tilt

        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(-dir);
        }
    }
}
