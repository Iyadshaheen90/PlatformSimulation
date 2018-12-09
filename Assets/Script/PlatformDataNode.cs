using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDataNode : MonoBehaviour {

    // ----- node variables -----

    // initialized in PlatformManager
    public int iPosition;
    public int jPosition;

    public float yPosition; // individual node will hold their own y-axis position

    public Color nodeColor;

    // state of the node
    public bool isSelected;
    public bool isProgrammed;
    public bool isSimulated;

    public delegate void UpdatePlatformDataNodeUI(PlatformDataNode pdn);
    public static event UpdatePlatformDataNodeUI OnUpdatePlatformDataNodeUI;

    // subscribing delegates
    private void OnEnable()
    {
        UIManager.OnNodeProgramChanged += UIManager_OnNodeProgramChanged;
    }

    // unsubscribing delegates
    private void OnDisable()
    {
        UIManager.OnNodeProgramChanged -= UIManager_OnNodeProgramChanged;
    }

    private void UIManager_OnNodeProgramChanged(float val)
    {
        if (isProgrammed) // if we're in Programming scene
        {
            if (isSelected) // if the cube is selected
            {
                Debug.Log("Programming " + transform.name + " height");
                transform.position = new Vector3(transform.position.x, val, transform.position.z);
                yPosition = val;
            }
        }
    }

    // Use this for initialization
    void Start () {
        //yPosition = transform.position.y;
        transform.gameObject.GetComponent<Renderer>().material.color = Color.white;
        //ResetDataNode();
    }

    // Update is called once per frame
    void Update () {

        //if (isProgrammed) // if we're in Programming scene
        //{
        //    if (isSelected) // if the cube is selected
        //    {
        //        // change cube color to blue when we clicked on it
        //        transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;
        //    }
        //}

        if (isSimulated)
        {
            // smooth transition position
            transform.position =
                Vector3.Lerp(
                    transform.position, // current position
                    new Vector3(transform.position.x, yPosition, transform.position.z), // destination
                    //Time.deltaTime // lerp time
                    0.05f
                );

            // smooth transition color
            //if (yPosition != 0f)
            //{
            //    transform.gameObject.GetComponent<Renderer>().material.color =
            //        Color.Lerp(
            //            transform.gameObject.GetComponent<Renderer>().material.color, // current color
            //            Color.red, // changing the color to this
            //            //Time.deltaTime // lerp time
            //            0.05f
            //        );
            //}
        }

    }

    // resetting data node when not in programming scene
    public void ResetDataNode()
    {
        isSelected = false;

        transform.gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        //yPosition = 0f;
    }

    public void SelectNode()
    {
        isSelected = true;
        transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;

        if (OnUpdatePlatformDataNodeUI != null)
            OnUpdatePlatformDataNodeUI(this);
    }

    // setting the height of each node when we go to "Simulate" scene straight away 
    public void SimulationSetHeight(float val)
    {
        if (isSimulated) // if we're in Simulation scene
        {
            Debug.Log("Simulating " + transform.name + " height");
            transform.position = new Vector3(transform.position.x, val, transform.position.z);
            yPosition = val;
        }
    }

    public void SetProgrammedHeight(float val)
    {
        yPosition = val;
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2}", iPosition, jPosition, yPosition);
    }
}
