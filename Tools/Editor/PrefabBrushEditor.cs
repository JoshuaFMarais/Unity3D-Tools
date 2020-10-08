using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PrefabBrushInspector))]
public class PrefabBrushEditor : Editor
{

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Raycaster();

    }

    void OnEnable()
    {
        monos = (PrefabBrushInspector)target;
        monos.Refresh();
    }
    enum BrushMode { del_all, add, del_selected };
    [SerializeField]
    static BrushMode brushMode;
    bool drawDefualts = false;
    public override void OnInspectorGUI()
    {
        GUI.color = Color.green;
        GUILayout.Label("Set the 'AcceptedLayers' property for better control");
        GUI.color = monos.colors.BaseColor;
        drawDefualts = GUILayout.Toggle(drawDefualts, "Setter");
        if (drawDefualts)
        {
            DrawDefaultInspector();
        }
        else
        {
            GUILayout.Label("Brush Mode " + brushMode);
            GUILayout.Space(11);
            monos.UseNormals = GUILayout.Toggle(monos.UseNormals, "Use Mesh Normals");
            GUILayout.Label("Brush Size");
            monos.BrushSize = EditorGUILayout.Slider(monos.BrushSize, 0.1f, 50);
            GUILayout.Label("Obj Spawn Number");
            monos.SpawnNumber = EditorGUILayout.IntSlider(monos.SpawnNumber, 1, 50);
            GUILayout.Label("Scale Offset");
            monos.ScaleOffset = EditorGUILayout.Slider(monos.ScaleOffset, 0.1f, 3);
            //  monos. = EditorGUILayout.IntSlider(monos.sca, 1, 50);
            GUI.color = monos.colors.SecondaryColor;
            if (brushMode == BrushMode.del_all) GUI.color = monos.colors.ActiveColor;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Brush Delete All Types"))
            {
                brushMode = BrushMode.del_all;
            }
            GUI.color = monos.colors.SecondaryColor;
          
            GUILayout.EndHorizontal();

            if (monos.brushs == null||monos.brushs.Length==0) {
                GUILayout.Label("Click On the 'Setters' and assign some BRUSHES to get started");
            }

            for (int i = 0; i < monos.brushs.Length; i++)
            {
                GUI.color = monos.colors.DelColor;

                GUILayout.BeginHorizontal();
                if (monos.m_AllObjectsSpawned.Count > i) GUILayout.Label("Count " + monos.m_AllObjectsSpawned[i].Count, GUILayout.Width(100)); else GUILayout.Label("Count 0", GUILayout.Width(100));
                if (brushMode == BrushMode.del_selected && i == monos.BrushSelected)
                {
                    GUI.color = monos.colors.ActiveColor;
                }
                if (GUILayout.Button("Delete Type", GUILayout.Width(100)))
                {
                    monos.BrushSelected = i;
                    brushMode = BrushMode.del_selected;
                }
                GUI.color = monos.colors.BaseColor;
                if (monos.BrushSelected == i) GUI.color = monos.colors.ActiveColor;
                if (GUILayout.Button(monos.brushs[i].Name))
                {
                    brushMode = BrushMode.add;
                    monos.BrushSelected = i;
                }
                GUILayout.EndHorizontal();
            }  

            GUI.color = Color.red;
            if (GUILayout.Button("Clear All Objects"))
            {
                monos.ClearAllObjectsAndData();
            }
        }
      
       
    }
    PrefabBrushInspector monos;

    Vector2 mousePos = new Vector2();
    RaycastHit hit = new RaycastHit();
    Vector3 HitPoint = new Vector3();
    Vector3 HitNormal = new Vector3(0, 1, 0);
    public RaycastHit Raycaster()
    {
        Selection.objects = new Object[] { ((PrefabBrushInspector)target).gameObject };
        Event e = Event.current;
        monos = (PrefabBrushInspector)target;

        Handles.color = new Color(0, 1, 0, 0.4f);

        if (Camera.current != null)
        {
            mousePos.Set(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y - 40);
            Ray ray = Camera.current.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, monos.AcceptedLayers))
            {
                HitPoint = hit.point;
                HitNormal = hit.normal;
            }
            //look           
            Handles.color = Handles.yAxisColor;
            Handles.CircleHandleCap(
                0,
                HitPoint + new Vector3(0f, 0, 0f),
                Quaternion.LookRotation(HitNormal),
                monos.BrushSize,
                EventType.Repaint
            );
            //end look
            if (e.isMouse && (e.button == 0 && e.type == EventType.MouseDrag))
            {
                switch (brushMode)
                {
                    case BrushMode.add:
                        monos.PaintObjects(hit.point, -hit.normal);
                        break;
                    case BrushMode.del_selected:
                        monos.RemoveInRange(hit.point, monos.BrushSelected);
                        break;
                    case BrushMode.del_all:
                        monos.RemoveAllInRange(hit.point);
                        break;
                }
                Repaint();
                e.Use();
            }
        }

        return hit;
    }

}
