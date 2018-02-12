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

        public void OnEnable()
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


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(this.transform.localPosition, this.transform.localScale);
        }
    }
}