using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//this file Should be attached to a emty game object in your scene. you will then be able to use this tool
//make sure you have the Editor script as well
[System.Serializable]
public class PrefabBrushInspector : MonoBehaviour
{
    [Header("Set Brushes Here")]
    public PrefabBrushAsset[] brushs;
    [Tooltip("change the size  of the brush")]
    [Range(0.1f, 50)]
    public float BrushSize = 1;
    [Tooltip("How many get spawned at a time")]
    [Range(0.1f, 50)]
    public int SpawnNumber = 1;
    [Tooltip("Scale the spawned object's")]
    public float ScaleOffset = 1;
    [HideInInspector]
    public int BrushSelected = 0;
    [Tooltip("what layers are valid to draw too")]
    public LayerMask AcceptedLayers = ~0;

    [Tooltip("Objects will Face the Normalif Enabled")]
    public bool UseNormals = true;

    [System.Serializable]
    public class PrefabBrushAsset
    {
        [Tooltip("Set The Name Of The Brush")]
        public string Name = "Brush";
        [Tooltip("Set What Objects Can Be Spawned With This Brush")]
        public GameObject[] ObjOptions;
        [Tooltip("The Min Scale Multiplier")]
        public float minScale = 1;
        [Tooltip("The Max Scale Multiplier")]
        public float maxScale = 1.1f;
        [HideInInspector]
        public Transform GetParent(int index)
        {
            if (MyParent == null)
            {
                MyParent = new GameObject("" + ObjOptions[0].name + " Container").transform;
                GameObject t = GameObject.Find("Obj Parent");
                if (t == null) t = new GameObject("Obj Parent");
                MyParent.transform.SetParent(t.transform);
                MyParent.localPosition = Vector3.zero;
            }
            return MyParent;
        }
        [SerializeField]
        private Transform MyParent;
    }

    [SerializeField]
    public EditorColors colors;
    [System.Serializable]
    public class EditorColors
    {
        public Color DelColor = Color.red;
        public Color BaseColor = Color.white;
        public Color SecondaryColor = Color.grey;
        public Color ActiveColor = Color.green;
    }
    GameObject m_objectParent
    {
        get
        {
            if (GameObject.Find("Obj Parent"))
                return GameObject.Find("Obj Parent");
            else
                return new GameObject("Obj Parent");
        }
    }
    [SerializeField]
    public List<List<GameObject>> m_AllObjectsSpawned = new List<List<GameObject>>();


    public void PaintObjects(Vector3 point, Vector3 direction)
    {
        Init();

        if (brushs == null) return;



        PrefabBrushAsset b = brushs[BrushSelected];
        GameObject g;
        for (int i = 0; i < SpawnNumber; i++)
        {
            Vector3 p = point + Random.insideUnitSphere * Random.Range(BrushSize * 0.4f, BrushSize);
            RaycastHit hit = raycaster.Raycast(p + ((-direction) * 1000), direction);
            Vector3 SpawnDirection = -hit.normal;

            if (hit.collider != null && hit.collider.gameObject.layer == 13)
            {
                //If the object is a prefab
                int index = Random.Range(0, b.ObjOptions.Length);
                if (PrefabUtility.IsPartOfAnyPrefab(b.ObjOptions[index]))
                {
                    g = (GameObject)PrefabUtility.
                          InstantiatePrefab(b.ObjOptions[index]);
                }
                //if the it is not a prefab
                else
                {
                    g = Instantiate(b.ObjOptions[index]);
                }
                g.transform.SetParent(b.GetParent(BrushSelected));
                g.transform.position = hit.point;
                g.transform.localScale *= Random.Range(b.minScale, b.maxScale) * ScaleOffset;
                if (UseNormals)
                {
                    g.transform.rotation = Quaternion.LookRotation(SpawnDirection);
                    g.transform.Rotate(-90, 0, 0, Space.Self);
                }

                g.transform.Rotate(0, Random.Range(0, 300), 0, Space.Self);

                m_AllObjectsSpawned[BrushSelected].Add(g);
            }
        }
    }
    public void Refresh()
    {
        m_AllObjectsSpawned.Clear();
        for (int i = 0; i < brushs.Length; i++)
        {
            m_AllObjectsSpawned.Add(new List<GameObject>());
            //make sure that there is a parent for this object group
            Transform t = brushs[i].GetParent(i);
            for (int i2 = 0; i2 < m_objectParent.transform.GetChild(i).childCount; i2++)
            {
                m_AllObjectsSpawned[i].Add(m_objectParent.transform.GetChild(i).GetChild(i2).gameObject);
            }
        }
    }
    
    RaycastHelper raycaster;
    public void Init()
    {
       
        if (brushs.Length != m_objectParent.transform.childCount)
        {
            for (int i = m_objectParent.transform.childCount - 1; i > -1; i--)
            {
                if(m_objectParent.transform.GetChild(i).childCount==0)
                DestroyImmediate(m_objectParent.transform.GetChild(i).gameObject);
            }
        }
        if (raycaster == null)
        {
            raycaster = new RaycastHelper();
        }
        if (m_AllObjectsSpawned == null)
        {
            m_AllObjectsSpawned = new List<List<GameObject>>();
        }
        if (brushs == null) return;
        while (m_AllObjectsSpawned.Count < brushs.Length)
        {
            m_AllObjectsSpawned.Add(new List<GameObject>());
        }
        for (int i = 0; i < brushs.Length; i++)
        {
            if (brushs[i].ObjOptions != null && brushs[i].ObjOptions[0] != null)
                brushs[i].GetParent(i);
        }
    }
    public void RemoveAllInRange(Vector3 point)
    {
        Init();
        for (int i = 0; i < m_AllObjectsSpawned.Count; i++)
        {
            RemoveInRange(point, i);
        }
    }

    public void ClearAllObjectsAndData()
    {
        m_AllObjectsSpawned.Clear();
        DestroyImmediate(m_objectParent);
    }

    public void RemoveInRange(Vector3 point, int objTypeIndex)
    {
        Init();
        for (int i = m_AllObjectsSpawned[objTypeIndex].Count - 1; i > -1; i--)
        {
            if (m_AllObjectsSpawned[objTypeIndex] != null && m_AllObjectsSpawned[objTypeIndex][i] == null)
            {
                m_AllObjectsSpawned[objTypeIndex].RemoveAt(i);
            }
            else
            if (IsInRange(m_AllObjectsSpawned[objTypeIndex][i].transform.position, point))
            {
                DestroyImmediate(
m_AllObjectsSpawned[objTypeIndex][i]
);
            }
        }
    }

    public bool IsInRange(Vector3 pos, Vector3 other)
    {
        if ((pos - other).magnitude <= BrushSize)
        {
            return true;
        }
        return false;
    }

#if UNITY_EDITOR



#endif
}

