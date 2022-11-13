using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;
using UnityEngine.UI;

public class TankAIAgent : Agent
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
    private int count;

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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var y = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            y = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            y = -1;
        }

        var x = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            x = 1;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = -1;
        }

        //var z = 0;
        //if (Input.GetKey(KeyCode.Comma))
        //{
        //    z = 1;
        //}
        //else if (Input.GetKey(KeyCode.Period))
        //{
        //    z = -1;
        //}

        var f = 0;
        if (Input.GetKeyUp(KeyCode.Space))
        {
            f = 1;
        }

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = x + 1;
        discreteActions[1] = y + 1;
        //discreteActions[2] = z + 1;
        discreteActions[2] = f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!enemy)
        {
            return;
        }

        var x = GetCoordNorm(transform.localPosition.x);
        sensor.AddObservation(x);
        var y = (transform.localPosition.y - bounds.min.y) / (bounds.max.y - bounds.min.y);
        sensor.AddObservation(y);
        var z = GetCoordNorm(transform.localPosition.z);
        sensor.AddObservation(z);

        Vector3 dirToEnemy = enemy.transform.localPosition - transform.localPosition;
        var ex = GetCoordNorm(dirToEnemy.x, true);
        sensor.AddObservation(ex);
        var ey = dirToEnemy.y / (bounds.max.y - bounds.min.y);
        sensor.AddObservation(ey);
        var ez = GetCoordNorm(dirToEnemy.z, true);
        sensor.AddObservation(ez);

        var rx = GetAngleNorm(transform.localRotation.eulerAngles.x);
        sensor.AddObservation(rx);
        var ry = GetAngleNorm(transform.localRotation.eulerAngles.y);
        sensor.AddObservation(ry);
        var rz = GetAngleNorm(transform.localRotation.eulerAngles.z);
        sensor.AddObservation(rz);

        var sx = rigidBody.velocity.x / mc.maxSpeed / 1.2f;
        sensor.AddObservation(sx);
        var sy = rigidBody.velocity.y / mc.maxSpeed / 1.2f;
        sensor.AddObservation(sy);
        var sz = rigidBody.velocity.z / mc.maxSpeed / 1.2f;
        sensor.AddObservation(sz);

        //for (int i = 0; i < 360; i*=10)
        //{
            Vector3 dir = new Vector3(fireTransform.forward.x, 0, fireTransform.forward.z);
            //dir = Quaternion.Euler(0, i, 0) * dir;
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
            sensor.AddObservation(val/3f);
            sensor.AddObservation(dist);
        //}
        
        int cs = CanShoot(IsCloseEnough()) ? 1 : 0;
        sensor.AddObservation(cs);

        int cf = fc.CanFire() ? 1 : 0;
        sensor.AddObservation(cf);


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

    public override void OnActionReceived(ActionBuffers actions)
    {
        int move = actions.DiscreteActions[0] - 1;
        int turn = actions.DiscreteActions[1] - 1;
        //int turretTurn = actions.DiscreteActions[2] - 1;
        int fire = actions.DiscreteActions[2];
        mc.Accelerate(move, turn);
        //mc.TurnTurret(turretTurn);
        if (fire == 1)
        {
            fc.Fire();
        }

        AddReward(-1f / 10000);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Borders"))
        {
            SetReward(-2f);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
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
        SpawnUnit();
    }

    private static Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    private void Awake()
    {
        terrain = GetComponentInParent<Terrain>();
        bounds = terrain.GetComponent<TerrainCollider>().bounds;
        fc = GetComponent<FireController>();
        mc = GetComponent<MoveController>();
        rigidBody = GetComponent<Rigidbody>();
        fireInd = transform.Find("FireInd");
        fierIndImage = fireInd.Find("Panel").Find("Image").GetComponent<Image>();
    }

    private Vector3 GetRandSpawnPoint(bool far = false)
    {
        var offset = far ? 30 : 0;
        var p = new Vector3(Random.Range(bounds.min.x + 4 + offset, bounds.max.x - 4)
            , 0
            , Random.Range(bounds.min.z + 4 + offset, bounds.max.z - 4)
        );

        p.Set(p.x, terrain.SampleHeight(p)+1, p.z);
        return p;
    }

    private void SpawnUnit()
    {
        enemy = Instantiate(
            unitPrefab
            , GetRandSpawnPoint(true)
            , GetRandomRotation()
            , terrain.transform
        );
        SetupUnit(enemy, Color.red, LayerMask.NameToLayer("EnemyUnits"));
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

    private void LateUpdate()
    {
        //fireInd.gameObject.SetActive(false);
        
        //if (!enemy)
        //{
        //    return;
        //}

        //Debug.DrawLine(transform.position, enemy.transform.position, Color.red);

        //bool closeEnough = IsCloseEnough();
        ////if (!closeEnough)
        ////{
        ////    if (proximityRewardAdded)
        ////    {
        ////        AddReward(-1f);
        ////        proximityRewardAdded = false;
        ////    }
        ////    return;
        ////}

        ////if (!proximityRewardAdded)
        ////{
        ////    AddReward(1f);
        ////    proximityRewardAdded = true;
        ////}

        //if (closeEnough)
        //{
        //    fireInd.gameObject.SetActive(true);
        //    fierIndImage.color = Color.yellow;
        //}
        
        //if (CanShoot(closeEnough))
        //{
        //    if (fc.CanFire())
        //    {
        //        fierIndImage.color = Color.green;
        //    }
        //    AddReward(1f);
        //    //EndEpisode();
        //}
    }
}
