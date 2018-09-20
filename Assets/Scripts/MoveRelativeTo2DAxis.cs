using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

public class MoveRelativeTo2DAxis : MonoBehaviour
{
    public float amplitudeMaxX = 1f;
    public float amplitudeMaxY = 1f;

    public InputAbstraction.Handedness hand = InputAbstraction.Handedness.LEFT;
    public bool ignoreSDK = false;

    private Transform m_Transform;

    void Start()
    {
        m_Transform = this.transform;
        amplitudeMaxX = Mathf.Abs(amplitudeMaxX);
        amplitudeMaxY = Mathf.Abs(amplitudeMaxY);
    }

    // Update is called once per frame
    void Update()
    {
        m_Transform.localPosition = new Vector3(
            InputAbstraction.GetAxis(InputAbstraction.AxisAlias.X, hand, ignoreSDK) * amplitudeMaxX,
            m_Transform.localPosition.y,
            InputAbstraction.GetAxis(InputAbstraction.AxisAlias.Y, hand, ignoreSDK) * amplitudeMaxY);
    }
}
