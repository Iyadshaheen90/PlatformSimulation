using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlatformManager : PlatformGenericSinglton<PlatformManager> {

    // ----- variables -----

    //private float prevSpacing = 0.1f; // default spacing between each cubes
    //private float scaleOnY = 0.1f; // height of the cubes

    //private float displacementRange = 1.0f; // max height that the cube can move to

    // set default value m*n matrix
    //private int m = 16;
    //private int n = 9;

    // keep previous value of M and N
    //private int prevM = 0, prevN = 0;

    public GameObject cubePrefab; // cube prefab
    public GameObject[,] allCube; // array holding all the cubes

    //private float[,] nextPos; // holds cubes' next y-axis position value. (m,n = cube coord.)
    //private Color[,] nextColor; // holds cubes' color (m,n = cube coord.)

    // flag to determine current scene (config, programming or simulating)
    private bool simulatingScene = false;
    private bool programmingScene = false;

    /**
     *  which color option the sim currently is on
     *  0 = grayscale, 1 = red, 2 = green, 3 = blue, 4 = all RGB
     *  color is set to grayscale once the simulation is started
     */
    private int colorOption = 0;

    public Material cubeMaterial;

    public PlatformConfigurationData configData;

    public GameObject currentSelection; // holds current cube that got clicked

    // ----- delegates and events -----

    public delegate void PlatformManagerChanged(PlatformConfigurationData pcd);
    public static event PlatformManagerChanged OnPlatformManagerChanged;

    //public delegate void PlatformManagerUpdateUI(string nodeName);
    //public static event PlatformManagerUpdateUI OnPlatformManagerUpdateUI;

    // subscribing to delegates here
    private void OnEnable()
    {
        UIManager.BuildPlatformOnClicked += UIManager_BuildPlatformOnClicked;
        UIManager.OnWriteProgramData += UIManager_OnWriteProgramData;

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    // unsubscribing to delegates here
    private void OnDisable()
    {
        UIManager.BuildPlatformOnClicked -= UIManager_BuildPlatformOnClicked;
        UIManager.OnWriteProgramData -= UIManager_OnWriteProgramData;

        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    // clicking the green build button
    private void UIManager_BuildPlatformOnClicked(PlatformConfigurationData pcd)
    {
        // erase previous configData if available
        if (configData != null)
        {
            Debug.Log("configData != null. Destroying and create new..");

            // create new configData
            PlatformConfigurationData newConfig = gameObject.AddComponent<PlatformConfigurationData>();

            // destroying the old ones
            Destroy(configData);
            //configData = null;

            // set up new ones
            configData = newConfig;
        }
        // if not available, rebuild and reconfigure configData
        else
        {
            Debug.Log("configData == null. Creating new..");
            //configData = Instantiate(pcd);
            configData = gameObject.AddComponent<PlatformConfigurationData>();
        }

        // copy pcd data to configData
        configData.mSize = pcd.mSize;
        configData.nSize = pcd.nSize;
        configData.deltaSpace = pcd.deltaSpace;
        configData.height = pcd.height;

        BuildPlatform();

        // keep hold of the m and n value
        //prevM = pcd.mSize;
        //Debug.Log("prevM = " + prevM);
        //prevN = pcd.nSize;
        //Debug.Log("prevN = " + prevN);
        //prevSpacing = pcd.deltaSpace;
    }

    // writing save file
    private void UIManager_OnWriteProgramData(PlatformConfigurationData pcd)
    {
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            // ln 1 will be the platform configuration
            outputFile.WriteLine(pcd.ToString());

            // the rest of the file will have individual node's position
            for (int i = 0; i < pcd.mSize; i++)
            {
                for (int j = 0; j < pcd.nSize; j++)
                {
                    // accessing the instance of PlatformManager to get individual cube's status
                    outputFile.WriteLine(Instance.allCube[i, j].gameObject.GetComponent<PlatformDataNode>().ToString());
                }
            }
        }
    }

    // reading data from a file
    //private void UIManager_OnReadProgramData(PlatformConfigurationData pcd)
    public void readFile()
    {
        using (StreamReader sr = new StreamReader(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            bool isFirstLine = true;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                //Debug.Log(line);
                if (isFirstLine)
                {
                    string[] firstLine = line.Split(',');

                    // creating new PCD and feed it the data from firstLine string
                    PlatformConfigurationData newPCD = gameObject.AddComponent<PlatformConfigurationData>();

                    int.TryParse(firstLine[0], out newPCD.mSize);
                    int.TryParse(firstLine[1], out newPCD.nSize);
                    float.TryParse(firstLine[2], out newPCD.deltaSpace);
                    float.TryParse(firstLine[3], out newPCD.height);

                    isFirstLine = false; // set bool to false to access below else part
                    UIManager_BuildPlatformOnClicked(newPCD); // building platform using PCD data
                }
                else
                {
                    string[] currentLine = line.Split(',');

                    // call SimulationSetHeight() from PDN to set + transform each node's height
                    allCube[int.Parse(currentLine[0]), int.Parse(currentLine[1])].gameObject.transform.
                        GetComponent<PlatformDataNode>().SimulationSetHeight(float.Parse(currentLine[2]));
                }
            }
        }
    }

    // triggered when the scene got loaded
    // parameter: arg0.name will give the current scene's name
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        // when we creating platform from scratch
        if (allCube != null)
        {
            if (arg0.name.Equals("Programming"))
            {
                programmingScene = true; // set programming flag to true (others defaulted to false)
            }
            else
            {
                programmingScene = false;
            }

            // build (and rebuild) the platform if we're not on Main Menu scene
            if (!arg0.name.Equals("MainMenu"))
            {
                Debug.Log("Changing to arg0 (" + arg0.name + ") scenes, rebuilding platform..");
                BuildPlatform();
            }
            
        }
        else // allCube[,] is empty when we go straight into Simulate scene
        {
            if (arg0.name.Equals("Simulate"))
            {
                simulatingScene = true; // set simulating flag to true (others defaulted to false)
                readFile();
            }
        }
    }


    // Use this for initialization
    void Start () {

        //BuildPlatform(m, n, prevSpacing);

    }
	
	// Update is called once per frame
	void Update () {

        // ---------- keyboard input ----------

        // start and stop simulation
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetSimulation();
        }

        // key W increase displacement range
        if (Input.GetKeyDown(KeyCode.W))
        {
            //RangePlus();
        }

        // key S decrease displacement range
        if (Input.GetKeyDown(KeyCode.S))
        {
            //RangeMinus();
        }

        // quit application
        // (command will be ignored in editor)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuitApp();
        }

        // grayscale mode
        if (Input.GetKeyDown(KeyCode.H))
        {
            // set our color option
            colorOption = 0;
            Debug.Log("Color mode set to Grayscale");
        }

        // red-only mode
        if (Input.GetKeyDown(KeyCode.R))
        {
            // set our color option
            colorOption = 1;
            Debug.Log("Color mode set to Red");
        }

        // green-only mode
        if (Input.GetKeyDown(KeyCode.G))
        {
            // set our color option
            colorOption = 2;
            Debug.Log("Color mode set to Green");
        }

        // blue-only mode
        if (Input.GetKeyDown(KeyCode.B))
        {
            // set our color option
            colorOption = 3;
            Debug.Log("Color mode set to Blue");
        }

        // random RGB
        if (Input.GetKeyDown(KeyCode.E))
        {
            // set our color option
            colorOption = 4;
            Debug.Log("Color mode set to RGB");
        }

        // ---------- mouse input ----------

        if (SceneManager.GetActiveScene().name.Equals("Programming"))
        {
            if (Input.GetMouseButtonUp(0))
            {

                // check if we're clicking UI element or cube
                if (IsPointerOverUIObject())
                    return;
                else
                {
                    RaycastHit hitInfo = new RaycastHit();
                    bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);

                    if (hit)
                    {
                        Debug.Log(hitInfo.transform.gameObject.name + " clicked");

                        if (currentSelection != null)
                        {
                            currentSelection.GetComponent<PlatformDataNode>().ResetDataNode();
                        }

                        currentSelection = hitInfo.transform.gameObject;
                        currentSelection.GetComponent<PlatformDataNode>().SelectNode();

                        //OnPlatformManagerUpdateUI(hitInfo.transform.gameObject.name);
                    }
                    else
                    {
                        Debug.Log("no hit!");
                    }
                }
            }
        }

        // ---------- simulating the platform ----------

        //if (allCube != null)
        //{
        //    if (simulatingScene)
        //    {
        //        // traverse through allCube[,] in every frame
        //        for (int i = 0; i < allCube.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < allCube.GetLength(1); j++)
        //            {
        //                // if the cube almost reaching the lerp destination, then we'll get new y position for it to lerp to
        //                // (lerp won't necessarily reach the final position exactly (due to float value))
        //                // will also get a new color
        //                if ((allCube[i, j].transform.position.y - nextPos[i, j]) < 0.05f)
        //                {
        //                    // get random number and set next position
        //                    nextPos[i, j] = Random.Range(-displacementRange, displacementRange);

        //                    // set new color
        //                    nextColor[i, j] = SetNewColor();
        //                }

        //                // smooth transition position
        //                allCube[i, j].transform.position =
        //                    Vector3.Lerp(
        //                        allCube[i, j].transform.position, // current position
        //                        new Vector3(allCube[i, j].transform.position.x, nextPos[i, j], allCube[i, j].transform.position.z), // destination
        //                        Time.deltaTime // lerp time
        //                    );

        //                // smooth transition color
        //                allCube[i, j].transform.gameObject.GetComponent<Renderer>().material.color =
        //                    Color.Lerp(
        //                        allCube[i, j].transform.gameObject.GetComponent<Renderer>().material.color, // current color
        //                        nextColor[i, j], // changing the color to this
        //                        Time.deltaTime // lerp time
        //                    );
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // reset the cubes back to y = 0
        //        for (int i = 0; i < allCube.GetLength(0); i++)
        //        {
        //            for (int j = 0; j < allCube.GetLength(1); j++)
        //            {
        //                // smooth transitiion back to y = 0
        //                allCube[i, j].transform.position =
        //                    Vector3.Lerp(
        //                        allCube[i, j].transform.position, // current position
        //                        new Vector3(allCube[i, j].transform.position.x, 0, allCube[i, j].transform.position.z), // reset y to 0
        //                        Time.deltaTime // lerp time
        //                    );

        //                // smooth transition color back to white
        //                allCube[i, j].transform.gameObject.GetComponent<Renderer>().material.color =
        //                    Color.Lerp(
        //                        allCube[i, j].transform.gameObject.GetComponent<Renderer>().material.color, // current color
        //                        Color.white, // changing the color to this
        //                        Time.deltaTime // lerp time
        //                    );
        //            }
        //        }
        //    }
        //}
    }

    // to determine if we clicking over the UI element or not
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        //foreach (var result in results)
        //{
        //    Debug.Log(result.gameObject.name);
        //}

        return results.Count > 0;
    }

    // destroying cubes
    public void DestroyCube()
    {
        if (allCube != null)
        {
            for (int i = 0; i < allCube.GetLength(0); i++)
            {
                for (int j = 0; j < allCube.GetLength(1); j++)
                {
                    Destroy(allCube[i, j]);
                }
            }
        }
    }

    // initializing platform here
    public void BuildPlatform()
    {
        // destroy all the cubes first if it's already initialized
        DestroyCube();

        // initializing arrays' size to given m and n value
        allCube = new GameObject[configData.mSize, configData.nSize];

        //nextPos = new float[mSize, nSize];
        //nextColor = new Color[mSize, nSize];

        // creating all the cubes (platform)
        for (int i = 0; i < configData.mSize; i++)
        {
            for (int j = 0; j < configData.nSize; j++)
            {
                // create cube
                //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                //cube.transform.position = new Vector3(i + (i * cubeSpacing), 0f, j + (j * cubeSpacing)); // give position
                //cube.transform.rotation = Quaternion.identity; // no cube rotation
                //cube.transform.localScale = new Vector3(1f, scaleOnY, 1f); // set size of the cubes
                //cube.name = string.Format("Cube-{0}-{1}", i, j);

                // instantiate cube prefab
                GameObject cube = Instantiate(
                    cubePrefab, // original object
                    new Vector3(i + (i * configData.deltaSpace), 0f, j + (j * configData.deltaSpace)), // position
                    Quaternion.identity // rotation
                    );
                cube.name = string.Format("Node[{0},{1}]", i, j);

                allCube[i, j] = cube; // keep all initialized cubes into 2d GameObject array

                cube.AddComponent<PlatformDataNode>(); // adding PlatformDataNode script to the cube

                PlatformDataNode pdn = cube.transform.GetComponent<PlatformDataNode>();
                pdn.iPosition = i;
                pdn.jPosition = j;
                pdn.isProgrammed = programmingScene; // toggle programming flag on each cube
                pdn.isSimulated = simulatingScene; // toggle simulating flag on each cube


                //nextPos[i, j] = cube.transform.position.y;
                //nextColor[i, j] = Color.white; // set all cube to white
                //cube.GetComponent<Renderer>().material = cubeMaterial; // set the material for the cube
            }
        }

        // updating UI after building platform
        if (OnPlatformManagerChanged != null)
        {
            // if subscribed, update data and UI
            OnPlatformManagerChanged(configData);
        }
    }

    // quit application
    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Exiting application!!");
    }

    // ----- getter method -----

    //public int GetMSize()
    //{
    //    return m;
    //}

    //public int GetNSize()
    //{
    //    return n;
    //}

    //public float GetDisplacementRange()
    //{
    //    return displacementRange;
    //}

    //public float GetSpacing()
    //{
    //    return prevSpacing;
    //}

    public int GetColorOption()
    {
        return colorOption;
    }

    public bool GetSimStatus()
    {
        return simulatingScene;
    }

    // ----- setter method -----

    // increase y-axis displacement range
    //public void RangePlus()
    //{
    //    displacementRange += 0.1f;
    //    displacementRange = Mathf.Round(displacementRange * 10f) / 10f; // set value to just 1 decimal point
    //    if (displacementRange > 1f) displacementRange = 1f; // set upper limit to 1
    //    Debug.Log("new displacementRange = " + displacementRange);
    //}

    // decrease y-axis displacement range
    //public void RangeMinus()
    //{
    //    displacementRange -= 0.1f;
    //    displacementRange = Mathf.Round(displacementRange * 10f) / 10f; // set value to just 1 decimal point
    //    if (displacementRange < 0f) displacementRange = 0f; // reset to 0 if it goes below
    //    Debug.Log("new displacementRange = " + displacementRange);
    //}

    // set colorOption value from the dropdown
    public void SetColorOption(int selected)
    {
        colorOption = selected;
    }

    // set new cube color depending on colorOption
    private Color SetNewColor()
    {
        Color randomNewColor = new Color();

        switch (colorOption)
        {
            case 0:
                randomNewColor = Random.ColorHSV(0f, 0f, 0f, 0f, 0f, 1f); // grayscale
                break;
            case 1:
                randomNewColor = new Color(Random.Range(0f, 1.0f), 0, 0); // red spectrum
                break;
            case 2:
                randomNewColor = new Color(0, Random.Range(0f, 1.0f), 0); // green spectrum
                break;
            case 3:
                randomNewColor = new Color(0, 0, Random.Range(0f, 1.0f)); // blue spectrum
                break;
            case 4:
                randomNewColor = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f)); // all RGB
                break;
            default:
                break;
        }

        return randomNewColor;
    }

    // switch simulation on/off
    public void SetSimulation()
    {
        simulatingScene = !simulatingScene;

        // button testing purposes
        if (simulatingScene) Debug.Log("sim start");
        else Debug.Log("sim stop");
    }

}
