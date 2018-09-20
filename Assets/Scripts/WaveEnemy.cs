using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEnemy : Health
{
    protected override void Start()
    {
        WaveManager.instance.AddWaveEnemy(this);
        base.Start();
    }

    protected override void Die()
    {
        if (WaveManager.instance != null)
            WaveManager.instance.RemoveWaveEnemy(this);

        base.Die();
    }
}
