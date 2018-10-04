﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RuGoInteraction : MonoBehaviour {
    /*
     * TASKS
     * ** RAY       - Input space to world space ray
     * ** CONFIRM   - An action button triggered on all input
     * ** BACK      - A button mapped for Back action / Reset action
     * ** SCROLL    - 
    */

    public static RuGoInteraction Instance = null;
    private void MakeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private SteamVR_ControllerManager ControllerManager;
    private SteamVR_TrackedObject LeftTrackedObject;
    private SteamVR_TrackedObject RightTrackedObject;
    private SteamVR_LaserPointer LaserPointer;

    private SteamVR_Controller.Device LeftController
    {
        get
        {
            return SteamVR_Controller.Input((int)LeftTrackedObject.index);
        }
    }

    private SteamVR_Controller.Device RightController
    {
        get
        {
            return SteamVR_Controller.Input((int)RightTrackedObject.index);
        }
    }

    private void CacheControllers()
    {
        ControllerManager = GetComponent<SteamVR_ControllerManager>();

        if(ControllerManager != null)
        {
            LeftTrackedObject = ControllerManager.left.GetComponent<SteamVR_TrackedObject>();
            RightTrackedObject = ControllerManager.right.GetComponent<SteamVR_TrackedObject>();

            LaserPointer = ControllerManager.right.GetComponent<SteamVR_LaserPointer>();
        }
    }

    void Awake()
    {
        MakeSingleton();
    }

    void Start ()
    {
        // We do it here because we want all the VR initializations to be done first
        CacheControllers();
	}

    private void EnableDebugging()
    {
        if (!DebugCapsule.gameObject.activeSelf)
        {
            DebugCapsule.gameObject.SetActive(true);
        }

        if (!DebugCylinder.gameObject.activeSelf)
        {
            DebugCylinder.gameObject.SetActive(true);
        }
    }

    private void DisableDebugging()
    {
        if (DebugCapsule.gameObject.activeSelf)
        {
            DebugCapsule.gameObject.SetActive(false);
        }

        if (DebugCylinder.gameObject.activeSelf)
        {
            DebugCylinder.gameObject.SetActive(false);
        }
    }
	
	void Update ()
    {
        if(DebugInputs)
        {
            EnableDebugging();

            Ray selectorRay = SelectorRay;
            Debug.DrawRay(selectorRay.origin, selectorRay.direction, Color.red, 0.0f, false);
            DebugCapsule.position = selectorRay.origin;
            DebugCapsule.rotation = Quaternion.LookRotation(selectorRay.direction, Vector3.up);

            if (IsConfirmPressed)
            {
                RaycastHit hit;
                if (Physics.Raycast(selectorRay, out hit))
                {
                    DebugCylinder.position = hit.point;
                }
            }
        }
        else
        {
            DisableDebugging();
        }
	}


    // ACTIONS
    public bool DebugInputs = false;
    public Transform DebugCapsule;
    public Transform DebugCylinder;

    private bool IsSelectorControllerActive
    {
        get
        {
            return ControllerManager != null && ControllerManager.right.activeSelf;
        }
    }

    private bool IsManipulatorControllerActive
    {
        get
        {
            return ControllerManager != null && ControllerManager.left.activeSelf;
        }
    }

    private Transform SelectorController
    {
        get
        {
            if (!IsSelectorControllerActive)
                return null;

            return ControllerManager.right.transform;
        }
    }

    private Transform GetManipulatorController
    {
        get
        {
            if (!IsManipulatorControllerActive)
                return null;

            return ControllerManager.left.transform;
        }
    }

    public Ray SelectorRay
    {
        get
        {
            Ray selectorRay = new Ray();

            if (IsSelectorControllerActive)
            {
                selectorRay.origin = SelectorController.localPosition;
                selectorRay.direction = SelectorController.forward;
            }
            else
            {
                if(Camera.main != null)
                {
                    selectorRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                }
            }

            return selectorRay;
        }
    }

    public bool IsConfirmPressed
    {
        get
        {
            if (IsSelectorControllerActive)
            {
                return RightController.GetHairTriggerDown() && (!IsManipulatorControllerActive || !LeftController.GetHairTrigger());
            }
            else
            {
                return Input.GetMouseButtonDown(0);
            }
        }
    }

    public bool IsConfirmHeld
    {
        get
        {
            if (IsSelectorControllerActive)
            {
                return RightController.GetHairTrigger() && (!IsManipulatorControllerActive || !LeftController.GetHairTrigger());
            }
            else
            {
                return Input.GetMouseButton(0);
            }
        }
    }

    public bool IsConfirmReleased
    {
        get
        {
            if (IsSelectorControllerActive)
            {
                return RightController.GetHairTriggerUp() && (!IsManipulatorControllerActive || !LeftController.GetHairTrigger());
            }
            else
            {
                return Input.GetMouseButtonUp(0);
            }
        }
    }

    public bool IsDoubleTriggerDown
    {
        get
        {
            if(IsSelectorControllerActive && IsManipulatorControllerActive)
            {
                return RightController.GetHairTrigger() && LeftController.GetHairTrigger();
            }
            else
            {
                // If PC always return False
                return false;
            }
        }
    }

     
    public Vector3 ControllerToControllerDirection
    {
        get
        {
            Vector3 direction = ControllerManager.right.transform.position - ControllerManager.left.transform.position;
            if(direction.sqrMagnitude > 0)
            {
                direction.Normalize();
            }

            return direction;
        }
    }

    public bool IsMenuActionPressed
    {
        get
        {
            if(IsSelectorControllerActive)
            {
                return RightController.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad);
            }
            else
            {
                // FOR PC We want to return false for now until controllers are unified
                //return Input.GetKeyDown(KeyCode.Return);
                return false;
            }
        }
    }

    public bool IsMenuConfirmPressed
    {
        get
        {
            if (IsSelectorControllerActive)
            {
                return RightController.GetPressDown(EVRButtonId.k_EButton_Grip);
            }
            else
            {
                // FOR PC We want to return false for now until controllers are unified
                //return Input.GetKeyDown(KeyCode.Return);
                return false;
            }
        }
    }
}
