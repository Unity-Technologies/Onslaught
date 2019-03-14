using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class MoveRelativeTo2DAxis : MonoBehaviour
{
    public float amplitudeMaxX = 1f;
    public float amplitudeMaxY = 1f;

    // This script assumes these values don't change during runtime
    public InputAbstraction.Handedness hand = InputAbstraction.Handedness.LEFT;
    public bool ignoreSDK = false;

    private Transform m_Transform;

    void Start()
    {
        m_Transform = this.transform;
        amplitudeMaxX = Mathf.Abs(amplitudeMaxX);
        amplitudeMaxY = Mathf.Abs(amplitudeMaxY);

        if (!ignoreSDK)
            GameManager.instance.inputAbstraction.AxisChanged += OnAxisChanged;
        else if (hand == InputAbstraction.Handedness.LEFT)
            GameManager.instance.inputAbstraction.AxisChangedIgnoreSDKLeft += OnAxisChanged;
        else if (hand == InputAbstraction.Handedness.RIGHT)
            GameManager.instance.inputAbstraction.AxisChangedIgnoreSDKRight += OnAxisChanged;
    }

    private void OnDestroy()
    {
        if (!ignoreSDK)
            GameManager.instance.inputAbstraction.AxisChanged -= OnAxisChanged;
        else if (hand == InputAbstraction.Handedness.LEFT)
            GameManager.instance.inputAbstraction.AxisChangedIgnoreSDKLeft -= OnAxisChanged;
        else if (hand == InputAbstraction.Handedness.RIGHT)
            GameManager.instance.inputAbstraction.AxisChangedIgnoreSDKRight -= OnAxisChanged;
    }

    void OnAxisChanged(Vector2 value)
    {
        m_Transform.localPosition = new Vector3(
            value.x * amplitudeMaxX,
            m_Transform.localPosition.y,
            value.y * amplitudeMaxY);
    }
}
