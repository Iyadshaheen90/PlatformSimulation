using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCameraControl : MonoBehaviour
{
    public float centerX = 0f;
    public float centerZ = 0f;

    public float speed = 30.0f;

    protected float fDistance = 1;
    protected float fSpeed = 1;

    Vector3 targetPosition;

    private void OnEnable()
    {
        UIManager.BuildPlatformOnClicked += UIManager_BuildPlatformOnClicked;
        //UIManager.OnUpdateCameraPosition += UIManager_BuildPlatformOnClicked;
        //PlatformManager.OnUpdateCameraPosition += UIManager_BuildPlatformOnClicked;
    }

    private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
    {
        //centerX = pcd.mSize / 2;
        //centerZ = pcd.nSize / 2;

        //transform.position = new Vector3(pcd.mSize, 10, pcd.nSize);
        //targetPosition = new Vector3(centerX, 3, centerZ);
        UpdateCameraPosition(pcd);
    }

    private void OnDisable()
    {
        UIManager.BuildPlatformOnClicked -= UIManager_BuildPlatformOnClicked;
        //UIManager.OnUpdateCameraPosition -= UIManager_BuildPlatformOnClicked;
        //PlatformManager.OnUpdateCameraPosition += UIManager_BuildPlatformOnClicked;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(targetPosition);

        #region ZOOM IN/OUT
        if (Input.GetKey(KeyCode.LeftShift))
            if(Input.GetKey(KeyCode.UpArrow))
                transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftShift))
            if (Input.GetKey(KeyCode.DownArrow))
                transform.Translate(-Vector3.forward * speed * Time.deltaTime);
        #endregion

        #region ROTATE VERTICAL/HORIZONTAL
        if (Input.GetKey(KeyCode.RightArrow))
            transform.RotateAround(targetPosition, -Vector3.up, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.RotateAround(targetPosition, Vector3.up, Time.deltaTime * speed);


        if (Input.GetKey(KeyCode.UpArrow))
            transform.RotateAround(targetPosition, Vector3.right, Time.deltaTime * speed);
        if (Input.GetKey(KeyCode.DownArrow))
            transform.RotateAround(targetPosition, -Vector3.right, Time.deltaTime * speed);
        #endregion
    }

    public void UpdateCameraPosition(PlatformConfigurationData pcd)
    {
        // centerX = pcd.mSize / 2;
        // centerZ = pcd.nSize / 2;

        // transform.position = new Vector3(pcd.mSize, 10, pcd.nSize);
        // targetPosition = new Vector3(centerX, 3, centerZ);

        centerX = pcd.mSize / 4;
        centerZ = pcd.nSize / 2;

        transform.position = new Vector3(centerX/2, 10, centerZ/2);
        targetPosition = new Vector3(centerX, 3, centerZ);
    }
}
