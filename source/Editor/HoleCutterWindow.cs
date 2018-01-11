using System;
using UnityEditor;
using UnityEngine;

namespace NodeLevelEditor
{
    public class HoleCutterWindow:EditorWindow
    {
        public HoleCutterBehaviour cutter;
        public bool HasCutter
        {
            get { return this.cutter != null; }
        }
        public Vector3 CutterPos
        {
            get
            {
                return this.HasCutter ? this.cutter.transform.localPosition : Vector3.zero;
            }
            set
            {
                if (this.HasCutter)
                {
                    this.cutter.transform.localPosition = value;
                }
            }
        }
        public Vector3 CutterSca
        {
            get
            {
                return this.HasCutter ? this.cutter.transform.localScale : Vector3.zero;
            }
            set
            {
                if (this.HasCutter)
                {
                    this.cutter.transform.localScale = value;
                }
            }
        }

        public void OnGUI()
        {
            selectCutterGUI();
            newCutterGUI();
            cutterSizeGUI();
            cutHoleGUI();
        }

        private void selectCutterGUI()
        {
            GUI.enabled = this.HasCutter;
            if (GUILayout.Button("Select Cutter"))
            {
                Selection.activeGameObject = this.cutter.gameObject;
            }
            GUI.enabled = true;
        }

        private void newCutterGUI()
        {
            var buttonText = (this.HasCutter ? "Create New Cutter" : "Create Cutter");
            if (GUILayout.Button(buttonText))
            {
                if (this.HasCutter)
                {
                    DestroyImmediate(this.cutter.gameObject);
                }

                this.cutter = new GameObject("NLE_CUTTER").AddComponent<HoleCutterBehaviour>();
            }
        }
      
        private void cutterSizeGUI()
        {
            GUI.enabled = this.HasCutter;
            this.CutterPos = EditorGUILayout.Vector3Field("Position", this.CutterPos);
            this.CutterSca = EditorGUILayout.Vector3Field("Scale", this.CutterSca);
            GUI.enabled = true;
        }


        private void cutHoleGUI()
        {
            GUI.enabled = this.HasCutter;
            if (GUILayout.Button("Cut Hole"))
            {
                NodeHoleCutter.CutHoles(this.cutter);
            }
            GUI.enabled = true;
        }

        public void Update()
        {
            if (this.cutter == null)
            {
                this.cutter = FindObjectOfType<HoleCutterBehaviour>();
            }
        }

        public void OnDestroy()
        {
            if (this.cutter != null)
            {
                DestroyImmediate(this.cutter.gameObject);
            }
        }

        public static void ShowWindow()
        {
            var window = (HoleCutterWindow)GetWindow(typeof(HoleCutterWindow));
            window.minSize = new Vector2(50, 50);
            window.Show();
        }
    }
}
