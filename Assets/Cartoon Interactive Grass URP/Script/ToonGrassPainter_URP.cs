using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsTools.URP;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class ToonGrassPainter_URP : MonoBehaviour
{
    public Action OnMeshUpdate;
    public Mesh mesh;
    MeshFilter filter;
    public Color AdjustedColor;

    [Range(1, 600000)] public int grassLimit = 50000;

    private Vector3 lastPosition = Vector3.zero;

    public int toolbarInt = 0;

    public int toolbarIntEdit = 0;

    [SerializeField] List<Vector3> positions = new List<Vector3>();
    [SerializeField] List<Color> colors = new List<Color>();
    [SerializeField] List<int> indicies = new List<int>();
    [SerializeField] List<Vector3> normals = new List<Vector3>();
    [SerializeField] List<Vector2> length = new List<Vector2>();

    public int i = 0;

    public float sizeWidth = 1f;
    public float sizeLength = 1f;
    public float density = 1f;

    public float normalLimit = 1;

    public float rangeR, rangeG, rangeB;
    public LayerMask hitMask = 1;
    public LayerMask paintMask = 1;
    public float brushSize;
    public float brushFalloffSize;
    public float Flow;

    private int flowTimer;
    Vector3 mousePos;
    [HideInInspector] public Vector3 hitPosGizmo;
    Vector3 hitPos;
    [HideInInspector] public Vector3 hitNormal;
    int[] indi;

    public Action<Color> OnGrassCut;
    public Color currentCutColor;

    public int verticesCount;

    private void Start()
    {
        verticesCount = mesh.vertexCount;
    }

    //RUNTIME'DA EDITLEMEK ICIN EKLENDI
    public void RemoveGrassAtPosition(Vector3 position, float removalRadius)
    {
        List<int> indicesToRemove = new List<int>();

        for (int j = 0; j < positions.Count; j++)
        {
            Vector3 grassPosition = positions[j] + transform.position;
            float distance = Vector3.Distance(position, grassPosition);

            if (distance <= removalRadius)
            {
                indicesToRemove.Add(j);
            }
        }

        if (indicesToRemove.Count <= 0) return;
        
        // OnGrassCut?.Invoke(colors[0]);


        // triangle index hatasi fix icin
        foreach (int indexToRemove in indicesToRemove.OrderByDescending(i => i))
        {
            positions.RemoveAt(indexToRemove);
            currentCutColor = colors[indexToRemove];
            colors.RemoveAt(indexToRemove);
            normals.RemoveAt(indexToRemove);
            length.RemoveAt(indexToRemove);
        }

        // Clear and rebuild the indices array
        indicies.Clear();
        for (int i = 0; i < positions.Count; i++)
        {
            indicies.Add(i);
        }

        // Rebuild mesh, guncellenmis data ile
        if (mesh != null)
        {
            Destroy(mesh);
        }
        mesh = new Mesh();
        mesh.SetVertices(positions);
        indi = indicies.ToArray();
        mesh.SetIndices(indi, MeshTopology.Points, 0);
        mesh.SetUVs(0, length);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);

        //Meshi guncelledikten sonra
        PlayerMove_URP.Instance.currentMesh = mesh;

        filter.mesh = mesh;
    }

    public bool AreVerticesEqual(int vertices1, int vertices2)
    {
        return vertices1 == vertices2;
    }

