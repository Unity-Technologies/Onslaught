using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class InputAbstraction : MonoBehaviour
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

    private float m_FireTriggerThreshold = 0.75f;

    public delegate void InputBoolEventHandler();
    public delegate void InputVector2EventHandler(Vector2 value);
    public event InputBoolEventHandler FireActive;
    public event InputBoolEventHandler FireInactive;
    public event InputBoolEventHandler NavActive;
    public event InputVector2EventHandler AxisChanged;
    public event InputVector2EventHandler AxisChangedIgnoreSDKLeft;
    public event InputVector2EventHandler AxisChangedIgnoreSDKRight;

    struct InputState
    {
        public Vector2 xy;
        public Vector2 xyIgnoreSDKLeftHand;
        public Vector2 xyIgnoreSDKRightHand;
        public bool fire;
        public bool nav;
    }

    private InputState previousInputState;
    private InputState newARInputState;
    private bool isAR = false;
    private bool newARFrame = false;

    void Update()
    {
        InputState newInputState = new InputState();

        if (!isAR) {
            newInputState.fire = FireControlActive(PreferedHand());
            newInputState.nav = GetButtonDown(ButtonAlias.AXIS_CLICK, PreferedHand());
            newInputState.xy = new Vector2(GetAxis(AxisAlias.X, PreferedHand()), GetAxis(AxisAlias.Y, PreferedHand()));
            newInputState.xyIgnoreSDKLeftHand = new Vector2(GetAxis(AxisAlias.X, Handedness.LEFT, true), GetAxis(AxisAlias.Y, Handedness.LEFT, true));
            newInputState.xyIgnoreSDKRightHand = new Vector2(GetAxis(AxisAlias.X, Handedness.RIGHT, true), GetAxis(AxisAlias.Y, Handedness.RIGHT, true));
        }
        else
        {
            newInputState = newARInputState;

            if (newARFrame)// isAR
            {
                if (newARInputState.nav)
                    newARInputState.nav = false; // nav should persist for one frame, then be unset
                else
                    newARFrame = false;
            }
        }

        if (newInputState.fire != previousInputState.fire && newInputState.fire)
            FireActive?.Invoke();
        if (newInputState.fire != previousInputState.fire && !newInputState.fire)
            FireInactive?.Invoke();
        if (newInputState.nav != previousInputState.nav && newInputState.nav)
            NavActive?.Invoke();
        if (newInputState.xy != previousInputState.xy)
        {
            AxisChanged?.Invoke(newInputState.xy);
            AxisChangedIgnoreSDKLeft?.Invoke(newInputState.xyIgnoreSDKLeftHand);
            AxisChangedIgnoreSDKRight?.Invoke(newInputState.xyIgnoreSDKRightHand);
        }

        previousInputState = newInputState;
    }

    public void ARSetNav()
    {
        isAR = true;

        newARInputState.nav = true;
        newARFrame = true;
    }

    public void ARSetFireDirection(Vector2 joystickDirection)
    {
        isAR = true;

        // assumes game is placed on floor
        Vector3 ProjectedCamera = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Vector2 CameraForward = new Vector2(ProjectedCamera.x, ProjectedCamera.z);
        float angle = Vector2.SignedAngle(Vector2.up, joystickDirection) + Vector2.SignedAngle(Vector2.up, CameraForward);
        angle *= (2 * Mathf.PI) / 360f;

        newARInputState.xy = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
        newARFrame = true;
    }

    public void ARSetFire()
    {
        isAR = true;

        newARInputState.fire = true;
        newARFrame = true;
    }

    public void ARUnsetFire()
    {
        isAR = true;

        newARInputState.fire = false;
        newARFrame = true;
    }

    private bool FireControlActive(Handedness hand)
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

    private string AliasToControlString(ButtonAlias Alias, Handedness hand)
    {
        if (hand == Handedness.LEFT)
        {
            switch (Alias)
            {
                case ButtonAlias.AXIS_CLICK:
                    if (XRSettings.loadedDeviceName == "WindowsMR")
                        return "VR_Thumbrest_Left";
                    else
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
                    if (XRSettings.loadedDeviceName == "WindowsMR")
                        return "VR_Thumbrest_Right";
                    else
                        return "VR_Primary2DAxisClickButton_Right";
                case ButtonAlias.AXIS_TOUCH:
                    return "VR_Primary2DAxisTouchButton_Right";
                default:
                    return "";
            }
        }
    }

    private string AliasToControlStringSDKSpecific(AxisAlias Alias, Handedness Hand)
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

    private string AliasToControlString(AxisAlias Alias, Handedness Hand)
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

    private bool GetButton(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButton(AliasToControlString(alias, hand));
    }

    private bool GetButtonDown(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButtonDown(AliasToControlString(alias, hand));
    }

    private bool GetButtonUp(ButtonAlias alias, Handedness hand)
    {
        return Input.GetButtonUp(AliasToControlString(alias, hand));
    }

    private float GetAxis(AxisAlias alias, Handedness hand, bool ignoreSDKSpecific = false)
    {
        if (ignoreSDKSpecific)
            return Input.GetAxis(AliasToControlString(alias, hand));
        else
            return Input.GetAxis(AliasToControlStringSDKSpecific(alias, hand));
    }

    private Handedness PreferedHand()
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
