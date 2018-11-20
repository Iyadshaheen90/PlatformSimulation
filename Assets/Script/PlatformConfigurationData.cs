using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 *  Keep basic platform configuration here
 *  call ToString() to print out our platform configuration into save file
 */
public class PlatformConfigurationData : MonoBehaviour {

    // default value
    public int mSize = 16;
    public int nSize = 9;
    public float deltaSpace = 0.1f; // spacing between cubes
    public float height = 1.0f; // y-axis range

    public override string ToString()
    {
        return string.Format("{0},{1},{2},{3}", mSize, nSize, deltaSpace, height);
    }
}
