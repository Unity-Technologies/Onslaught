using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetARNav : MonoBehaviour
{
    public void SetNav()
    {
        if (GameManager.instance != null)
            GameManager.instance.inputAbstraction.ARSetNav();
    }
}
