using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class Player : Health
{
    public AudioClip hurtSound;
    public AudioClip healSound;

    private Text m_HealthDisplayText;
    private AudioSource m_hitAudioSource;
    private AudioSource m_healAudioSource;

    protected override void Start()
    {
        base.Start();

        UpdateHealthDisplayText();
        m_hitAudioSource = gameObject.AddComponent<AudioSource>();
        m_healAudioSource = gameObject.AddComponent<AudioSource>();
        m_hitAudioSource.clip = hurtSound;
        m_healAudioSource.clip = healSound;
    }

    public override void ChangeHealth(float amountToChange)
    {
        base.ChangeHealth(amountToChange);

        if (amountToChange < 0)
            m_hitAudioSource.Play();
        else if (amountToChange > 0)
            m_healAudioSource.Play();

        UpdateHealthDisplayText();
    }

    protected override void Die()
    {
        GameManager.instance.StateTransitionTo(GameManager.GameState.Menu);

        base.Die();
    }

    public void SetHealthDisplayText(Text text)
    {
        m_HealthDisplayText = text;
    }

    public void AddSpecialAmmo(int ammoToAdd)
    {
        GetComponentInChildren<FireBullet>().AddSpecialAmmo(1);
    }

    private void UpdateHealthDisplayText()
    {
        if (m_HealthDisplayText != null)
            m_HealthDisplayText.text = healthValue.ToString();
    }
}
