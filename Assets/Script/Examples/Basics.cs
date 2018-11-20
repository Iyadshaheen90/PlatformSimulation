using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Basics : MonoBehaviour {

    public float spacing = 0.1f; // spacing between each cubes
    public float scaleOnY = 0.1f; // height of the cubes

    float speed = 1f; // speed for the cube to move up/down

    float displacementRange = 1.0f; // max height that the cube can move to

    // getting random cube (from array)
    int randomM, randomN;

    // hold random Y position for the cube to move to
    float movePositionY;

    // set m*n matrix
    public int m = 10;
    public int n = 10;

    GameObject[,] allCube; // array holding all the cubes
    
    //Transform[,] currentPosition; // holding position of current simulated cube
    //Transform[,] nextPosition; // holding the position of next cube to simulate

    float[,] nextPos; // holds cubes' position y. (m,n = cube coord.)

    bool simulating = false;

    // cube coloring
    GameObject currentSelection = null;
    Color nextColor;
    float y = 0.0f; // float value for position y

    // Use this for initialization
    void Start () {

        allCube = new GameObject[m, n];
        nextPos = new float[m, n];

        //currentPosition = new Transform[m, n];
        //simulating = false;

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                // create cube
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // give position
                cube.transform.position = new Vector3(i+(i*spacing), 0f, j+(j*spacing));
                cube.transform.rotation = Quaternion.identity;
                cube.transform.localScale = new Vector3(1f, scaleOnY, 1f); // set size of the cubes
                cube.name = string.Format("Cube-{0}-{1}", i, j);

                // adding a script to each cubes in runtime
                //cube.AddComponent<SomeOtherClass>();

                allCube[i, j] = cube;
                nextPos[i, j] = cube.transform.position.y;
                //currentPosition[i, j] = cube.transform;

                // interact with the inner cube script
                //cube.transform.GetComponent<SomeOtherClass>().someVar = newVar;
            }
        }

	}

    // Update is called once per frame
    void Update () {

        // ---------- keyboard input ----------

        // start and stop simulation
        if (Input.GetKeyDown(KeyCode.T))
        {
            // switch the simulation on/off
            simulating = !simulating;

            // button testing purposes
            if (simulating) Debug.Log("sim start");
            else Debug.Log("sim stop");
        }

        // key W increase displacement range
        if (Input.GetKeyDown(KeyCode.W))
        {
            displacementRange += 1.0f;
            Debug.Log("new displacementRange = " + displacementRange);
        }

        // key S decrease displacement range
        if (Input.GetKeyDown(KeyCode.S))
        {
            displacementRange -= 1.0f;
            if (displacementRange < 0f) displacementRange = 0f; // reset to 0 if it goes below
            Debug.Log("new displacementRange = " + displacementRange);
        }

        // ---------- moving the cubes (my algo) ----------

        
        if (simulating)
        {
            // get random m,n to set which cube to move
            randomM = Random.Range(0, m);
            randomN = Random.Range(0, n);

            // set currentSelection to be cube(m,n)
            currentSelection = allCube[randomM, randomN];

            // initialize the color and y-position change
            nextColor = Color.red;
            y = Random.Range(-displacementRange, displacementRange);

            // smooth transition color
            currentSelection.transform.gameObject.GetComponent<Renderer>().material.color =
                Color.Lerp(
                    currentSelection.transform.gameObject.GetComponent<Renderer>().material.color, // current color
                    nextColor, // changing the color to this
                    //Time.deltaTime // lerp time
                    speed
                );

            // smooth transition position
            currentSelection.transform.position =
                Vector3.Lerp(
                    currentSelection.transform.position,
                    new Vector3(currentSelection.transform.position.x, y, currentSelection.transform.position.z),
                    //Time.deltaTime
                    speed
                );

            /*
            // get displacement vector
            Vector3 newvector = new Vector3(0f, 0f, 0f);
            //newvector.y = displacementRange * Mathf.Sin(Time.time * speed); // sin to move it up/down (?)
            newvector.y = y;

            // get x and z position (don't wanna move it)
            newvector.x = allCube[randomM, randomN].transform.position.x;
            newvector.z = allCube[randomM, randomN].transform.position.z;

            Vector3 currentPosition = new Vector3(newvector.x, allCube[randomM, randomN].transform.position.y, newvector.z);

            // move the cube
            allCube[randomM, randomN].transform.position = newvector;
            */
        }
        else
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Vector3 resetVector = new Vector3(0, 0, 0);
                    resetVector.x = allCube[i, j].transform.position.x;
                    resetVector.z = allCube[i, j].transform.position.z;

                    allCube[i, j].transform.position = resetVector;
                }
            }
        }
        
        
        // change currentSelection to nextPos[i, j]

        /*
        if (simulating)
        {
            if (currentSelection != null)
            {
                // smooth transition color
                currentSelection.transform.gameObject.GetComponent<Renderer>().material.color =
                    Color.Lerp(
                        currentSelection.transform.gameObject.GetComponent<Renderer>().material.color, // current color
                        nextColor, // changing the color to this
                        Time.deltaTime // lerp time
                    );

                // smooth transition position
                currentSelection.transform.position = 
                    Vector3.Lerp(
                        currentSelection.transform.position,
                        new Vector3(currentSelection.transform.position.x, y, currentSelection.transform.position.z),
                        Time.deltaTime
                    );
            }
        }
        */

        /*
        if (allCube[i,j].transform.position.y == nextPos[i,j])
        {
            ...
        }
        */

        // ---------- rotating selected cube ----------

        /*
        if (currentSelection != null)
        {
            if (Input.GetKey(KeyCode.X))
            {
                currentSelection.transform.Rotate(new Vector3(1, 0, 0), 1.0f);
                Debug.Log("X key pressed");
            }
            if (Input.GetKey(KeyCode.Y))
            {
                currentSelection.transform.Rotate(Vector3.up, 1.0f);
                Debug.Log("Y pressed");
            }
            if (Input.GetKey(KeyCode.Z))
            {
                currentSelection.transform.Rotate(Vector3.forward, 1.0f);
                Debug.Log("Z pressed");
            }
        }
        */

        // ---------- get currentSelection and cube coloring ----------

        if (Input.GetMouseButtonUp(0))
        {

            // check if we're clicking UI element or cube
            //if (IsPointerOverUIObject())
            //    return;

            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

            if (hit)
            {
                Debug.Log(hitInfo.transform.gameObject.name + " clicked");

                // coloring the selected cube
                //hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
                //nextColor = new Color(r, g, b) // for random color
                nextColor = Color.red;
                y = Random.Range(-displacementRange, displacementRange);

                if (currentSelection != null)
                {
                    // resetting
                    currentSelection.transform.gameObject.GetComponent<Renderer>().material.color = Color.white;
                    currentSelection.transform.position = new Vector3(currentSelection.transform.position.x, 0, currentSelection.transform.position.z);

                    // new cube selected
                    currentSelection = hitInfo.transform.gameObject;
                }
                else
                {
                    currentSelection = hitInfo.transform.gameObject;
                }
            }
            else
            {
                Debug.Log("no hit!");
            }
        }

    }

    // to determine if we clicking over the UI element or not
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (var result in results)
        {
            Debug.Log(result.gameObject.name);
        }

        return results.Count > 0;
    }

}
