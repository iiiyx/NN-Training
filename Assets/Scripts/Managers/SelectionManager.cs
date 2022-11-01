using System.Collections.Generic;
using System.Timers;
using UnityEngine;

// TODO: pull out the GUI renderer
public class SelectionManager : MonoBehaviour
{
    public float m_TankRadius = 1;
    public float m_Gap = 1;
    public LayerMask m_IgnoreLayers;

    private readonly HashSet<Transform> m_SelectedUnits = new HashSet<Transform>();
    private bool m_IsDragging = false;
    private Vector3 m_DragStartingPosition;

    private List<GameObject> m_PlayerUnits;

    private Vector3 m_ClickMousePosition;
    private bool m_ClickedWithShift;
    private bool m_ClickedWithAlt;
    private LayerMask m_RaycastLayers;

    private readonly Timer m_MouseSingleClickTimer = new Timer();

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerUnits = GetComponentInParent<UnitSpawner>().m_PlayerUnits;
        m_MouseSingleClickTimer.Interval = 400;
        m_MouseSingleClickTimer.Elapsed += OnSingleClickTimer;

        m_RaycastLayers = ~m_IgnoreLayers;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void OnGUI()
    {
        if (m_IsDragging && IsBigEnoughRect())
        {
            Rect rect = GetScreenRect(m_DragStartingPosition, Input.mousePosition);
            DrawSelectRect(rect);
        }
    }

    private void HandleInput()
    {
        bool withShiftKey = Input.GetKey(KeyCode.LeftShift);
        bool withAltKey = Input.GetKey(KeyCode.LeftAlt);
        if (Input.GetMouseButtonDown(0))
        {
            m_DragStartingPosition = Input.mousePosition;
            m_IsDragging = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!IsBigEnoughRect())
            {
                m_IsDragging = false;
            }

            if (m_IsDragging)
            {
                HandleDragging(withShiftKey, withAltKey);
                return;
            }

            if (m_MouseSingleClickTimer.Enabled == false)
            {
                // ... timer start
                m_MouseSingleClickTimer.Start();
                m_ClickMousePosition = Input.mousePosition;
                m_ClickedWithShift = withShiftKey;
                m_ClickedWithAlt = withAltKey;
                HandleSignleClick();
                // ... wait for double click...
            }
            else
            {
                //Doubleclick performed - Cancel single click
                m_MouseSingleClickTimer.Stop();

                //Do your stuff here for double click...
                HandleDoubleClick(Input.mousePosition, withShiftKey, withAltKey);
            }
        }