#if UNITY_EDITOR
    void OnFocus()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= this.OnScene;
        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += this.OnScene;
    }

    void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnScene;
    }

    private void OnEnable()
    {
        filter = GetComponent<MeshFilter>();
        SceneView.duringSceneGui += this.OnScene;
    }

    public void ClearMesh()
    {
        i = 0;
        positions = new List<Vector3>();
        indicies = new List<int>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        length = new List<Vector2>();
    }

    void OnScene(SceneView scene)
    {
        if (Selection.Contains(gameObject))
        {
            Event e = Event.current;
            RaycastHit terrainHit;
            mousePos = e.mousePosition;
            float ppp = EditorGUIUtility.pixelsPerPoint;
            mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
            mousePos.x *= ppp;

            Ray rayGizmo = scene.camera.ScreenPointToRay(mousePos);
            RaycastHit hitGizmo;

            if (Physics.Raycast(rayGizmo, out hitGizmo, 200f, hitMask.value))
            {
                hitPosGizmo = hitGizmo.point;
            }

            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 0)
            {
                for (int k = 0; k < density; k++)
                {
                    float t = 2f * Mathf.PI * Random.Range(0f, brushSize);
                    float u = Random.Range(0f, brushSize) + Random.Range(0f, brushSize);
                    float r = (u > 1 ? 2 - u : u);
                    Vector3 origin = Vector3.zero;

                    if (k != 0)
                    {
                        origin.x += r * Mathf.Cos(t);
                        origin.y += r * Mathf.Sin(t);
                    }
                    else
                    {
                        origin = Vector3.zero;
                    }

                    Ray ray = scene.camera.ScreenPointToRay(mousePos);
                    ray.origin += origin;

                    if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value) && i < grassLimit &&
                        terrainHit.normal.y <= (1 + normalLimit) && terrainHit.normal.y >= (1 - normalLimit))
                    {
                        if ((paintMask.value & (1 << terrainHit.transform.gameObject.layer)) > 0)
                        {
                            hitPos = terrainHit.point;
                            hitNormal = terrainHit.normal;
                            if (k != 0)
                            {
                                var grassPosition = hitPos; // + Vector3.Cross(origin, hitNormal);
                                grassPosition -= this.transform.position;

                                positions.Add((grassPosition));
                                indicies.Add(i);
                                length.Add(new Vector2(sizeWidth, sizeLength));

                                colors.Add(new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR),
                                    AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG),
                                    AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));

                                normals.Add(terrainHit.normal);
                                i++;
                            }
                            else
                            {
                                if (Vector3.Distance(terrainHit.point, lastPosition) > brushSize)
                                {
                                    var grassPosition = hitPos;
                                    grassPosition -= this.transform.position;
                                    positions.Add((grassPosition));
                                    indicies.Add(i);
                                    length.Add(new Vector2(sizeWidth, sizeLength));
                                    colors.Add(new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR),
                                        AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG),
                                        AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));
                                    normals.Add(terrainHit.normal);
                                    i++;

                                    if (origin == Vector3.zero)
                                    {
                                        lastPosition = hitPos;
                                    }
                                }
                            }
                        }
                    }
                }

                e.Use();
            }

            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 1)
            {
                Ray ray = scene.camera.ScreenPointToRay(mousePos);

                if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value))
                {
                    hitPos = terrainHit.point;
                    hitPosGizmo = hitPos;
                    hitNormal = terrainHit.normal;
                    for (int j = 0; j < positions.Count; j++)
                    {
                        Vector3 pos = positions[j];

                        pos += this.transform.position;
                        float dist = Vector3.Distance(terrainHit.point, pos);


                        if (dist <= brushSize)
                        {
                            positions.RemoveAt(j);
                            colors.RemoveAt(j);
                            normals.RemoveAt(j);
                            length.RemoveAt(j);
                            indicies.RemoveAt(j);
                            i--;
                            for (int i = 0; i < indicies.Count; i++)
                            {
                                indicies[i] = i;
                            }
                        }
                    }
                }

                e.Use();
            }

            //edit
            if (e.type == EventType.MouseDrag && e.button == 1 && toolbarInt == 2)
            {
                Ray ray = scene.camera.ScreenPointToRay(mousePos);

                if (Physics.Raycast(ray, out terrainHit, 200f, hitMask.value))
                {
                    hitPos = terrainHit.point;
                    hitPosGizmo = hitPos;
                    hitNormal = terrainHit.normal;
                    for (int j = 0; j < positions.Count; j++)
                    {
                        Vector3 pos = positions[j];

                        pos += this.transform.position;
                        float dist = Vector3.Distance(terrainHit.point, pos);


                        if (dist <= brushSize)
                        {
                            brushFalloffSize = Mathf.Clamp(brushFalloffSize, 0, brushSize);
                            float falloff = Mathf.Clamp01((dist - brushFalloffSize) / (brushSize - brushFalloffSize));

                            Color OrigColor = colors[j];

                            Color newCol = (new Color(AdjustedColor.r + (Random.Range(0, 1.0f) * rangeR),
                                AdjustedColor.g + (Random.Range(0, 1.0f) * rangeG),
                                AdjustedColor.b + (Random.Range(0, 1.0f) * rangeB), 1));

                            Vector2 origLength = length[j];
                            Vector2 newLength = new Vector2(sizeWidth, sizeLength);
                            ;

                            flowTimer++;
                            if (flowTimer > Flow)
                            {
                                if (toolbarIntEdit == 0 || toolbarIntEdit == 2)
                                {
                                    colors[j] = Color.Lerp(newCol, OrigColor, falloff);
                                }

                                if (toolbarIntEdit == 1 || toolbarIntEdit == 2)
                                {
                                    length[j] = Vector2.Lerp(newLength, origLength, falloff);
                                }

                                flowTimer = 0;
                            }
                        }
                    }
                }

                e.Use();
            }

            if (mesh != null)
            {
                DestroyImmediate(mesh);
            }

            mesh = new Mesh();
            mesh.SetVertices(positions);
            indi = indicies.ToArray();
            mesh.SetIndices(indi, MeshTopology.Points, 0);
            mesh.SetUVs(0, length);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);
            filter.mesh = mesh;
        }
    }
#endif
}