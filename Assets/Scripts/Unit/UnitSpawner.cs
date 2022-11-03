using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public GameObject m_TankUnitPrefab;

    [Header("Player Unit Attrs")]
    public int m_PlayerTanksNum = 1;
    public Material m_PlayerUnitMaterial;
    public SingleUnityLayer m_PlayerUnitLayer;
    
    [Header("Enemy Unit Attrs")]
    public int m_EnemyTanksNum = 1;
    public Material m_EnemyUnitMaterial;
    public SingleUnityLayer m_EnemyUnitLayer;

    [HideInInspector] public List<GameObject> m_PlayerUnits = new List<GameObject>();
    [HideInInspector] public List<GameObject> m_EnemyUnits = new List<GameObject>();
    private Terrain m_Terrain;
    private Bounds m_Bounds;
    private UnitSpawnAttributes m_PlayerUnitSpawnAttributes;
    private UnitSpawnAttributes m_EnemyUnitSpawnAttributes;

    private Vector3 GetRandSpawnPoint()
    {
        var p = new Vector3(Random.Range(m_Bounds.min.x, m_Bounds.max.x), 0, Random.Range(m_Bounds.min.z, m_Bounds.max.z));

        p.Set(p.x, m_Terrain.SampleHeight(p), p.z);
        return p;
    }

    private void Awake()
    {
        m_Terrain = Terrain.activeTerrain;
        m_Bounds = m_Terrain.GetComponent<TerrainCollider>().bounds;
        m_PlayerUnitSpawnAttributes = new UnitSpawnAttributes {
            SpawnPoint = GetRandSpawnPoint(),
            Layer = m_PlayerUnitLayer,
            Count = m_PlayerTanksNum,
            Material = m_PlayerUnitMaterial,
            UnitsCollection = m_PlayerUnits,
        };

        m_EnemyUnitSpawnAttributes = new UnitSpawnAttributes
        {
            SpawnPoint = GetRandSpawnPoint(),
            Layer = m_EnemyUnitLayer,
            Count = m_EnemyTanksNum,
            Material = m_EnemyUnitMaterial,
            UnitsCollection = m_EnemyUnits,
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < m_PlayerTanksNum; i++)
        {
            SpawnPlayerUnit(m_TankUnitPrefab);
        }

        for (int i = 0; i < m_EnemyTanksNum; i++)
        {
            SpawnEnemyUnit(m_TankUnitPrefab);
        }

    }

    private void SpawnPlayerUnit(GameObject unitPrefab)
    {
        SpawnUnit(unitPrefab, m_PlayerUnitSpawnAttributes);
    }

    private void SpawnEnemyUnit(GameObject unitPrefab)
    {
        SpawnUnit(unitPrefab, m_EnemyUnitSpawnAttributes);
    }

    private void OnDeath(Transform transform)
    {
        transform.GetComponent<UnitManager>().OnDeath -= OnDeath;
        if (transform.gameObject.layer == m_PlayerUnitLayer.LayerIndex)
        {
            m_PlayerUnits.Remove(transform.gameObject);
        }
        if (transform.gameObject.layer == m_EnemyUnitLayer.LayerIndex)
        {
            m_EnemyUnits.Remove(transform.gameObject);
        }
    }

    private Quaternion GetSpawnRotation(Vector3 p)
    {
        return Quaternion.FromToRotation(p, m_Bounds.center);
    }

    private void SpawnUnit(GameObject unitPrefab, UnitSpawnAttributes attrs)
    {
        float x = Random.Range(-3f, 3f);
        float y = Random.Range(-3f, 3f);
        GameObject unit = Instantiate(unitPrefab, attrs.SpawnPoint + (Vector3.right * x) + (Vector3.forward * y), GetSpawnRotation(attrs.SpawnPoint));
        SetupUnit(unit, attrs.Material.color, attrs.Layer.LayerIndex);
        attrs.UnitsCollection.Add(unit);
        unitPrefab.GetComponent<UnitManager>().OnDeath += OnDeath;
        //AudioSource[] auSources = unitPrefab.GetComponents<AudioSource>();
    }

    private void SetupUnit(GameObject unit, Color color, int layer)
    {
        MeshRenderer[] renderers = unit.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = color;
        }

        unit.gameObject.layer = layer;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SpawnEnemyUnit(m_TankUnitPrefab);
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            SpawnPlayerUnit(m_TankUnitPrefab);
        }
    }
}
