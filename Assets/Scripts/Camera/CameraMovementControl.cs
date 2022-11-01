using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementControl : MonoBehaviour
{
    public Camera cam;
    public float moveSpeed = 3f;
    public float rotationSpeed = 70f;
    public float angleSpeed = 60f;
    public float zoomSpeed = 3000f;

    public float maxZoom = 100;
    public float minZoom = 20;

    public float maxOrthSize = 40f;
    public float minOrthSize = 8f;

    public float minAngle = 10f;
    public float maxAngle = 80f;

    public float minZ = -70f;
    public float minX = -40f;
    public float maxZ = 40f;
    public float maxX = 40f;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.F))
        {
            ChangeAngle();
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            Rotate();
        }

        Zoom();
    }

    private void Zoom()
    {
        if (cam.orthographic)
        {
            float zoomDt = Input.GetAxis("Mouse ScrollWheel") * -zoomSpeed * Time.deltaTime;
            float size = cam.orthographicSize + zoomDt;
            size = Mathf.Min(maxOrthSize, size);
            size = Mathf.Max(minOrthSize, size);
            cam.orthographicSize = size;
        }
        else
        {
            float zoomDt = Input.GetAxis("Mouse ScrollWheel") * -zoomSpeed * Time.deltaTime;
            if (zoomDt == 0)
            {
                return;
            }

            float fov = cam.fieldOfView + zoomDt;
            fov = Mathf.Max(minZoom, fov);
            fov = Mathf.Min(maxZoom, fov);

            cam.fieldOfView = fov;
            //cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, 200f * Time.deltaTime);
        }
    }

    private void Move()
    {
        float heightSpeed = cam.orthographic ? cam.orthographicSize/3 : Mathf.Log(cam.fieldOfView);
        float sideDt = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime * heightSpeed;
        float forwardDt = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime * heightSpeed;

        if (sideDt == 0 && forwardDt == 0)
        {
            return;
        }

        Vector3 forwardMovement = transform.forward * forwardDt;
        Vector3 sideMovement = transform.right * sideDt;

        Vector3 move = forwardMovement + sideMovement;
        Vector3 nextPos = transform.position + move;
        transform.position = nextPos;
        LockPositionInBounds();
    }

    private void LockPositionInBounds()
    {
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            transform.position.y,
            Mathf.Clamp(transform.position.z, minZ, maxZ)
        );
    }

    private void Rotate()
    {
        Quaternion rotation = transform.rotation;
        if (Input.GetKey(KeyCode.Q))
        {
            rotation *= Quaternion.Euler(Vector3.up * -rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotation *= Quaternion.Euler(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        transform.rotation = rotation;
    }

    private void ChangeAngle()
    {
        Transform camTransform = transform.GetChild(0).transform;
        float angleVal = Input.GetKey(KeyCode.F) ? angleSpeed : -angleSpeed;
        float nextX = angleVal * Time.deltaTime + camTransform.localRotation.eulerAngles.x;
        if (nextX < minAngle)
        {
            nextX = minAngle;
        }
        else if (nextX > maxAngle)
        {
            nextX = maxAngle;
        }

        Quaternion rotation = Quaternion.Euler(new Vector3(nextX, 0, 0));
        camTransform.localRotation = rotation;
    }
}
