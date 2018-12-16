using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlatformManager : PlatformGenericSinglton<PlatformManager> {

    // ----- variables -----

    public GameObject cubePrefab; // cube prefab
    public GameObject[,] allCube; // array holding all the cubes

    private float[,] programmedHeight; // hold programmed height of all the cubes

    // flag to determine current scene (config, programming or simulating)
    private bool simulatingScene = false;
    private bool programmingScene = false;

    // pressing 'T' key to pause/play simulation
    private bool isSimPaused = false;

    /**
     *  which color option the sim currently is on
     *  0 = grayscale, 1 = red, 2 = green, 3 = blue, 4 = all RGB
     *  //color is set to grayscale once the simulation is started
     *  unset value is -1
     */
    private int colorOption = -1;

    public Material cubeMaterial;

    public PlatformConfigurationData configData;

    public GameObject currentSelection; // holds current cube that got clicked

    // ----- delegates and events -----

    public delegate void PlatformManagerChanged(PlatformConfigurationData pcd);
    public static event PlatformManagerChanged OnPlatformManagerChanged;

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
        // set up PCD configData
        CreatePCDInstance();

        // copy pcd data to configData
        InitializePCDInstance(pcd);

        BuildPlatform();
    }

    // set up PCD configData
    private void CreatePCDInstance()
    {
        // erase previous configData if available
        if (configData != null)
        {
            Debug.Log("configData != null. Destroying and create new..");

            // create new configData
            PlatformConfigurationData newConfig = gameObject.AddComponent<PlatformConfigurationData>();

            // destroying the old ones
            Destroy(configData);

            // set up new ones
            configData = newConfig;
        }
        // if not available, rebuild and reconfigure configData
        else
        {
            Debug.Log("configData == null. Creating new..");
            configData = gameObject.AddComponent<PlatformConfigurationData>();
        }
    }

    // initialize PCD configData values
    private void InitializePCDInstance(PlatformConfigurationData pcd)
    {
        // copy pcd data to configData
        configData.mSize = pcd.mSize;
        configData.nSize = pcd.nSize;
        configData.deltaSpace = pcd.deltaSpace;
        configData.height = pcd.height;
        configData.color = pcd.color;
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
    public void ReadFile()
    {
        using (StreamReader sr = new StreamReader(Path.Combine(Application.dataPath, "WriteLines.txt")))
        {
            bool isFirstLine = true;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    string[] firstLine = line.Split(',');

                    // creating new PCD and feed it the data from firstLine string
                    PlatformConfigurationData newPCD = gameObject.AddComponent<PlatformConfigurationData>();

                    int.TryParse(firstLine[0], out newPCD.mSize);
                    int.TryParse(firstLine[1], out newPCD.nSize);
                    float.TryParse(firstLine[2], out newPCD.deltaSpace);
                    float.TryParse(firstLine[3], out newPCD.height);
                    int.TryParse(firstLine[4], out newPCD.color);

                    isFirstLine = false; // set bool to false to access below else part

                    // set up configData and its value using data from firstLine (newPCD)
                    CreatePCDInstance();
                    InitializePCDInstance(newPCD);

                    // initialize programmedHeight array
                    programmedHeight = new float[newPCD.mSize, newPCD.nSize];

                    // updating UI top panel
                    OnPlatformManagerChanged(configData);

                    Destroy(newPCD); // deleting newPCD to avoid duplicates in Singleton
                }
                else
                {
                    string[] currentLine = line.Split(',');
                    programmedHeight[int.Parse(currentLine[0]), int.Parse(currentLine[1])] = float.Parse(currentLine[2]);
                }
            }
        }

        // build platform aftre we've read it all
        BuildPlatform();
    }

    // triggered when the scene got loaded
    // parameter: arg0.name will give the current scene's name
    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        // when we creating platform from scratch
        if (allCube != null)
        {
            // set all flag to false at Main Menu
            if (arg0.name.Equals("MainMenu"))
            {
                StopAllCoroutines(); // safe check to stop Simulation and preventing missing object error
                programmingScene = false;
                simulatingScene = false;
            }

            if (arg0.name.Equals("Programming"))
            {
                programmingScene = true; // set programming flag to true
                programmedHeight = null; // reset array
            }
            else { programmingScene = false; }

            // will trigger if we have a save file already
            if (arg0.name.Equals("Simulate"))
            {
                allCube = null; // resetting array
                simulatingScene = true; // set simulating flag to true 
                ReadFile(); // build platform in here instead, so no need to go to below if (!MainMenu)

                // updating camera position after building platform
                Camera.main.gameObject.GetComponent<PlatformCameraControl>().UpdateCameraPosition(configData);

                // updating color option from PCD
                colorOption = configData.color;

                // start simulation
                isSimPaused = false; // reset pause flag
                StartCoroutine(StartSimulation());

                // so we don't trigger any other if case
                return;
            }
            else { simulatingScene = false; }

            // build (and rebuild) the platform if we're not on Main Menu scene
            if (!arg0.name.Equals("MainMenu"))
            {
                StopAllCoroutines(); // safe check to stop Simulation and preventing missing object error

                Debug.Log("Changing to arg0 (" + arg0.name + ") scenes, rebuilding platform..");
                BuildPlatform();

                // updating camera position after building platform
                Camera.main.gameObject.GetComponent<PlatformCameraControl>().UpdateCameraPosition(configData);
            }

        }
        else // allCube[,] is empty when we go straight into Simulate scene
        {
            if (arg0.name.Equals("Simulate"))
            {
                simulatingScene = true; // set simulating flag to true (others defaulted to false)
                ReadFile();

                // updating camera position after building platform
                Camera.main.gameObject.GetComponent<PlatformCameraControl>().UpdateCameraPosition(configData);

                // updating color option from PCD
                colorOption = configData.color;

                // start simulation
                StartCoroutine(StartSimulation());
            }
        }
    }

    // simulate the programmed platform
    private IEnumerator StartSimulation()
    {
        if (programmedHeight != null && allCube != null)
        {
            if (simulatingScene)
            {
                int row = 0;
                // row++ because we want the whole thing to move forward
                while (row < configData.mSize)
                {
                    // pausing simulation if T is pressed
                    while (isSimPaused) { yield return null; }

                    // start working (workingRow) from row and backward to 0
                    // start reading (readingRow) programmedHeight[,] from last row, and only reading for this much (row+1) rows
                    int workingRow = row, readingRow = configData.mSize - 1;
                    while (workingRow != -1 && readingRow >= configData.mSize - 1 - row)
                    {
                        // lerping every column while we at workingRow
                        int col = 0;
                        while (col < configData.nSize)
                        {
                            allCube[workingRow, col].GetComponent<PlatformDataNode>().
                                SetProgrammedHeightAndColor(
                                    programmedHeight[readingRow, col],
                                    SetNewColor()
                                );
                            col++;
                        }
                        workingRow--;
                        readingRow--;
                    }

                    row++;

                    // wait before moving the whole thing
                    // (if statement so that we don't need to wait extra to transition to the looping part)
                    if (row < configData.mSize) yield return new WaitForSeconds(0.8f);
                }
                
                // looping the simulation back to front after we finished the first round run (and will continue in this state)

                int looping = 0; // total row looping
                while (simulatingScene)
                {
                    // pausing simulation if T is pressed
                    while (isSimPaused) { yield return null; }

                    //Debug.Log("start loop 2");
                    int readingRow = configData.mSize - 1 - looping; // start reading top chunk that's not looped yet
                    int readingLoop = configData.mSize - 1; // all the lower chunk is the one that's being looped

                    // working from lowest row to finish moving the unlooped chunk 
                    for (int allrow = configData.mSize - 1; allrow > looping - 1; allrow--)
                    {
                        int col = 0;
                        while (col < configData.nSize)
                        {
                            allCube[allrow, col].GetComponent<PlatformDataNode>().
                                SetProgrammedHeightAndColor(
                                    programmedHeight[readingRow, col],
                                    SetNewColor()
                                );
                            col++;
                        }
                        //Debug.Log("readingRow = " + readingRow + ", allrow = " + allrow);
                        readingRow--;
                    }
                    // and rest of the row for looped chunk
                    for (int rowsleft = looping - 1; rowsleft > -1; rowsleft--)
                    {
                        int col = 0;
                        while (col < configData.nSize)
                        {
                            allCube[rowsleft, col].GetComponent<PlatformDataNode>().
                                SetProgrammedHeightAndColor(
                                    programmedHeight[readingLoop, col],
                                    SetNewColor()
                                );
                            col++;
                        }
                        //Debug.Log("readingLoop = " + readingLoop + ", rowsleft = " + rowsleft);
                        readingLoop--;
                    }

                    // wait before moving the whole thing 
                    yield return new WaitForSeconds(0.8f);
                    looping++;

                    // resetting all counter
                    if (looping == configData.mSize) looping = 0;
                }

            }
        }
    }

	// Update is called once per frame
	void Update () {

        // ---------- keyboard input ----------

        // play and pause simulation (only in simulation scene)
        if (Input.GetKeyDown(KeyCode.T) && simulatingScene)
        {
            SetSimulation();
        }

        // quit application
        // (command will be ignored in editor)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuitApp();
        }

        // grayscale mode
        if (Input.GetKeyDown(KeyCode.H) && simulatingScene)
        {
            // set our color option
            colorOption = 0;
            Debug.Log("Color mode set to Grayscale");
        }

        // red-only mode
        if (Input.GetKeyDown(KeyCode.R) && simulatingScene)
        {
            // set our color option
            colorOption = 1;
            Debug.Log("Color mode set to Red");
        }

        // green-only mode
        if (Input.GetKeyDown(KeyCode.G) && simulatingScene)
        {
            // set our color option
            colorOption = 2;
            Debug.Log("Color mode set to Green");
        }

        // blue-only mode
        if (Input.GetKeyDown(KeyCode.B) && simulatingScene)
        {
            // set our color option
            colorOption = 3;
            Debug.Log("Color mode set to Blue");
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
                    }
                    else
                    {
                        Debug.Log("no hit!");
                    }
                }
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

        //foreach (var result in results) { Debug.Log(result.gameObject.name); }

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

        // creating all the cubes (platform)
        for (int i = 0; i < configData.mSize; i++)
        {
            for (int j = 0; j < configData.nSize; j++)
            {
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

    public int GetColorOption()
    {
        return colorOption;
    }

    public bool GetSimStatus()
    {
        return simulatingScene;
    }

    // ----- setter method -----

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
                randomNewColor = new Color(0.5f, 0.5f, 0.5f); // grayscale
                //randomNewColor = Random.ColorHSV(0f, 0f, 0f, 0f, 0f, 1f); // grayscale
                break;
            case 1:
                randomNewColor = new Color(1.0f, 0, 0); // red spectrum
                //randomNewColor = new Color(Random.Range(0f, 1.0f), 0, 0); // red spectrum
                break;
            case 2:
                randomNewColor = new Color(0, 1.0f, 0); // green spectrum
                //randomNewColor = new Color(0, Random.Range(0f, 1.0f), 0); // green spectrum
                break;
            case 3:
                randomNewColor = new Color(0, 0, 1.0f); // blue spectrum
                //randomNewColor = new Color(0, 0, Random.Range(0f, 1.0f)); // blue spectrum
                break;
            default:
                break;
        }

        return randomNewColor;
    }

    // switch simulation paused/play
    public void SetSimulation()
    {
        isSimPaused = !isSimPaused;

        // button testing purposes
        if (isSimPaused) Debug.Log("sim paused");
        else Debug.Log("sim resumed");
    }

}
