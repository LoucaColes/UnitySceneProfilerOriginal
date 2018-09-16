using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class CustomProfiler : EditorWindow
{
    public struct MeshData
    {
        public int triangleCount;
        public int verticesCount;
        public int polyCount;
        public int runtimeMemorySize;
    }

    public struct GeneralData
    {
        public int overallRuntimeMemorySize;
        public string tag;
        public int layer;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public int componentCount;
    }

    public struct LightData
    {
        public string type;
        public float intensity;
        public string shadowType;
        public string renderMode;
        public int cullingMask;
    }

    public struct ProfilerData
    {
        public int monoHeapSize;
        public int monoUsedSize;
        public int tempAllocatorSize;
        public int totalAllocatedMemory;
        public int totalReservedMemory;
        public int totalUnusedReservedMemory;
    }

    public struct ParticleSystemData
    {
        public bool collisionsEnabled;
        public float collisionBounceMultiplier;
        public float collisionBounceCurveMultiplier;
        public float collisionDampenMultiplier;
        public int collisionLayerMask;
        public bool dynamicColliders;
        public bool interiorCollisions;
        public int maxCollisionsShapes;
        public string collisionMode;
        public string collisionQuality;
        public float collisionRadiusScale;
        public string collisionType;
        public float collisionVoxelSize;

        public int burstCount;
        public float rateOverDistanceMultiplier;
        public float rateOverTimeMultiplier;

        public bool externalForces;
        public float externalForcesMultiplier;

        public bool isEmitting;
        public int particleCount;

    }

    public struct ColliderData
    {
        public string colliderType;
        public bool isTrigger;
        public Vector3 center;
        public Vector3 size;
        public float radius;
        public int direction;
        public float height;
        public bool convex;
        public bool inflateMesh;
        public float skinWidth;
    }

    public struct MaterialData
    {
        public int materialCount;
        public int materialsMemoryUsage;
    }

    public int m_triangleLimit, m_verticesLimit, m_polyLimit, m_componentCountLimit, m_overallRuntimeMemoryLimit;
    private bool m_displayMeshLimits = false, m_displayOptions = false;
    private bool m_ignoreLimits = true, m_displayGeneralData = true, m_displayProfilerData = true;
    private bool m_displayMeshData = true, m_displayLightData = true, m_displayColliderData = true;
    private bool m_displayParticleSystemData = true, m_displayMaterialData = true;

    public List<GameObject> m_gameObjects;
    private List<MeshData> m_objectsMeshData;
    private List<GeneralData> m_objectsGeneralData;
    private List<MaterialData> m_objectsMaterialData;
    private List<LightData> m_objectsLightData;
    private List<ColliderData> m_objectsColliderData;
    private List<ParticleSystemData> m_objectsParticleSystemData;
    private List<bool> m_objectFoldouts;
    ProfilerData profilerData;
    static EditorWindow profiler;

    private Vector2 m_scrollPos;
    static GUIStyle m_titleLabelStyle, m_subHeaderLabelStyle, m_warningStyle;

    [MenuItem("Window/Custom Profiler %#&P")]

    public static void ShowWindow()
    {

        Debug.Log("Custom Profiler Pressed");
        
        profiler = EditorWindow.GetWindowWithRect(typeof(CustomProfiler), new Rect(0, 0, 600, 800));
    }

    void OnEnable()
    {
        m_gameObjects = new List<GameObject>();
        m_objectsMeshData = new List<MeshData>();
        m_objectsGeneralData = new List<GeneralData>();
        m_objectsLightData = new List<LightData>();
        m_objectsColliderData = new List<ColliderData>();
        m_objectsParticleSystemData = new List<ParticleSystemData>();
        m_objectsMaterialData = new List<MaterialData>();
        m_objectFoldouts = new List<bool>();
        GUIStyleInit();
        GetProfilerData();
        GetObjects();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 600, 100), "Custom Profiler", m_titleLabelStyle);
        GUILayout.Space(30f);

        DisplayOptions();
        GUILayout.Space(10f);
        GUILayout.Space(10f);
        DisplayProfilerData();
        GUILayout.Space(10f);
        //GUI.Label(new Rect(0, 290, 600, 100), "Object Data", m_subHeaderLabelStyle);
        //GUILayout.Space(10f);
        m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
        DisplayObjectsData();
        EditorGUILayout.EndScrollView();
    }

    void Update()
    {

    }

    private void OnHierarchyChange()
    {
        Debug.Log("Hierarchy has changed");
        WipeLists();
        GetObjects();
    }

    private void GetProfilerData()
    {
        profilerData = new ProfilerData();
        profilerData.monoHeapSize = (int)Profiler.GetMonoHeapSize();
        profilerData.monoUsedSize = (int)Profiler.GetMonoUsedSize();
        profilerData.tempAllocatorSize = (int)Profiler.GetTempAllocatorSize();
        profilerData.totalAllocatedMemory = (int)Profiler.GetTotalAllocatedMemory();
        profilerData.totalReservedMemory = (int)Profiler.GetTotalReservedMemory();
        profilerData.totalUnusedReservedMemory = (int)Profiler.GetTotalUnusedReservedMemory();
    }

    private void DisplayProfilerData()
    {
        if (m_displayProfilerData)
        {
            EditorGUILayout.IntField("Mono Heap Size: ", profilerData.monoHeapSize);
            EditorGUILayout.IntField("Mono Used Size: ", profilerData.monoUsedSize);
            EditorGUILayout.IntField("Temp Allocator Size: ", profilerData.tempAllocatorSize);
            EditorGUILayout.IntField("Total Allocated Memory: ", profilerData.totalAllocatedMemory);
            EditorGUILayout.IntField("Total Reserved Memory: ", profilerData.totalReservedMemory);
            EditorGUILayout.IntField("Total Unused Reserved Memory: ", profilerData.totalUnusedReservedMemory);
        }
    }

    static void GUIStyleInit()
    {
        m_titleLabelStyle = new GUIStyle();
        m_titleLabelStyle.fontSize = 25;
        m_titleLabelStyle.alignment = TextAnchor.UpperCenter;
        m_titleLabelStyle.normal.textColor = Color.blue;

        m_subHeaderLabelStyle = new GUIStyle();
        m_subHeaderLabelStyle.fontSize = 15;
        m_subHeaderLabelStyle.alignment = TextAnchor.UpperCenter;
        m_subHeaderLabelStyle.normal.textColor = Color.blue;

        m_warningStyle = new GUIStyle();
        m_warningStyle.normal.textColor = Color.red;
    }

    void DisplayOptions()
    {

        if (GUI.Button(new Rect(1f, 30f, (profiler.maxSize.x - 5) /3, 50f), "Display Profiler Data"))
        {
            m_displayProfilerData = !m_displayProfilerData;
        }
        if (GUI.Button(new Rect(1f + ((profiler.maxSize.x -5) / 3), 30f, (profiler.maxSize.x - 5) / 3, 50f), "Display General Data"))
        {
            m_displayGeneralData = !m_displayGeneralData;
        }
        if (GUI.Button(new Rect(1f + (2* ((profiler.maxSize.x -5) / 3)), 30f, (profiler.maxSize.x - 5) / 3, 50f), "Display Mesh Data"))
        {
            m_displayMeshData = !m_displayMeshData;
        }
        if (GUI.Button(new Rect(1f, 80, (profiler.maxSize.x - 5) / 3, 50f), "Display Collider Data"))
        {
            m_displayColliderData = !m_displayColliderData;
        }
        if (GUI.Button(new Rect(1f + ((profiler.maxSize.x - 5) / 3), 80, (profiler.maxSize.x - 5) / 3, 50f), "Display Particle System Data"))
        {
            m_displayParticleSystemData = !m_displayParticleSystemData;
        }
        if (GUI.Button(new Rect(1f + (2 * ((profiler.maxSize.x - 5) / 3)), 80, (profiler.maxSize.x - 5) / 3, 50f), "Display Light Data"))
        {
            m_displayLightData = !m_displayLightData;
        }
        if (GUI.Button(new Rect(1f, 130, (profiler.maxSize.x - 5) / 3, 50f), "Display Material Data"))
        {
            m_displayMaterialData = !m_displayMaterialData;
        }

        if (GUI.Button(new Rect(1f + ((profiler.maxSize.x - 5) / 3), 130, (profiler.maxSize.x - 5) / 3, 50f), "Ignore all limits"))
        {
            m_ignoreLimits = !m_ignoreLimits;
        }

        GUILayout.Space(150);

        if (!m_ignoreLimits)
        {
            m_overallRuntimeMemoryLimit = EditorGUILayout.IntField("Overall Runtime Memory Limit", m_overallRuntimeMemoryLimit);
            m_triangleLimit = EditorGUILayout.IntField("Triangle Limit", m_triangleLimit);
            m_verticesLimit = EditorGUILayout.IntField("Vertices Limit", m_verticesLimit);
            m_polyLimit = EditorGUILayout.IntField("Polygon Limit", m_polyLimit);
            m_componentCountLimit = EditorGUILayout.IntField("Component Count Limit", m_componentCountLimit);
        }
    }

    void WipeLists()
    {
        m_gameObjects.Clear();
        m_objectsGeneralData.Clear();
        m_objectsMeshData.Clear();
        m_objectsLightData.Clear();
    }

    void CleanList()
    {
        for (int i = 0; i < m_gameObjects.Count; i++)
        {
            if (m_gameObjects[i] == null)
            {
                m_gameObjects.RemoveAt(i);
            }
        }
    }

    void GetObjects()
    {
        Debug.Log("Getting All Objects");
        GameObject[] t_gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < t_gameObjects.Length; i++)
        {
            m_gameObjects.Add(t_gameObjects[i]);
            m_objectFoldouts.Add(false);
            GetGeneralData(i);
            if (m_gameObjects[i].GetComponent<MeshFilter>())
            {
                GetMeshData(i);
            }
            if (m_gameObjects[i].GetComponent<Renderer>())
            {
                GetMaterialData(i);
            }
            if (m_gameObjects[i].GetComponent<Light>())
            {
                GetLightData(i);
            }
            if (m_gameObjects[i].GetComponent<Collider>())
            {
                GetColliderData(i);
            }
            if (m_gameObjects[i].GetComponent<ParticleSystem>())
            {
                GetParticleSystemData(i);
            }
        }
    }

    void GetGeneralData(int _index)
    {
        GeneralData m_generalData = new GeneralData();
        m_generalData.tag = m_gameObjects[_index].tag;
        m_generalData.layer = m_gameObjects[_index].layer;
        m_generalData.position = m_gameObjects[_index].transform.position;
        m_generalData.rotation = m_gameObjects[_index].transform.rotation.eulerAngles;
        m_generalData.scale = m_gameObjects[_index].transform.localScale;
        Component[] tempComponents = m_gameObjects[_index].GetComponents<Component>();
        m_generalData.componentCount = tempComponents.Length;
        m_generalData.overallRuntimeMemorySize = Profiler.GetRuntimeMemorySize(m_gameObjects[_index]);
        m_objectsGeneralData.Add(m_generalData);
    }

    void GetMeshData(int _index)
    {
        MeshData m_meshData = new MeshData();
        m_meshData.triangleCount = GetTriCount(_index);
        m_meshData.verticesCount = GetVertCount(_index);
        m_meshData.polyCount = GetPolyCount(_index);
        m_meshData.runtimeMemorySize = Profiler.GetRuntimeMemorySize(m_gameObjects[_index].GetComponent<MeshFilter>());
        m_objectsMeshData.Add(m_meshData);
    }

    void GetLightData(int _index)
    {
        LightData lightData = new LightData();
        lightData.type = m_gameObjects[_index].GetComponent<Light>().type.ToString();
        lightData.intensity = m_gameObjects[_index].GetComponent<Light>().intensity;
        lightData.shadowType = m_gameObjects[_index].GetComponent<Light>().shadows.ToString();
        lightData.cullingMask = m_gameObjects[_index].GetComponent<Light>().cullingMask;
        lightData.renderMode = m_gameObjects[_index].GetComponent<Light>().renderMode.ToString();
        m_objectsLightData.Add(lightData);
    }

    void GetMaterialData(int _index)
    {
        MaterialData materialData = new MaterialData();
        materialData.materialCount = m_gameObjects[_index].GetComponent<Renderer>().materials.Length;
        Material[] materials = m_gameObjects[_index].GetComponent<Renderer>().materials;
        materialData.materialsMemoryUsage = Profiler.GetRuntimeMemorySize(materials[0]);

        m_objectsMaterialData.Add(materialData);
    }

    void GetParticleSystemData(int _index)
    {
        ParticleSystemData particleSystemData = new ParticleSystemData();
        ParticleSystem particleSystem;
        particleSystem = m_gameObjects[_index].GetComponent<ParticleSystem>();
        particleSystemData.burstCount = particleSystem.emission.burstCount;
        particleSystemData.collisionBounceMultiplier = particleSystem.collision.bounceMultiplier;
        particleSystemData.collisionDampenMultiplier = particleSystem.collision.dampenMultiplier;
        particleSystemData.collisionLayerMask = particleSystem.collision.collidesWith;
        particleSystemData.collisionMode = particleSystem.collision.mode.ToString();
        particleSystemData.collisionQuality = particleSystem.collision.quality.ToString();
        particleSystemData.collisionRadiusScale = particleSystem.collision.radiusScale;
        particleSystemData.collisionsEnabled = particleSystem.collision.enabled;
        particleSystemData.collisionType = particleSystem.collision.type.ToString();
        particleSystemData.collisionVoxelSize = particleSystem.collision.voxelSize;
        particleSystemData.dynamicColliders = particleSystem.collision.enableDynamicColliders;
        particleSystemData.externalForces = particleSystem.externalForces.enabled;
        particleSystemData.externalForcesMultiplier = particleSystem.externalForces.multiplier;
        particleSystemData.interiorCollisions = particleSystem.collision.enableInteriorCollisions;
        particleSystemData.isEmitting = particleSystem.isEmitting;
        particleSystemData.maxCollisionsShapes = particleSystem.collision.maxCollisionShapes;
        particleSystemData.particleCount = particleSystem.particleCount;
        particleSystemData.rateOverDistanceMultiplier = particleSystem.emission.rateOverDistanceMultiplier;
        particleSystemData.rateOverTimeMultiplier = particleSystem.emission.rateOverTimeMultiplier;
        particleSystem = null;
        m_objectsParticleSystemData.Add(particleSystemData);
    }

    void GetColliderData(int _index)
    {
        ColliderData colliderData = new ColliderData();
        if (m_gameObjects[_index].GetComponent<Collider>().GetType() == typeof(BoxCollider))
        {
            colliderData.colliderType = "Box Collider";
            colliderData.isTrigger = m_gameObjects[_index].GetComponent<BoxCollider>().isTrigger;
            colliderData.center = m_gameObjects[_index].GetComponent<BoxCollider>().center;
            colliderData.size = m_gameObjects[_index].GetComponent<BoxCollider>().size;
        }
        if (m_gameObjects[_index].GetComponent<Collider>().GetType() == typeof(SphereCollider))
        {
            colliderData.colliderType = "Sphere Collider";
            colliderData.isTrigger = m_gameObjects[_index].GetComponent<SphereCollider>().isTrigger;
            colliderData.center = m_gameObjects[_index].GetComponent<SphereCollider>().center;
            colliderData.radius = m_gameObjects[_index].GetComponent<SphereCollider>().radius;
        }
        if (m_gameObjects[_index].GetComponent<Collider>().GetType() == typeof(CapsuleCollider))
        {
            colliderData.colliderType = "Capsule Collider";
            colliderData.isTrigger = m_gameObjects[_index].GetComponent<CapsuleCollider>().isTrigger;
            colliderData.center = m_gameObjects[_index].GetComponent<CapsuleCollider>().center;
            colliderData.radius = m_gameObjects[_index].GetComponent<CapsuleCollider>().radius;
            colliderData.direction = m_gameObjects[_index].GetComponent<CapsuleCollider>().direction;
            colliderData.height = m_gameObjects[_index].GetComponent<CapsuleCollider>().height;
        }
        if (m_gameObjects[_index].GetComponent<Collider>().GetType() == typeof(MeshCollider))
        {
            colliderData.colliderType = "Mesh Collider";
            colliderData.isTrigger = m_gameObjects[_index].GetComponent<MeshCollider>().isTrigger;
            colliderData.inflateMesh = m_gameObjects[_index].GetComponent<MeshCollider>().inflateMesh;
            colliderData.convex = m_gameObjects[_index].GetComponent<MeshCollider>().convex;
            colliderData.skinWidth = m_gameObjects[_index].GetComponent<MeshCollider>().skinWidth;
        }
        m_objectsColliderData.Add(colliderData);
    }

    int GetVertCount(int _index)
    {
        return m_gameObjects[_index].gameObject.GetComponent<MeshFilter>().sharedMesh.vertexCount;
    }

    int GetPolyCount(int _index)
    {
        return (int)m_gameObjects[_index].gameObject.GetComponent<MeshFilter>().sharedMesh.triangles.Length / 3;
    }

    int GetTriCount(int _index)
    {
        return m_gameObjects[_index].gameObject.GetComponent<MeshFilter>().sharedMesh.triangles.Length;
    }

    void DisplayObjectsData()
    {
        Debug.Log("Count: " + m_objectFoldouts.Count);
        if (m_gameObjects.Count > 0)
        {
            int meshCount = 0;
            int lightCount = 0;
            int colliderCount = 0;
            int particleSystemCount = 0;
            int materialCount = 0;
            int foldoutCount = 0;
            for (int i = 0; i < m_gameObjects.Count; i++)
            {
                Debug.Log("index: "+ i);
                if (m_gameObjects[i].gameObject != null)
                {
                    m_objectFoldouts[foldoutCount] = EditorGUILayout.Foldout(m_objectFoldouts[foldoutCount], m_gameObjects[i].name);
                    if (m_objectFoldouts[foldoutCount])
                    {
                        if (m_displayGeneralData)
                        {
                            DisplayGeneralData(i);
                        }
                        if (m_displayMeshData)
                        {
                            if (m_gameObjects[i].GetComponent<MeshFilter>())
                            {
                                DisplayMeshData(meshCount);
                                meshCount++;
                            }
                        }
                        if (m_displayLightData)
                        {
                            if (m_gameObjects[i].GetComponent<Light>())
                            {
                                DisplayLightData(lightCount);
                                lightCount++;
                            }
                        }
                        if (m_displayColliderData)
                        {
                            if (m_gameObjects[i].GetComponent<Collider>())
                            {
                                DisplayColliderData(colliderCount);
                                colliderCount++;
                            }
                        }
                        if (m_displayParticleSystemData)
                        {
                            if (m_gameObjects[i].GetComponent<ParticleSystem>())
                            {
                                DisplayParticleSystemData(particleSystemCount);
                                particleSystemCount++;
                            }
                        }
                        if (m_displayMaterialData)
                        {
                            if (m_gameObjects[i].GetComponent<Renderer>())
                            {
                                DisplayMaterialData(materialCount);
                                materialCount++;
                            }
                        }
                    }
                    foldoutCount++;
                }
            }
        }
    }

    void DisplayGeneralData(int _index)
    {
        int tempOverallRuntimeMemoryUsage = m_objectsGeneralData[_index].overallRuntimeMemorySize;
        string tempTag = m_objectsGeneralData[_index].tag;
        int tempLayer = m_objectsGeneralData[_index].layer;
        Vector3 tempPos = m_objectsGeneralData[_index].position;
        Vector3 tempRot = m_objectsGeneralData[_index].rotation;
        Vector3 tempScale = m_objectsGeneralData[_index].scale;
        int tempCompCount = m_objectsGeneralData[_index].componentCount;

        if (!m_ignoreLimits && (tempOverallRuntimeMemoryUsage > m_overallRuntimeMemoryLimit))
        {
            EditorGUILayout.IntField("Overall Runtime Memory Size: ", tempOverallRuntimeMemoryUsage, m_warningStyle);
            EditorGUILayout.HelpBox("Warning Object has a lot of Runtime Memory Usage", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.IntField("Overall Runtime Memory Size: ", tempOverallRuntimeMemoryUsage);
        }
        EditorGUILayout.TextField("Tag: ", tempTag);
        EditorGUILayout.IntField("Layer: ", tempLayer);
        EditorGUILayout.Vector3Field("Position: ", tempPos);
        EditorGUILayout.Vector3Field("Rotation: ", tempRot);
        EditorGUILayout.Vector3Field("Scale ", tempScale);
        if (!m_ignoreLimits && (tempCompCount > m_componentCountLimit))
        {
            EditorGUILayout.IntField("Num of Components: ", tempCompCount, m_warningStyle);
            EditorGUILayout.HelpBox("Warning Object has a lot of Components", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.IntField("Num of Components: ", tempCompCount);
        }
    }

    void DisplayMeshData(int _index)
    {
        int tempVertex = m_objectsMeshData[_index].verticesCount;
        int tempTriangleCount = m_objectsMeshData[_index].triangleCount;
        int tempPolyCount = m_objectsMeshData[_index].polyCount;
        int tempRuntimeMemorySize = m_objectsMeshData[_index].runtimeMemorySize;
        EditorGUILayout.IntField("Runtime Memory Size: ", tempRuntimeMemorySize);
        if (tempVertex > 0)
        {
            if (!m_ignoreLimits)
            {
                if (tempVertex > m_verticesLimit)
                {
                    EditorGUILayout.IntField("Vertices Count: ", tempVertex, m_warningStyle);
                    EditorGUILayout.HelpBox("Warning Object has too many vertices", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.IntField("Vertex Count: ", tempVertex);
                }
            }
            else
            {
                EditorGUILayout.IntField("Vertex Count: ", tempVertex);
            }

        }
        if (tempTriangleCount > 0)
        {
            if (!m_ignoreLimits)
            {
                if (tempTriangleCount > m_triangleLimit)
                {
                    EditorGUILayout.IntField("Triangle Count: ", tempTriangleCount, m_warningStyle);
                    EditorGUILayout.HelpBox("Warning Object has too many triangles", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.IntField("Triangle Count: ", tempTriangleCount);
                }
            }
            else
            {
                EditorGUILayout.IntField("Triangle Count: ", tempTriangleCount);
            }
        }
        if (tempPolyCount > 0)
        {
            if (!m_ignoreLimits)
            {
                if (tempPolyCount > m_polyLimit)
                {
                    EditorGUILayout.IntField("Polygon Count: ", tempPolyCount, m_warningStyle);
                    EditorGUILayout.HelpBox("Warning Object has too many polygons", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.IntField("Polygon Count: ", tempPolyCount);
                }
            }
            else
            {
                EditorGUILayout.IntField("Polygon Count: ", tempPolyCount);
            }
        }
        
    }

    void DisplayColliderData(int _index)
    {
        EditorGUILayout.TextField("Collider Type: ", m_objectsColliderData[_index].colliderType);
        EditorGUILayout.Toggle("Is Trigger: ", m_objectsColliderData[_index].isTrigger);
        if (m_objectsColliderData[_index].colliderType != "Mesh Collider")
        {
            EditorGUILayout.Vector3Field("Center: ", m_objectsColliderData[_index].center);
        }
        if (m_objectsColliderData[_index].colliderType == "Box Collider")
        {
            EditorGUILayout.Vector3Field("Size: ", m_objectsColliderData[_index].size);
        }
        if (m_objectsColliderData[_index].colliderType == "Sphere Collider" || m_objectsColliderData[_index].colliderType == "Capsule Collider")
        {
            EditorGUILayout.FloatField("Radius: ", m_objectsColliderData[_index].radius);
        }
        if (m_objectsColliderData[_index].colliderType == "Capsule Collider")
        {
            EditorGUILayout.IntField("Direction: ", m_objectsColliderData[_index].direction);
        }
        if (m_objectsColliderData[_index].colliderType == "Capsule Collider")
        {
            EditorGUILayout.FloatField("Height: ", m_objectsColliderData[_index].height);
        }
        if (m_objectsColliderData[_index].colliderType == "Mesh Collider")
        {
            EditorGUILayout.Toggle("Inflate Mesh: ", m_objectsColliderData[_index].inflateMesh);
        }
        if (m_objectsColliderData[_index].colliderType == "Mesh Collider")
        {
            EditorGUILayout.Toggle("Convex: ", m_objectsColliderData[_index].convex);
        }
        if (m_objectsColliderData[_index].colliderType == "Mesh Collider")
        {
            EditorGUILayout.FloatField("Skin Width: ", m_objectsColliderData[_index].skinWidth);
        }
    }

    void DisplayLightData(int _index)
    {
        string tempType = m_objectsLightData[_index].type;
        float tempIntensity = m_objectsLightData[_index].intensity;
        string tempShadowType = m_objectsLightData[_index].shadowType;
        string tempRenderMode = m_objectsLightData[_index].renderMode;
        int tempCullingMask = m_objectsLightData[_index].cullingMask;

        EditorGUILayout.TextField("Light Type: ", tempType);
        EditorGUILayout.FloatField("Intensity: ", tempIntensity);
        EditorGUILayout.TextField("Shadow Type: ", tempShadowType);
        EditorGUILayout.TextField("Render Mode: ", tempRenderMode);
        EditorGUILayout.IntField("Culling Mask: ", tempCullingMask);
    }

    void DisplayMaterialData(int _index)
    {
        int materialCount = m_objectsMaterialData[_index].materialCount;
        int materialDataUsage = m_objectsMaterialData[_index].materialsMemoryUsage;

        EditorGUILayout.IntField("Materials Count: ", materialCount);
        EditorGUILayout.IntField("Material " + " Mem Usage: ", materialDataUsage);
    }

    void DisplayParticleSystemData(int _index)
    {
        EditorGUILayout.Toggle("Collisions Enabled: ", m_objectsParticleSystemData[_index].collisionsEnabled);
        EditorGUILayout.FloatField("Bounce Multiplier: ", m_objectsParticleSystemData[_index].collisionBounceMultiplier);
        EditorGUILayout.FloatField("Bounce Curve Multiplier: ", m_objectsParticleSystemData[_index].collisionBounceCurveMultiplier);
        EditorGUILayout.FloatField("Bounce Dampen Multiplier: ", m_objectsParticleSystemData[_index].collisionDampenMultiplier);
        EditorGUILayout.IntField("Collision Layer Mask: ", m_objectsParticleSystemData[_index].collisionLayerMask);
        EditorGUILayout.Toggle("Dynamic Colliders: ", m_objectsParticleSystemData[_index].dynamicColliders);
        EditorGUILayout.Toggle("Interior Collision: ", m_objectsParticleSystemData[_index].interiorCollisions);
        EditorGUILayout.IntField("Max Collision: ", m_objectsParticleSystemData[_index].collisionLayerMask);
        EditorGUILayout.TextField("Collision Mode: ", m_objectsParticleSystemData[_index].collisionMode);
        EditorGUILayout.TextField("Collision Quality: ", m_objectsParticleSystemData[_index].collisionQuality);
        EditorGUILayout.FloatField("Collision Radius Scale: ", m_objectsParticleSystemData[_index].collisionRadiusScale);
        EditorGUILayout.TextField("Collision Type: ", m_objectsParticleSystemData[_index].collisionType);
        EditorGUILayout.FloatField("Collision Voxel Size: ", m_objectsParticleSystemData[_index].collisionVoxelSize);
        EditorGUILayout.IntField("Burst Count: ", m_objectsParticleSystemData[_index].burstCount);
        EditorGUILayout.FloatField("Rate Over Dist Multiplier: ", m_objectsParticleSystemData[_index].rateOverDistanceMultiplier);
        EditorGUILayout.FloatField("Rate Over Time Multiplier: ", m_objectsParticleSystemData[_index].rateOverTimeMultiplier);
        EditorGUILayout.Toggle("External Forces: ", m_objectsParticleSystemData[_index].externalForces);
        EditorGUILayout.FloatField("External Forces Multiplier: ", m_objectsParticleSystemData[_index].externalForcesMultiplier);
        EditorGUILayout.Toggle("Is Emitting: ", m_objectsParticleSystemData[_index].isEmitting);
        EditorGUILayout.IntField("Particle Count: ", m_objectsParticleSystemData[_index].particleCount);
    }
}
