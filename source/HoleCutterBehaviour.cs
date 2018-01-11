using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeLevelEditor
{
    [ExecuteInEditMode]
    public class HoleCutterBehaviour : MonoBehaviour
    {
        private static HoleCutterBehaviour _instance;
        private static HoleCutterBehaviour Instance
        {
            get
            {
                return _instance;
            }
        }

        public BoxCollider boxCollider;

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                DestroyImmediate(this.gameObject);
            }
        }

        public void Update()
        {
            if (this.boxCollider == null)
            {
                this.boxCollider = this.GetComponent<BoxCollider>();

                if (this.boxCollider == null)
                {
                    this.boxCollider = this.gameObject.AddComponent<BoxCollider>();
                }
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(this.transform.localPosition, this.transform.localScale);
        }
    }
}