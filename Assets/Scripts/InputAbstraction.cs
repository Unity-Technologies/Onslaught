using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class InputAbstraction
{
    public enum Handedness
    {
        LEFT,
        RIGHT
    }

    public enum ButtonAlias
    {
        AXIS_CLICK,
        AXIS_TOUCH
    }

    public enum AxisAlias
    {
        X,
        Y
    }

    private static float m_FireTriggerThreshold = 0.75f;

    public static bool FireControlActive(Handedness hand)
    {
        if (XRSettings.loadedDeviceName == "daydream")
        {
            if (hand == Handedness.LEFT)
                return Input.GetButton("VR_PrimaryButton_Left");
            else // right hand
                return Input.GetButton("VR_PrimaryButton_Right");
        }
        else
        {
            if (hand == Handedness.LEFT)
                return Input.GetAxis("VR_TriggerAxis_Left") > m_FireTriggerThreshold;
            else // right hand
                return Input.GetAxis("VR_TriggerAxis_Right") > m_FireTriggerThreshold;
        }
    }

    public static string AliasToControlString(ButtonAlias Alias, Handedness hand)
    {
        if (hand == Handedness.LEFT)
        {
            switch (Alias)
            {
                case ButtonAlias.AXIS_CLICK:
                    return "VR_Primary2DAxisClickButton_Left";
                case ButtonAlias.AXIS_TOUCH:
                    return "VR_Primary2DAxisTouchButton_Left";
                default:
                    return "";
            }
        }
        else // RIGHT HAND
        {
            switch (Alias)
            {
                case ButtonAlias.AXIS_CLICK:
                    return "VR_Primary2DAxisClickButton_Right";
                case ButtonAlias.AXIS_TOUCH:
                    return "VR_Primary2DAxisTouchButton_Right";
                default:
                    return "";
            }
        }
    }

    public static string AliasToControlStringSDKSpecific(AxisAlias Alias, Handedness Hand)
    {
        // For daydream, you hold the controller sideways.
        if (XRSettings.loadedDeviceName == "daydream")
        {
            if (Hand == Handedness.LEFT)
            {
                switch (Alias)
                {
                    case AxisAlias.X:
                        return "VR_Primary2DAxis_Y_Left";
                    case AxisAlias.Y:
                        return "VR_Primary2DAxis_X_Left";
                    default:
                        return "";
                }
            }
            else // RIGHT HAND
            {
                switch (Alias)
                {
                    case AxisAlias.X:
                        return "VR_Primary2DAxis_InvY_Right";
                    case AxisAlias.Y:
                        return "VR_Primary2DAxis_InvX_Right";
                    default:
                        return "";
                }
            }
        }
        else   // Non-daydream platforms
        {
            return AliasToControlString(Alias, Hand);
        }
    }

    public static string AliasToControlString(AxisAlias Alias, Handedness Hand)
    {
        if (Hand == Handedness.LEFT)
        {
            switch (Alias)
            {
                case AxisAlias.X:
                    return "VR_Primary2DAxis_X_Left";
                case AxisAlias.Y:
                    return "VR_Primary2DAxis_InvY_Left";
                default:
                    return "";
            }
        }
        else     // RIGHT HAND
        {
            switch (Alias)
            {
                case AxisAlias.X:
                    return "VR_Primary2DAxis_X_Right";
                case AxisAlias.Y:
                    return "VR_Primary2DAxis_InvY_Right";
                default:
                    return "";
            }
        }
    }

    public static bool GetButton(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButton(AliasToControlString(alias, hand));
    }

    public static bool GetButtonDown(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButtonDown(AliasToControlString(alias, hand));
    }

    public static bool GetButtonUp(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButtonUp(AliasToControlString(alias, hand));
    }

    public static float GetAxis(AxisAlias alias, Handedness hand, bool ignoreSDKSpecific = false)
    {
        if (ignoreSDKSpecific)
            return Input.GetAxis(AliasToControlString(alias, hand));
        else
            return Input.GetAxis(AliasToControlStringSDKSpecific(alias, hand));
    }

    public static Handedness PreferedHand()
    {
        List<XRNodeState> nodeStates = new List<XRNodeState>();
        InputTracking.GetNodeStates(nodeStates);

        bool hasLeft = false;
        bool hasRight = false;
        foreach (XRNodeState nodeState in nodeStates)
        {
            if (nodeState.nodeType == XRNode.LeftHand)
                hasLeft = true;
            if (nodeState.nodeType == XRNode.RightHand)
                hasRight = true;
        }

        if (hasLeft && !hasRight)
            return Handedness.LEFT;
        else if (!hasLeft && hasRight)
            return Handedness.RIGHT;
        else
            return Handedness.RIGHT;
    }
}
