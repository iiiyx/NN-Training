using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankNNAgent: MonoBehaviour
{
    public Transform turretTransform;
    public Transform fireTransform;
    public GameObject unitPrefab;
    private Terrain terrain;
    private Bounds bounds;
    private FireController fc;
    private MoveController mc;
    private Rigidbody rigidBody;
    private Transform fireInd;
    private Image fierIndImage;
    private GameObject enemy;
    private bool proximityRewardAdded;
    private bool targetingRewardAdded;
    private int count;
    private float reward = 0;
    private AIManager aiManager;
    private TankNN nn;
    private bool stopped;

    // Use this for initialization
    void Start()
    {
        nn = new TankNN(25, 128, 128, 3);
        terrain = GetComponentInParent<Terrain>();
        bounds = terrain.GetComponent<TerrainCollider>().bounds;
        fc = GetComponent<FireController>();
        mc = GetComponent<MoveController>();
        rigidBody = GetComponent<Rigidbody>();
        fireInd = transform.Find("FireInd");
        fierIndImage = fireInd.Find("Panel").Find("Image").GetComponent<Image>();
        aiManager = GameObject.FindObjectOfType<AIManager>();
    }

    void LateUpdate()
    {
        if (stopped)
        {
            return;
        }

        CollectObservations(out List<float> inputList);
        int[] inputs = nn.FeedInputs(inputList.ToArray());
        HandleInputs(inputs);
    }

    private void HandleInputs(int[] inputs)
    {

        if (!enemy)
        {
            return;
        }
        int move = inputs[0];
        int turn = inputs[1];
        int fire = inputs[2];
        mc.Accelerate(move, turn);
        if (fire == 1)
        {
            fc.Fire();
            //if (!CanShoot(IsCloseEnough()))
            //{
            //    AddReward(-1f);
            //}
        }
        var closeEnough = IsCloseEnough();
        var canShoot = CanShoot(closeEnough);

        if (!closeEnough && proximityRewardAdded)
        {
            AddReward(-0.1f);
            proximityRewardAdded = false;
        }

        if (closeEnough && !proximityRewardAdded)
        {
            AddReward(0.1f);
            proximityRewardAdded = true;
        }

        if (canShoot && !targetingRewardAdded)
        {
            AddReward(0.1f);
            targetingRewardAdded = true;
        }
        if (!canShoot && targetingRewardAdded)
        {
            AddReward(-0.1f);
            targetingRewardAdded = false;
        }

        //AddReward(-0.1f / 10000);
    }

    internal void AddReward(float v)
    {
        reward += v;
    }

    internal void SetReward(float v)
    {
        reward = v;
    }

    internal void EndEpisode()
    {
        aiManager.OnAgentEpisodeEnd(this, nn, reward);
        stopped = true;
    }

    private bool IsCloseEnough()
    {
        var d = Vector3.Distance(transform.position, enemy.transform.position);
        return d <= fc.m_AttackRange;
    }

    public bool CanShoot(bool closeEnough)
    {
        if (closeEnough)
        {
            if (Physics.Raycast(fireTransform.position, fireTransform.forward, out RaycastHit hit, fc.m_AttackRange))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("EnemyUnits"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void CollectObservations(out List<float> inputs)
    {
        inputs = null;
        if (!enemy)
        {
            return;
        }

        inputs = new List<float>(25);

        var x = GetCoordNorm(transform.localPosition.x);
        inputs.Add(x);
        var y = (transform.localPosition.y - bounds.min.y) / (bounds.max.y - bounds.min.y);
        inputs.Add(y);
        var z = GetCoordNorm(transform.localPosition.z);
        inputs.Add(z);

        Vector3 dirToEnemy = enemy.transform.localPosition - transform.localPosition;
        var ex = GetCoordNorm(dirToEnemy.x, true);
        inputs.Add(ex);
        var ey = dirToEnemy.y / (bounds.max.y - bounds.min.y);
        inputs.Add(ey);
        var ez = GetCoordNorm(dirToEnemy.z, true);
        inputs.Add(ez);

        var rx = GetAngleNorm(transform.localRotation.eulerAngles.x);
        inputs.Add(rx);
        var ry = GetAngleNorm(transform.localRotation.eulerAngles.y);
        inputs.Add(ry);
        var rz = GetAngleNorm(transform.localRotation.eulerAngles.z);
        inputs.Add(rz);

        //var sx = rigidBody.velocity.x / mc.maxSpeed / 1.2f;
        //inputs.Add(sx);
        //var sy = rigidBody.velocity.y / mc.maxSpeed / 1.2f;
        //inputs.Add(sy);
        //var sz = rigidBody.velocity.z / mc.maxSpeed / 1.2f;
        //inputs.Add(sz);

        for (int i = -90; i <= 90; i += 30)
        {
            Vector3 dir = new Vector3(fireTransform.forward.x, 0, fireTransform.forward.z);
            dir = Quaternion.Euler(0, i, 0) * dir;
            float val = 0f;
            float dist = 1f;
            float maxDist = fc.m_AttackRange + 2;
            if (Physics.Raycast(fireTransform.position, dir, out RaycastHit hit, maxDist))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("EnemyUnits"))
                {
                    val = 3f;
                    dist = hit.distance / maxDist;
                }
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    val = 2f;
                    dist = hit.distance / maxDist;
                }
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Borders"))
                {
                    val = 1f;
                    dist = hit.distance / maxDist;
                }
            }
            inputs.Add(val / 3f);
            inputs.Add(dist);
        }

        //int valE = 0;
        //if (Physics.Raycast(fireTransform.position, enemy.transform.position, out RaycastHit hitE))
        //{
        //    if (hitE.transform.gameObject.layer == LayerMask.NameToLayer("EnemyUnits"))
        //    {
        //        valE = 1;
        //    }
        //}
        //inputs.Add(valE);


        int cs = CanShoot(IsCloseEnough()) ? 1 : 0;
        inputs.Add(cs);

        int cf = fc.CanFire() ? 1 : 0;
        inputs.Add(cf);


        //if (++count % 100 == 0)
        //{
        //    Debug.Log("====================================");

        //    Debug.Log(x + "; " + z);
        //    Debug.Log(ex + "; " + ez);

        //    Debug.Log(transform.localRotation.eulerAngles.x + "; "
        //        + transform.localRotation.eulerAngles.y + "; "
        //        + transform.localRotation.eulerAngles.z
        //    );
        //    Debug.Log(rx + "; " + ry + "; " + rz);

        //    Debug.Log(s);

        //    Debug.Log(cs);
        //    Debug.Log(cf);

        //    Debug.Log("====================================");
        //    count = 0;
        //}
    }

    private float GetCoordNorm(float val, bool withOffset = false)
    {
        var offset = withOffset ? 60 : 0;
        return (val + offset) / (60 + offset);
    }

    private float GetAngleNorm(float val)
    {
        var offset = 0;
        return (val + offset) / (360 + offset);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Borders"))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    public void OnEpisodeBegin()
    {
        stopped = false;
        reward = 0;
        proximityRewardAdded = false;
        if (enemy != null && enemy.activeSelf)
        {
            enemy.SetActive(false);
            Destroy(enemy);
        }
        rigidBody.isKinematic = true;
        transform.position = GetRandSpawnPoint();
        transform.localRotation = GetRandomRotation();
        rigidBody.isKinematic = false;
        SpawnEnemy();
    }

    private static Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    private Vector3 GetRandSpawnPoint(bool far = false)
    {
        var offset = far ? 30 : 0;
        var p = new Vector3(Random.Range(bounds.min.x + 4 + offset, bounds.max.x - 4)
            , 0
            , Random.Range(bounds.min.z + 4 + offset, bounds.max.z - 4)
        );

        p.Set(p.x, terrain.SampleHeight(p) + 1, p.z);
        return p;
    }

    private void SpawnEnemy()
    {
        enemy = Instantiate(
            unitPrefab
            , GetRandSpawnPoint(true)
            , GetRandomRotation()
            , terrain.transform
        );
        SetupEnemy(enemy, Color.red, LayerMask.NameToLayer("EnemyUnits"));
    }

    private void SetupEnemy(GameObject unit, Color color, int layer)
    {
        MeshRenderer[] renderers = unit.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = color;
        }

        unit.gameObject.layer = layer;
    }
}
