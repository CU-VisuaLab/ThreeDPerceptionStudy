﻿using UnityEngine;

namespace ProceduralToolkit.Examples.Primitives
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Pyramid : MonoBehaviour
    {
        public float radius = 1f;
        public int segments = 16;
        public float height = 1f;

        private void Start()
        {
            GetComponent<MeshFilter>().mesh = MeshE.Pyramid(radius, segments, height);
        }

        public void UpdateMeshHeight(float h)
        {
            height = h;
            GetComponent<MeshFilter>().mesh = MeshE.Pyramid(radius, segments, height);
        }

        public void UpdateMeshRadius(float r)
        {
            radius = r;
            GetComponent<MeshFilter>().mesh = MeshE.Pyramid(radius, segments, height);
        }
    }
}