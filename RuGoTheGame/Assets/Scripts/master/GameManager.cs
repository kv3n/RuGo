﻿using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public GadgetManipulator Manipulator;
    public GadgetSelectorMenu GadgetSelectorMenu;
    public MainMenu MainMenu;
    public LoadMenu loadMenu;

    public PathTool PathTool;
    public Text GameModeDisplay;


    // Testing the VR Menu
    public int CurrentMenuOption = -1;

    private enum GameMode { Build, Select, Draw };
    private GameMode currentGameMode;
    private bool BuildModeEnabled
    {
        get
        {
            return currentGameMode == GameMode.Build;
        }
    }

    void Start()
    {
        Debug.Log("Game Manager Started");
        // Reparent the menu before we do anything else in the game
        GadgetSelectorMenu.ReparentSelectorMenu();
        EnableBuildMode();
    }

    void Update()
    {
        if (BuildModeEnabled)
        {
            if (RuGoInteraction.Instance.IsConfirmPressed &&
                Manipulator.ModifyModeEnabled() &&
                !Manipulator.GadgetSelected()
               )
            {
                SelectExistingGadget();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                this.ResetGadgetsInWorld();
            }
            if (Input.GetKeyDown(KeyCode.O)) {
                Manipulator.Save(World.DEFAULT_SAVE_FILE);
            }
            if (Input.GetKeyDown(KeyCode.P)) {
                Manipulator.Load();
            }
        }

        if (RuGoInteraction.Instance.IsMenuActionPressed)
        {
            if (GadgetSelectorMenu.isActiveAndEnabled)
            {
                EnableBuildMode();
            }
            else
            {
                EnableSelectMode();
            }
        }
    }

    /************************** Public Functions **************************/
    public void ResetGadgetsInWorld()
    {
        Manipulator.ResetGadgetsInWorld();
    }
    /// <summary>
    /// used to save the world from menu as a replacement from OnPress P
    /// </summary>
    public void GameManagerSaves()
    {
       Manipulator.Save(World.DEFAULT_SAVE_FILE);
       string[] words = System.DateTime.UtcNow.ToString().Replace(":", " ").Replace("/", " ").Split(' ');
       string fname = "";
       foreach (string word in words)
       {
           fname += word;
       }
       var name = System.DateTime.UtcNow.ToString();
       Manipulator.Save("SavedGames/" + fname + ".dat");
    }

    public void GameManagerLoad()
    {
      Manipulator.Load();

    }
    public void GameManagerClearWorld()
    {
        Manipulator.ClearWorld();

    }

    public void CreatePath() {
        this.EnableDrawMode();
    }

    /// <summary>
    /// name of the prefab file.
    /// </summary>
    /// <param name="prefabResourceName">The name of the resource to load from Prefab Directory</param>
    public void CreateGadget(string prefabResourceName)
    {
        this.EnableBuildMode();
       
        GameObject gadgetPrefab = Resources.Load(prefabResourceName) as GameObject;
        GameObject gadgetGameObject = Instantiate(gadgetPrefab, this.transform);
        Gadget gadget = gadgetGameObject.GetComponent<Gadget>();

        Manipulator.EnableCreateMode(gadget);
    }

    public void CreateGadgetAlongPath(Vector3[] path) {
        GameObject dominoPreb = Resources.Load("Domino") as GameObject;
        int numOfDominos = path.Length;

        if (numOfDominos > 1)
        {
            for (int i = 0; i < numOfDominos - 1; i++)
            {
                GameObject gadgetGameObject = Instantiate(dominoPreb, this.transform);
                Gadget domino = gadgetGameObject.GetComponent<Gadget>();

                Vector3 pathDirection = path[i + 1] - path[i];
                domino.transform.position = path[i];
                domino.transform.rotation = Quaternion.LookRotation(pathDirection);
                domino.Deselect();
                Manipulator.InsertGadgetIntoWorld(domino);
            }
            GameObject lastGadgetGameObject = Instantiate(dominoPreb, this.transform);
            Gadget lastDomino = lastGadgetGameObject.GetComponent<Gadget>();

            Vector3 lastPathDirection = path[numOfDominos - 1] - path[path.Length - 2];
            lastDomino.transform.position = path[numOfDominos - 1];
            lastDomino.transform.rotation = Quaternion.LookRotation(lastPathDirection);
            lastDomino.Deselect();
            Manipulator.InsertGadgetIntoWorld(lastDomino);
        }
        else if (path.Length == 1)
        {
            GameObject gadgetGameObject = Instantiate(dominoPreb, this.transform);
            Gadget domino = gadgetGameObject.GetComponent<Gadget>();

            domino.transform.position = path[0];
            domino.Deselect();
            Manipulator.InsertGadgetIntoWorld(domino);
        }

        EnableBuildMode();
    }

    public void EnableDrawMode() {
        GadgetSelectorMenu.Deactivate();
        PathTool.Activate(CreateGadgetAlongPath);
        this.currentGameMode = GameMode.Draw;
        GameModeDisplay.text = "Mode: Draw Path";

        // Player Enable Look PC_ONLY
        SetPlayerLook(true);
    }

    public void EnableBuildMode()
    {
        GadgetSelectorMenu.Deactivate();
        Manipulator.Activate();
        this.currentGameMode = GameMode.Build;
        GameModeDisplay.text = "Mode: Build";

        // Player Enable Look PC_ONLY
        SetPlayerLook(true);
    }

    public void EnableSelectMode()
    {
        Manipulator.Deactivate();
        PathTool.Deactivate();
        GadgetSelectorMenu.Activate();
        this.currentGameMode = GameMode.Select;
        GameModeDisplay.text = "Mode: Select";

        // Player Enable Look PC_ONLY
        SetPlayerLook(false);
    }

    //TODO change this to something better
    public void goFromTo()
    {
        GadgetSelectorMenu.Deactivate();
        MainMenu.Activate();
    }

    public void goToLoadMenu()
    {

        loadMenu.Activate();
    }


    /************************** Private Functions **************************/

    private void SelectExistingGadget()     {         Ray ray = RuGoInteraction.Instance.SelectorRay;         RaycastHit hit;          if (Physics.Raycast(ray, out hit))         {             Gadget gadget = hit.transform.GetComponent<Gadget>();              if (gadget)             {                 Manipulator.EnableModifyMode(gadget);             }             else             {                 gadget = hit.transform.GetComponentInParent<Gadget>();                 if (gadget)                 {
                     Manipulator.EnableModifyMode(gadget);                 }
                else
                {

                    Debug.Log(hit.transform.root);

                }

            }         }     }

    private void SetPlayerLook(bool enabled)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            FirstPersonMove playerMoveScript = player.GetComponent<FirstPersonMove>();
            playerMoveScript.EnableLook = enabled;
        }        
    }

}