        if (Input.GetMouseButtonUp(1) && m_SelectedUnits.Count > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, m_RaycastLayers))
            {
                if (hit.transform.gameObject.layer == 11) // enemy
                {
                    Attack(hit.transform);
                }
                //else if (hit.transform.gameObject.layer == 8) // ground
                //{
                //    ///
                //}
                else
                {
                    MoveUnits(hit.point);
                }
            }
        }
    }

    private void Attack(Transform hitTransform)
    {
        foreach (Transform unit in m_SelectedUnits)
        {
            UnitManager pUnit = unit.gameObject.GetComponent<UnitManager>();
            pUnit.Attack(hitTransform);
        }
    }

    private void MoveUnits(Vector3 hitPos)
    {
        Vector3 targetPos = new Vector3(hitPos.x, 0f, hitPos.z);

        //GetPointsForSquad(targetPos, out Vector3 shift, out List<Vector3> points);
        //int count = 0;
        foreach (Transform unit in m_SelectedUnits)
        {
            UnitManager pUnit = unit.gameObject.GetComponent<UnitManager>();
            //pUnit.MoveTo(points[count++], shift.magnitude);
            pUnit.MoveTo(hitPos, 0f);
        }
    }

    private void GetPointsForSquad(Vector3 targetPosition, out Vector3 squadShiftingVector, out List<Vector3> squadMovePoints)
    {
        int maxTanksNum = m_SelectedUnits.Count;

        if (maxTanksNum == 1)
        {
            squadShiftingVector = Vector3.zero;
            squadMovePoints = new List<Vector3>() { targetPosition };
            return;
        }

        float cellSize = m_TankRadius + m_Gap;
        float halfCell = cellSize / 2;
        Vector3 xBaseShift = Vector3.forward * cellSize;
        Vector3 yBaseShift = Vector3.right * cellSize;

        int squareWidthInTanksNum = (int)Mathf.Floor(Mathf.Sqrt(maxTanksNum));
        if (Mathf.Abs(Mathf.Pow(squareWidthInTanksNum, 2) - maxTanksNum) > Mathf.Abs(Mathf.Pow(squareWidthInTanksNum + 1, 2) - maxTanksNum))
        {
            squareWidthInTanksNum++;
        }
        float halfSide = squareWidthInTanksNum / 2;
        squadShiftingVector = xBaseShift * halfSide + yBaseShift * halfSide;
        squadMovePoints = new List<Vector3>(maxTanksNum);
        bool shouldBreak = false;
        for (int i = 0; i <= squareWidthInTanksNum; i++)
        {
            if (shouldBreak)
            {
                break;
            }
            for (int j = 0; j < squareWidthInTanksNum; j++)
            {
                Vector3 p = targetPosition + xBaseShift * i + yBaseShift * j + Vector3.forward * halfCell + Vector3.right * halfCell - squadShiftingVector;
                squadMovePoints.Add(p);

                if (squareWidthInTanksNum * i + j + 1 == maxTanksNum)
                {
                    shouldBreak = true;
                    break;
                }
            }
        }
    }

    private void OnSingleClickTimer(object o, System.EventArgs e)
    {
        m_MouseSingleClickTimer.Stop();
    }

    private void HandleSignleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(m_ClickMousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, m_RaycastLayers))
        {
            if (hit.transform.gameObject.layer == 9)
            {
                if (m_SelectedUnits.Contains(hit.transform))
                {
                    if (m_ClickedWithShift || m_ClickedWithAlt)
                    {
                        DeselectUnit(hit.transform);
                    }
                    else
                    {
                        SelectUnitOnly(hit.transform);
                    }
                }
                else if (!m_ClickedWithAlt)
                {
                    if (m_ClickedWithShift)
                    {
                        AddUnitToSelection(hit.transform);
                    }
                    else
                    {
                        SelectUnitOnly(hit.transform);
                    }
                }
            }
            else
            {
                DeselectAll();
            }
        }
    }

    private void HandleDoubleClick(Vector3 clickMousePos, bool withShift, bool withAlt)
    {
        Ray ray = Camera.main.ScreenPointToRay(clickMousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, m_RaycastLayers))
        {
            if (hit.transform.gameObject.layer == 9)
            {
                if (!withShift && !withAlt)
                {
                    DeselectAll();
                }
                List<GameObject> unitsOfType = m_PlayerUnits.FindAll(unit => unit.name == hit.transform.name);
                foreach (GameObject unit in unitsOfType)
                {
                    bool isInView = IsWithinViewport(unit.transform, new Vector3(0f, 0f, 0f), new Vector3(Screen.width, Screen.height, 0f));
                    if (isInView)
                    {
                        if (withAlt)
                        {
                            DeselectUnit(unit.transform);
                        }
                        else
                        {
                            AddUnitToSelection(unit.transform);
                        }
                    }
                }
            }
        }
    }

    private void HandleDragging(bool withShiftKey, bool withAltKey)
    {
        HashSet<GameObject> selectedUnits = new HashSet<GameObject>();
        foreach (GameObject unit in m_PlayerUnits)
        {
            if (IsWithinViewport(unit.transform, m_DragStartingPosition, Input.mousePosition))
            {
                selectedUnits.Add(unit);
            }
        }
        if (selectedUnits.Count > 0)
        {
            if (!withShiftKey && !withAltKey)
            {
                DeselectAll();
            }
            foreach (GameObject unit in selectedUnits)
            {
                if (withAltKey)
                {
                    DeselectUnit(unit.transform);
                }
                else
                {
                    AddUnitToSelection(unit.transform);
                }
            }
        }
        m_IsDragging = false;
    }

    private bool IsBigEnoughRect()
    {
        float min = 3f;
        return Mathf.Abs(m_DragStartingPosition.x - Input.mousePosition.x) > min
            || Mathf.Abs(m_DragStartingPosition.y - Input.mousePosition.y) > min;
    }

    private void DeselectUnit(Transform transform)
    {
        m_SelectedUnits.Remove(transform);
        ToggleUnitSelection(transform, false);
    }

    private void OnDeath(Transform transform)
    {
        DeselectUnit(transform);
        m_PlayerUnits.Remove(transform.gameObject);
        transform.GetComponent<UnitManager>().OnDeath -= OnDeath;
    }

    private void AddUnitToSelection(Transform unitTransform)
    {
        m_SelectedUnits.Add(unitTransform);
        ToggleUnitSelection(unitTransform, true);
        unitTransform.GetComponent<UnitManager>().OnDeath += OnDeath;
    }

    private void SelectUnitOnly(Transform unitTransform)
    {
        DeselectAll();
        AddUnitToSelection(unitTransform);
    }
    
    private void DeselectAll()
    {
        foreach(Transform unit in m_SelectedUnits)
        {
            ToggleUnitSelection(unit, false);
        }
        m_SelectedUnits.Clear();
    }

    private static void ToggleUnitSelection(Transform unit, bool isSelect)
    {
        UnitManager m = unit.gameObject.GetComponent<UnitManager>();
        if (m != null)
        {
            m.IsSelected = isSelect;
        }
    }

    private static void DrawSelectRect(Rect rect)
    {
        float thickness = 1f;
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Color.white);
        DrawScreenRect(new Rect(rect.xMin + thickness, rect.yMin + thickness, rect.width - 2*thickness, thickness), Color.black);

        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Color.white);
        DrawScreenRect(new Rect(rect.xMin + thickness, rect.yMax - 2*thickness, rect.width - 2*thickness, thickness), Color.black);

        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Color.white);
        DrawScreenRect(new Rect(rect.xMin + thickness, rect.yMin + thickness, thickness, rect.height - 2*thickness), Color.black);

        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Color.white);
        DrawScreenRect(new Rect(rect.xMax - 2*thickness, rect.yMin + thickness, thickness, rect.height - 2*thickness), Color.black);
    }

    private static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture);
    }

    private static Rect GetScreenRect(Vector3 screenPos1, Vector3 screenPos2)
    {
        screenPos1.y = Screen.height - screenPos1.y;
        screenPos2.y = Screen.height - screenPos2.y;

        Vector3 bottomRight = Vector3.Max(screenPos1, screenPos2);
        Vector3 topLeft = Vector3.Min(screenPos1, screenPos2);

        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    private static Bounds GetViewPortBounds(Camera cam, Vector3 screenPos1, Vector3 screenPos2)
    {
        Vector3 pos1 = cam.ScreenToViewportPoint(screenPos1);
        Vector3 pos2 = cam.ScreenToViewportPoint(screenPos2);

        Vector3 min = Vector3.Min(pos1, pos2);
        Vector3 max = Vector3.Max(pos1, pos2);

        min.z = cam.nearClipPlane;
        max.z = cam.farClipPlane;

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return bounds;
    }

    private static bool IsWithinViewport(Transform unit, Vector3 pos1, Vector3 pos2)
    {
        Bounds bounds = GetViewPortBounds(Camera.main, pos1, pos2);
        return bounds.Contains(Camera.main.WorldToViewportPoint(unit.position));
    }

    private static Texture2D m_Texture;

    private static Texture2D Texture
    {
        get
        {
            if (m_Texture == null)
            {
                m_Texture = new Texture2D(1, 1);
                m_Texture.SetPixel(0, 0, Color.white);
                m_Texture.Apply();
            }
            return m_Texture;
        }
    }
}
