using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace TOZ {

	public static class _MeshUtil {

		public static Vector3[] _SmoothNormals(Vector3[] vertices, Vector3[] normals) {
			if(vertices == null || normals == null)
				return null;

			List<int> openList = new List<int>();
			List<int> closeList = new List<int>();

			Vector3 temp = Vector3.zero;

			//Smooth overlapping vertex normals
			for(int i = 0; i < vertices.Length; i++) {
				if(closeList.Contains(i))
					continue;

				temp = normals[i].normalized;
				openList.Add(i);

				for(int j = 0; j < vertices.Length; j++) {
					if(i == j)
						continue;

					if(closeList.Contains(j))
						continue;

					if(vertices[i] == vertices[j]) {
						temp += normals[j].normalized;
						openList.Add(j);
					}
				}

				for(int k = 0; k < openList.Count; k++) {
					normals[openList[k]] = temp;
					closeList.Add(openList[k]);
				}
				openList.Clear();
			}
			closeList.Clear();

			//Normalize normals
			for(int i = 0; i < normals.Length; i++) {
				normals[i].Normalize();
			}

			return normals;
		}

		public static void _SmoothNormals(Mesh m) {
			m.normals = _SmoothNormals(m.vertices, m.normals);
		}

		public static Vector3[] _CalculateNormals(Vector3[] vertices, int[] triangles) {
			Vector3[] normals = new Vector3[vertices.Length];
			//Normals
			for(int i = 0; i < triangles.Length / 3; i++) {
				int v1 = triangles[i * 3];
				int v2 = triangles[i * 3 + 1];
				int v3 = triangles[i * 3 + 2];
				Vector3 ev1 = vertices[v2] - vertices[v1];
				Vector3 ev2 = vertices[v3] - vertices[v1];
				Vector3 normal = Vector3.Cross(ev1, ev2).normalized;
				normals[v1] += normal;
				normals[v2] += normal;
				normals[v3] += normal;
			}

			//Normalize normals
			for(int i = 0; i < normals.Length; i++) {
				normals[i].Normalize();
			}

			return normals;
		}

		public static Vector3[] _CalculateNormals(List<Vector3> vertices, List<int> triangles) {
			return _CalculateNormals(vertices.ToArray(), triangles.ToArray());
		}

		public static void _CalculateNormals(Mesh m) {
			m.normals = _CalculateNormals(m.vertices, m.triangles);
		}

		public static Vector4[] _CalculateTangents(Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uv) {
			Vector4[] tangents = new Vector4[vertices.Length];
			//Tangents
			Vector3[] t1 = new Vector3[vertices.Length];
			Vector3[] t2 = new Vector3[vertices.Length];
			
			for(int i = 0; i < triangles.Length / 3; i++) {
				int v1 = triangles[i * 3];
				int v2 = triangles[i * 3 + 1];
				int v3 = triangles[i * 3 + 2];
				Vector3 ev1 = vertices[v2] - vertices[v1];
				Vector3 ev2 = vertices[v3] - vertices[v1];
				Vector2 eu1 = uv[v2] - uv[v1];
				Vector2 eu2 = uv[v3] - uv[v1];
				
				float r = 1.0f / (eu1.x * eu2.y - eu2.x * eu1.y);
				Vector3 sDir = new Vector3 ((eu2.y * ev1.x - eu1.y * ev2.x) * r,
														(eu2.y * ev1.y - eu1.y * ev2.y) * r,
														(eu2.y * ev1.z - eu1.y * ev2.z) * r);
				Vector3 tDir = new Vector3 ((eu1.x * ev2.x - eu2.x * ev1.x) * r,
														(eu1.x * ev2.y - eu2.x * ev1.y) * r,
														(eu1.x * ev2.z - eu2.x * ev1.z) * r);
				t1[v1] += sDir; t1[v2] += sDir; t1[v3] += sDir;
				t2[v1] += tDir; t2[v2] += tDir; t2[v3] += tDir;
			}

			for(int i = 0; i < vertices.Length; i++) {
				// Gram-Schmidt orthogonalize
				Vector3.OrthoNormalize(ref normals[i], ref t1[i]);
				tangents[i].x = t1[i].x;
				tangents[i].y = t1[i].y;
				tangents[i].z = t1[i].z;
				tangents[i].w = (Vector3.Dot(Vector3.Cross(normals[i], t1[i]), t2[i]) < 0.0f) ? -1.0f : 1.0f;
			}

			return tangents;
		}

		public static Vector4[] _CalculateTangents(List<Vector3> vertices, List<int> triangles, Vector3[] normals, List<Vector2> uv) {
			return _CalculateTangents(vertices.ToArray(), triangles.ToArray(), normals, uv.ToArray());
		}

		public static void _CalculateTangents(Mesh m) {
			m.tangents = _CalculateTangents(m.vertices, m.triangles, m.normals, m.uv);
		}

		//Uses reflection
		public static Mesh _CreateCopy(Mesh m) {
			return (Mesh)UnityEngine.Object.Instantiate(m);
			/*
			Mesh mesh = new Mesh();

			foreach(PropertyInfo prop in typeof(Mesh).GetProperties()) {
				if(prop.GetSetMethod() != null && prop.GetGetMethod() != null) {
					prop.SetValue(mesh, prop.GetValue(m, null), null);
				}
			}*/
/*
			for(int i = 0; i < m.subMeshCount; i++) {
				SubMeshDescriptor desc = m.GetSubMesh(i);
				//mesh.SetSubMesh(i, desc);
			}*/
			//Debug.Log(mesh.subMeshCount);
			//return mesh;
		}
	}

}