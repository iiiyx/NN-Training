using System.Collections;
using UnityEngine;

public class ArtilleryControl : TankControl
{
    protected override void SetLookRotation(Vector3 targetPosition)
    {
        m_LookAtRotation = Quaternion.LookRotation(targetPosition - m_TurretTransform.position);

        float alfa = float.NaN;

        if (Mathf.Abs(targetPosition.y - transform.position.y) < 4f)
        {
            //var a = 30f; // угол
            //var vHorz = m_ShellSpeed0 * Mathf.Cos(a); // верт скор
            //var vVert = m_ShellSpeed0 * Mathf.Sin(a); // гор скор
            //var t = vVert / 9.81f; // время до пика параболы из формулы
            // V = V0 + a*t, V = 0 (мы ж в пике), V0 = верт скор в начале, a = -9,81 => t = (V - V0)/a => t = V0/g

            //var S = vHorz * 2 * t = m_ShellSpeed0 * Mathf.Cos(a) * 2 * m_ShellSpeed0 * Mathf.Sin(a) / 9.81f = Mathf.Pow(m_ShellSpeed0, 2f) * Mathf.Sin(2 * a) / 9.81f;
            float distance = Vector3.Distance(targetPosition, transform.position);
            alfa = 90 - Mathf.Asin(distance * 9.81f / Mathf.Pow(m_ShellSpeed0, 2f)) * 90 / Mathf.PI;
        }
        else
        {
            float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPosition.x, targetPosition.z));
            float a = 2 * m_ShellSpeed0 * m_ShellSpeed0;
            float b = (transform.position.y - targetPosition.y) * 9.81f;
            float c = distance * 9.81f;
            float d = a + b;
            float x = Cubic.Solve(c, -d, c, a - d);
            if (!float.IsNaN(x))
            {
                alfa = Mathf.Atan(x) * 180 / Mathf.PI;
            }
        }

        alfa = float.IsNaN(alfa) ? -45f : -alfa;

        m_LookAtRotation = Quaternion.Euler(new Vector3(alfa, m_LookAtRotation.eulerAngles.y, m_LookAtRotation.eulerAngles.z));
        m_TurretIsLocked = false;
    }
}
