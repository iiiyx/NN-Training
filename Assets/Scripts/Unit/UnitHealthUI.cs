using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthUI : MonoBehaviour
{
    public Canvas m_HpCanvas;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;  
    public Color m_ZeroHealthColor = Color.red;

    private float m_CurrentHealth;
    private float m_MaxHealth;
    private Vector3 m_BoxCenter;

    private void Start()
    {
        m_HpCanvas.gameObject.SetActive(false);
        m_BoxCenter = GetComponent<BoxCollider>().center;
    }

    public void ToggleHpCanvas(bool value)
    {
        m_HpCanvas.gameObject.SetActive(value);
    }

    private void LateUpdate()
    {
        Quaternion camRotation = Camera.main.transform.rotation;
        m_HpCanvas.transform.rotation = camRotation;
    }

    public void Setup(float currentHelath, float maxHealth)
    {
        m_CurrentHealth = currentHelath;
        m_MaxHealth = maxHealth;
        SetHealthUI();
    }

    public void SetHealth(float newHealth)
    {
        // Adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
        m_CurrentHealth = newHealth;
        SetHealthUI();
    }


    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        float percent = m_CurrentHealth / m_MaxHealth;
        m_FillImage.fillAmount = percent;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, percent);
    }
}