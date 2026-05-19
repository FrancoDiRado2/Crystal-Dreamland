using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BosquePintor : EditorWindow
{
    public List<GameObject> prefabs = new List<GameObject>();
    bool modoPintar = false;

    [MenuItem("Tools/Pintor de Bosque")]
    public static void ShowWindow() => GetWindow<BosquePintor>("Pintor");

    void OnGUI()
    {
        GUILayout.Label("Configuración del Bosque", EditorStyles.boldLabel);
        
        // Lista de prefabs en la ventana
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("prefabs");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        
        // Botón para activar/desactivar
        string botonTexto = modoPintar ? "DESACTIVAR PINTOR" : "ACTIVAR PINTOR";
        if (GUILayout.Button(botonTexto)) {
            modoPintar = !modoPintar;
        }

        if (modoPintar) {
            EditorGUILayout.HelpBox("Shift + Clic en la Escena para colocar un objeto al azar.", MessageType.Info);
        }
    }

    void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    void OnSceneGUI(SceneView sceneView)
    {
        if (!modoPintar || prefabs.Count == 0) return;

        Event e = Event.current;
        // Detectamos Shift + Clic Izquierdo
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Elegir uno al azar de la lista
                GameObject elegido = prefabs[Random.Range(0, prefabs.Count)];
                
                if (elegido != null) {
                    // Instanciar como Prefab (para que mantenga el link)
                    GameObject nuevoObjeto = (GameObject)PrefabUtility.InstantiatePrefab(elegido);
                    nuevoObjeto.transform.position = hit.point;
                    
                    // Aleatoriedad para que sea natural
                    nuevoObjeto.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    float escalaAleatoria = Random.Range(0.8f, 1.3f);
                    nuevoObjeto.transform.localScale = Vector3.one * escalaAleatoria;

                    // Para poder hacer Ctrl+Z
                    Undo.RegisterCreatedObjectUndo(nuevoObjeto, "Pintar Arbol");
                    
                    e.Use(); // Consumir el evento para que no seleccione otras cosas
                }
            }
        }
    }
}