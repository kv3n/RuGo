﻿using System;
using UnityEngine;

public class GadgetManipulator : MonoBehaviour
{
    public World World;
    public float TranslationDelta = 0.08f;

    private enum Mode { Modify, Create };
    private Gadget mSelectedGadget;
    private Mode mCurrentMode;
    private int mRayCastMask;

    public float turnSpeed = 50f;

    void Start()
    {
        mSelectedGadget = null;
        mCurrentMode = Mode.Modify;
        mRayCastMask = ~(1 << LayerMask.NameToLayer("SelectedGadget"));
    }

    void Update()
    {
        // Receive inputs only when a gadget is selected
        if (GadgetSelected())
        {
            // TODO: this is causing issue now that we have VRTK.
            // Two functions are trying to perform grabbing the same item.
            /*
            Ray ray = RuGoInteraction.Instance.SelectorRay;
            RaycastHit hit;

                https://docs.unity3d.com/Manual/Layers.html
                We want to ignore the selected gadget otherwise the raycast will keep intersecting repeatedly with itself translating the object in undesirable ways

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mRayCastMask))
            {
                mSelectedGadget.transform.position = hit.point;
            }*/

            if (Input.GetKey(KeyCode.Z))
            {
                mSelectedGadget.transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

            }
            if (Input.GetKey(KeyCode.C))
            {
                mSelectedGadget.transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
            }

            if (RuGoInteraction.Instance.IsTouchpadTouched)
            {

                print(RuGoInteraction.Instance.GetRotationDelta());
                mSelectedGadget.transform.Rotate(Vector3.up, RuGoInteraction.Instance.GetRotationDelta());
            }

            // TODO: this is causing issue now that we have VRTK.
            // Two functions are trying to perform grabbing the same item.
            /*
            if (Input.GetKeyDown(KeyCode.Return) || RuGoInteraction.Instance.IsConfirmPressed)
            {
                if (ModifyModeEnabled())
                {
                    PlaceGadget();
                }
                else
                {
                    World.CreateGadgetFromTemplate(mSelectedGadget);
                }
            }
            */

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (ModifyModeEnabled())
                {
                    RemoveGadget();
                }
            }
        }
    }

    private void PlaceGadget()
    {
        mSelectedGadget.Deselect();
        mSelectedGadget = null;
        World.MarkWorldModified();
    }

    private void RemoveGadget()
    {
        World.RemoveGadget(mSelectedGadget);
        mSelectedGadget = null;
    }

    /************************** Public Functions **************************/
    public bool ModifyModeEnabled()
    {
        return mCurrentMode == Mode.Modify;
    }

    public bool CreateModeEnabled()
    {
        return mCurrentMode == Mode.Create;
    }

    public void EnableModifyMode(Gadget gadget)
    {
        mSelectedGadget = gadget;
        mCurrentMode = Mode.Modify;
        //mSelectedGadget.MakeTransparent();
    }

    public void EnableCreateMode(Gadget gadget)
    {
        mSelectedGadget = gadget;
        mCurrentMode = Mode.Create;
        //mSelectedGadget.MakeTransparent();
    }

    public bool GadgetSelected()
    {
        return mSelectedGadget != null;
    }

    public void Activate()
    {
        this.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        this.Reset();
        this.gameObject.SetActive(false);
    }

    public void CreateNewWorld()
    {
        World.CreateNewWorld();
    }

    public void Save() 
    {
        World.Save();
    }

    // Function: Reset
    // Input: none
    // Output: none
    // Description:
    //  - Reset manipulator to its initial state. Depending on which mode the
    //    game is currently in, the function will perform different actions.
    //
    //    1. Modify mode:
    //       - Simply deselect the current selected gadget.
    //       - Set selectedGadget to null.
    //    2. Create mode:
    //       - Destroy the gadget template.
    //       - Set mode back to modify mode.
    //       - Set selectedGadget to null.
    public void Reset()
    {
        Debug.Log("GadgetManipulator is being reset.");

        if (GadgetSelected())
        {
            if (ModifyModeEnabled())
            {
                mSelectedGadget.Deselect();
            }
            else
            {
                Destroy(mSelectedGadget.gameObject);
                mCurrentMode = Mode.Modify;
            }
            mSelectedGadget = null;
        }
    }

    public void InsertGadgetIntoWorld(Gadget gadget) {
        World.InsertGadget(gadget);
    }

    public void ResetGadgetsInWorld()
    {
        World.LoadAuto();
    }

    public void ClearWorld()
    {
        World.Clear();
    }

 
}