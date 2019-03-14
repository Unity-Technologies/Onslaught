using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithPrimaryAxis : MonoBehaviour
{
    private Transform m_Transform;

    // Start is called before the first frame update
    void Start()
    {
        m_Transform = GetComponent<Transform>();
    }

    private void OnEnable()
    {
        InputAbstraction.AxisChanged += OnAxisChanged;
    }

    private void OnDisable()
    {
        InputAbstraction.AxisChanged -= OnAxisChanged;
    }
    
    void OnAxisChanged(Vector2 value)
    {
        float degrees = GetDegreeRotation(value);
        if (degrees > 0)
            m_Transform.rotation = Quaternion.Euler(0f, degrees, 0f);
    }

    float GetDegreeRotation(Vector2 value)
    {
        float degrees = 0f;
        float X = value.x;
        float Y = value.y;

        bool XIsNegative = X < 0 ? true : false;
        bool YIsNegative = Y < 0 ? true : false;

        X = Mathf.Abs(X);
        Y = Mathf.Abs(Y);

        degrees = (YIsNegative ^ XIsNegative) ? Mathf.Atan(Y / X) * (180 / Mathf.PI) : Mathf.Atan(X / Y) * (180 / Mathf.PI);

        if (YIsNegative && !XIsNegative)
            degrees += 90;
        else if (YIsNegative && XIsNegative)
            degrees += 180;
        else if (!YIsNegative && XIsNegative)
            degrees += 270;

        return degrees;
    }
}
