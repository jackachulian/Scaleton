using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.Rendering.Universal;


#if UNITY_EDITOR

[RequireComponent(typeof(CompositeCollider2D))]
public class TilemapLightCreator : MonoBehaviour {
    [Serializable]
    public class TileLightData {
        public Vector2 offset = Vector2.zero;
        public Color color = Color.white;
        public float intensity = 1f;
        public float innerRadius = 0f;
        public float outerRadius = 2f;
    }

    [SerializedDictionary("ID", "Audio Clips")]
    [SerializeField] private SerializedDictionary<Tile, TileLightData> tileLightDatas;
	private Tilemap tilemap;

    public void Create()
	{
		DestroyOldLights();
		tilemap = GetComponent<Tilemap>();

        BoundsInt bounds = tilemap.cellBounds;
        int totalLights = 0;

        for (int y = bounds.min.y; y < bounds.max.x; y++) {
            for (int x = bounds.min.x; x < bounds.max.x; x++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Tile tile = (Tile)tilemap.GetTile(pos);

                if (tile && tileLightDatas.ContainsKey(tile)) {
                    TileLightData data = tileLightDatas[tile];

                    GameObject obj = new GameObject();
                    obj.transform.parent = gameObject.transform;
                    obj.transform.position = pos + (Vector3)data.offset;
                    obj.name = "light_"+totalLights++;

                    Light2D light = obj.AddComponent<Light2D>();
                    light.color = data.color;
                    light.intensity = data.intensity;
                    light.pointLightInnerRadius = data.innerRadius;
                    light.pointLightOuterRadius = data.outerRadius;
                }
            }
        }
	}
	public void DestroyOldLights()
	{

		var tempList = transform.Cast<Transform>().ToList();
		foreach (var child in tempList)
		{
            if (child.GetComponent<Light2D>())
			DestroyImmediate(child.gameObject);
		}
	}
}

[CustomEditor(typeof(TilemapLightCreator))]
public class TilemapLightCreatorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Create"))
		{
			var creator = (TilemapLightCreator)target;
			creator.Create();
		}

		if (GUILayout.Button("Remove Lights"))
		{
			var creator = (TilemapLightCreator)target;
			creator.DestroyOldLights();
		}
		EditorGUILayout.EndHorizontal();
	}

}

#endif