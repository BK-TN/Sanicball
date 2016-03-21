using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sanicball
{
    [CustomEditor(typeof(AIPath))]
    public class AIPathEditor : Editor
    {
        private ReorderableList list;

        private SerializedProperty nodesProp;

        private int selectedNode = -1;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            nodesProp = serializedObject.FindProperty("nodes");

            list = new ReorderableList(serializedObject, nodesProp, true, true, true, true);

            list.drawHeaderCallback += rect => EditorGUI.LabelField(rect, "Path nodes");

            list.drawElementCallback += (rect, index, active, focused) =>
            {
                rect.height = 16;
                rect.y += 2;
                EditorGUI.PropertyField(rect,
                                        list.serializedProperty.GetArrayElementAtIndex(index),
                                        GUIContent.none);
            };

            list.onSelectCallback += (l) =>
            {
                selectedNode = list.index;
            };
            list.onChangedCallback += (l) =>
            {
                selectedNode = list.index;
            };
            /*list.onAddCallback += (l) => {
                Debug.Log("Added");
            };
            list.onRemoveCallback += (l) => {
                Debug.Log("Removed");
            };*/
        }

        private void OnSceneGUI()
        {
            List<Vector3> nodes = (serializedObject.targetObject as AIPath).nodes;
            //Node labels
            GUIStyle style = new GUIStyle();
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.background = Texture2D.whiteTexture;
            style.normal.textColor = new Color(0.2f, 0.4f, 1f);
            for (int i = 0; i < nodes.Count; i++)
            {
                Handles.Label(nodes[i], "Node " + i, style);
            }
            //Selected node movement handle
            if (selectedNode >= 0 && selectedNode < nodes.Count)
            {
                nodes[selectedNode] = Handles.PositionHandle(nodes[selectedNode], Quaternion.identity);
            }
        }

        private void OnDisable()
        {
        }

        /*public SerializedProperty testProperty;

        List<AINode> someList = new List<AINode>();

        private ReorderableListControl listControl;
        private IReorderableListAdaptor listAdaptor;

        void OnEnable() {
            testProperty = serializedObject.FindProperty("nodes");

            // Create list control and optionally pass flags into constructor.
            listControl = new ReorderableListControl();

            // Subscribe to events for item insertion and removal.
            listControl.ItemInserted += OnItemInserted;
            listControl.ItemRemoving += OnItemRemoving;

            // Create adaptor for example list.
            listAdaptor = new GenericListAdaptor(someList);
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            ReorderableListGUI.ListField(testProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            // Unsubscribe from events, good practice.
            if (listControl != null) {
                listControl.ItemInserted -= OnItemInserted;
                listControl.ItemRemoving -= OnItemRemoving;
            }
        }

        private void OnItemInserted(Object sender, ItemInsertedEventArgs args) {
            AINode item = someList[args.itemIndex];
            if (args.wasDuplicated)
                Debug.Log("Duplicated: " + item);
            else
                Debug.Log("Inserted: " + item);
        }

        private void OnItemRemoving(Object sender, ItemRemovingEventArgs args) {
            AINode item = someList[args.itemIndex];
            Debug.Log("Removing: " + item);

            // You can cancel item removal at this stage!
            //if (item == "Keep Me!")
            //	args.Cancel = true;
        }*/
    }
}
