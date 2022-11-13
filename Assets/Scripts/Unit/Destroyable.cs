using UnityEngine;

[RequireComponent(typeof(UnitDeath))]
[RequireComponent(typeof(UnitHealthUI))]
public class Destroyable : MonoBehaviour, IDamagable, IKillable
{
    public float m_StartingHealth = 100f;
    public float m_MaxHealth = 100f;
    public UnitDeath m_UnitDeath;
    public UnitHealthUI m_UnitHealth;

    public delegate void DeathAction(Transform transform);
    public event DeathAction OnDeath;

    [HideInInspector]
    public bool IsSelected
    {
        get => m_IsSelected;
        set
        {
            m_IsSelected = value;
            m_UnitHealth.ToggleHpCanvas(value);
        }
    }

    protected float m_InitialPosHeight;

    protected bool m_IsDead;
    private bool m_IsSelected;
    private float m_CurrentHelath;

    private void OnEnable()
    {
        m_CurrentHelath = m_StartingHealth;
        m_UnitHealth.Setup(m_CurrentHelath, m_MaxHealth);
        m_InitialPosHeight = transform.position.y;
    }

    public void Kill()
    {
        m_IsDead = true;
        IsSelected = false;

        OnDeath?.Invoke(transform);
        m_UnitDeath.Kill();
    }

    public bool Damage(float amount)
    {
        m_CurrentHelath -= amount;
        m_UnitHealth.SetHealth(m_CurrentHelath);

        if (m_CurrentHelath <= 0f)
        {
            Kill();
            return true;
        }
        return false;
    }

    private void OnMouseEnter()
    {
        ToggleHpCanvasOnMouseOver(true);
    }

    private void OnMouseExit()
    {
        ToggleHpCanvasOnMouseOver(false);
    }

    private void ToggleHpCanvasOnMouseOver(bool value)
    {
        if (IsSelected || m_IsDead)
        {
            return;
        }
        m_UnitHealth.ToggleHpCanvas(value);
    }
}
