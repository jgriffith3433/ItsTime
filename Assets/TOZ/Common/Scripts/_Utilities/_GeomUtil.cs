using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOZ {

	public static class _GeomUtil {
		//Are 2 points on the same side of a line
		public static bool _ArePointsOnSameSideOfLine(Vector3 p0, Vector3 p1, Vector3 l0, Vector3 l1) {
			bool res = false;
			Vector3 cp0 = Vector3.Cross(_DirUtil._Get3D(l0, l1), _DirUtil._Get3D(l0, p0));
			Vector3 cp1 = Vector3.Cross(_DirUtil._Get3D(l0, l1), _DirUtil._Get3D(l0, p1));
			if(Vector3.Dot(cp0, cp1) >= Mathf.Epsilon)
				res = true;
			return res;
		}

		//Is point inside a triangle
		public static bool _IsPointInsideTriangle(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2) {
			return
				_ArePointsOnSameSideOfLine(p, v0, v1, v2) &&
				_ArePointsOnSameSideOfLine(p, v1, v0, v2) &&
				_ArePointsOnSameSideOfLine(p, v2, v0, v1);
		}

		//Is point inside a rectangle
		public static bool _IsPointInsideRectangle(Vector2 p, float x0, float y0, float x1, float y1) {
			bool res = false;
			if((p.x >= x0 && p.x <= x1) && (p.y >= y0 && p.y <= y1)) {
				res = true;
			}
			return res;
		}

		//Is point inside a box
		public static bool IsPointInsideBox(Vector3 p, float x0, float y0, float z0, float x1, float y1, float z1) {
			bool res = false;
			if((p.x >= x0 && p.x <= x1) && (p.y >= y0 && p.y <= y1) && (p.z >= z0 && p.z <= z1)) {
				res = true;
			}
			return res;
		}

		//Is point inside a circle
		public static bool _IsPointInsideCircle(Vector2 p, Vector2 center, float radius) {
			return _DistUtil._Get2D(p, center) <= radius;
		}

		//Is point inside a sphere
		public static bool _IsPointInsideSphere(Vector3 p, Vector3 center, float radius) {
			return _DistUtil._Get3D(p, center) <= radius;
		}

		//Is point inside a 2d polygon
		public static bool _IsPointInsidePolygon2D(Vector2 p, Vector2[] vertices) {
			bool res = false;
			int size = vertices.Length;
			if(size < 3)
				return res;
			Vector2 p1, p2;
			Vector2 oldV = new Vector2(vertices[size-1].x, vertices[size-1].y);
			for(int i = 0; i < size; i++) {
				Vector2 newV = new Vector2(vertices[i].x, vertices[i].y);
				if(newV.x > oldV.x) {
					p1 = oldV;
					p2 = newV;
				}
				else {
					p1 = newV;
					p2 = oldV;
				}
				if((newV.x < p.x) == (p.x <= oldV.x)) {
					if((p.y - p1.y) * (p2.x - p1.x) < (p2.y - p1.y) * (p.x - p1.x)) {
						res = !res;
					}
				}
				oldV = newV;
			}
			return res;
		}

		//Is line intersecting a sphere
		public static bool _IsLineIntersectingSphere(Vector3 l0, Vector3 l1, Vector3 center, float radius) {
			Vector3 v0 = _DirUtil._Get3D(center, l0);
			Vector3 v1 = _DirUtil._Get3D(center, l1);
			float lenSqr = _DistUtil._Get3DSqr(v0, v1);
			float det = Vector3.Cross(v0, v1).sqrMagnitude;
			float discriminant = (2f * radius) * lenSqr - det;
			if(discriminant < 0)
				return false;
			else
				return true;
		}

		//Plane equation from 3 points
		public static Vector4 _GetPlaneEquation(Vector3 v0, Vector3 v1, Vector3 v2) {
			//Create two vectors from triangle representing the plane
			Vector3 AB = _DirUtil._Get3DNormalized(v0, v1);
			Vector3 AC = _DirUtil._Get3DNormalized(v0, v2);
			//Calculate normal
			Vector3 normal = Vector3.Cross(AB, AC).normalized;
			//Get plane
			Vector4 plane;
			plane.x = normal.x;
			plane.y = normal.y;
			plane.z = normal.z;
			plane.w = -Vector3.Dot(normal, v0);
			return plane;
		}

		//Plane equation from a normal and a position
		public static Vector4 _GetPlaneEquation(Vector3 normal, Vector3 position) {
			float d = -Vector3.Dot(normal, position);
			return new Vector4(normal.x, normal.y, normal.z, d);
		}

		//Get which side of a plane are we on
		public static float _GetSide(Transform trs, Vector3 pos) {
			return _GetSide(trs.up, trs.position, pos);
		}

		//Get which side of a plane are we on
		public static float _GetSide(Vector3 normal, Vector3 position, Vector3 pos) {
			Vector4 plane = _GetPlaneEquation(normal, position);
			return _GetSide(plane, pos);
		}

		//Get which side of a plane are we on
		public static float _GetSide(Vector4 plane, Vector3 pos) {
			//return _MathUtil._Sign(plane.x * pos.x + plane.y * pos.y + plane.z * pos.z + plane.w);
			return Mathf.Sign(plane.x * pos.x + plane.y * pos.y + plane.z * pos.z + plane.w);
		}

		//Distance and intersection of point to a line
		public static float _GetPointToLineDistance(Vector3 p, Vector3 l0, Vector3 l1, ref Vector3 intersection) {
			float lenSqr = _DistUtil._Get3DSqr(l0, l1);
			float u = (	((p.x - l0.x) * (l1.x - l0.x)) +
						((p.y - l0.y) * (l1.y - l0.y)) +
						((p.z - l0.z) * (l1.z - l0.z))) / lenSqr;
			//If no U, it doesnt intersect thus returning a negative number
			if(u < 0.0f || u > 1.0f) {
				//intersection = point;
				return -1f;
			}
			intersection = l0 + u * (l1 - l0);
			return (p - intersection).magnitude;
		}

		//Area calculation of a polygon
		public static float _CalculateArea(Vector3[] points) {
			Vector3 res = Vector3.zero;
			int length = points.Length;
			for(int v2 = length - 1, v1 = 0; v1 < length; v2 = v1++) {
				res += Vector3.Cross(points[v1], points[v2]);
			}
			res *= 0.5f;
			return res.magnitude;
		}

		//Area calculation of a polygon
		public static float _CalculateArea(List<Vector3> points) {
			return _CalculateArea(points.ToArray());
		}

		//Center of a polygon
		public static Vector3 _CalculateCenteroid(Vector3[] points) {
			float area = 0, x = 0, y = 0, z = 0;
			int length = points.Length;
			for(int v2 = length - 1, v1 = 0; v1 < length; v2 = v1++) {
				Vector3 factor = Vector3.Cross(points[v1], points[v2]);
				factor *= 0.5f;
				area += factor.magnitude;
				x += (points[v2].x + points[v1].x) * factor.magnitude;
				y += (points[v2].y + points[v1].y) * factor.magnitude;
				z += (points[v2].z + points[v1].z) * factor.magnitude;
			}
			area *= 2.0f;
			return new Vector3(x / area, y / area, z / area);
		}

		//Center of a polygon
		public static Vector3 _CalculateCenteroid(List<Vector3> points) {
			return _CalculateCenteroid(points.ToArray());
		}

		//Get an objects angular size (pixel size), best used on spherical objects
		public static float _GetAngularSize(Transform trs, Camera cam, float diameter) {
			float distance = _DistUtil._Get3D(trs.position, cam.transform.position);
			float size = (diameter / distance) * Mathf.Rad2Deg;
			return (size * Screen.height) / cam.fieldOfView;
		}

		//Get an objects angular size (pixel size), best used on spherical objects
		public static float _GetAngularSize(Transform trs, Camera cam) {
			float diameter = trs.GetComponent<Renderer>().bounds.extents.magnitude;
			return _GetAngularSize(trs, cam, diameter);
		}

		//Get an objects angular size (pixel size), best used on spherical objects
		public static float _GetRequiredFOVForAngularSize(Transform trs, Camera cam, float diameter) {
			float distance = _DistUtil._Get3D(trs.position, cam.transform.position);
			return Mathf.Atan2(diameter, distance) * Mathf.Rad2Deg;
		}

		//Angle on a circle given an height and radius
		public static float _CalculateAngleFromDistance(float radius, float distance) {
			//sin(x) = height / radius;
			//x = Mathf.Asin(height / radius) / Mathf.Rad2Deg;
			return Mathf.Acos(distance / radius) * Mathf.Rad2Deg;
		}

		//Angle on a circle given an height and radius
		public static float _CalculateAngleFromHeight(float radius, float height) {
			//sin(x) = height / radius;
			//x = Mathf.Asin(height / radius) / Mathf.Rad2Deg;
			return Mathf.Asin(height / radius) * Mathf.Rad2Deg;
		}

		//Height on a circle given an angle and radius
		public static float _CalculateHeightFromAngle(float radius, float angle) {
			return Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
		}

		//Distance on a circle given an angle and radius
		public static float _CalculateDistanceFromAngle(float radius, float angle) {
			return Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
		}

		//Arc Length on a circle
		public static float _CalculateArcLengthFromAngle(float radius, float angle) {
			return Mathf.PI * 2.0f * radius * (angle / 360f);
		}

		//Arc Length on a circle
		public static float _CalculateArcLengthFromRadian(float radius, float radian) {
			return radius * radian;
		}

		//Angle on a circle
		public static float _CalculateAngleFromArcLengthAndRadius(float radius, float length) {
			return length / (Mathf.PI * 2.0f * radius) * 360f;
		}

		//Angle on a circle
		public static float _CalculateAngleFromArcLengthAndCircumference(float circumference, float length) {
			return (length / circumference) * 360f;
		}

		//Calculate circle radius
		public static float _CalculateCircleRadiusFromDiameter(float diameter) {
			return diameter / 2.0f;
		}

		//Calculate circle radius
		public static float _CalculateCircleRadiusFromCircumference(float circumference) {
			return circumference / (2.0f * Mathf.PI);
		}

		//Calculate circle radius
		public static float _CalculateCircleRadiusFromArea(float area) {
			return Mathf.Sqrt(area / Mathf.PI);
		}

		//Calculate circle diameter
		public static float _CalculateCircleDiameter(float radius) {
			return radius * 2.0f;
		}

		//Calculate circle circumference
		public static float _CalculateCircleCircumference(float radius) {
			return Mathf.PI * 2.0f * radius;
		}

		//Calculate circle area
		public static float _CalculateCircleArea(float radius) {
			return Mathf.PI * Mathf.Pow(radius, 2);
		}

		//Calculate cone radius
		public static float _CalculateConeRadius(float height, float angle) {
			return height * Mathf.Tan(angle * Mathf.Deg2Rad);
		}

		//Given a transform of a plane, calculates clip plane in camera space.
		public static Vector4 _ClipPlane(Camera cam, Transform trs, float offset, float side) {
			return _ClipPlane(cam, trs.up, trs.position, offset, side);
		}

		//Given a plane, calculates clip plane in camera space.
		public static Vector4 _ClipPlane(Camera cam, Vector4 plane, float offset, float side) {
			Vector3 normal = new Vector3(plane.x, plane.y, plane.z);
			Vector3 position = normal * plane.w;
			return _ClipPlane(cam, normal, position, offset, side);
		}

		//Given position/normal of a plane, calculates clip plane in camera space.
		public static Vector4 _ClipPlane(Camera cam, Vector3 normal, Vector3 position, float offset, float side) {
			Matrix4x4 mtx = cam.worldToCameraMatrix;
			Vector3 dir = mtx.MultiplyVector(normal).normalized * side;
			Vector3 pos = mtx.MultiplyPoint(position + (normal * side * offset));
			return _GetPlaneEquation(dir, pos);
		}

		//Set the projection matrix to reflect
		public static Matrix4x4 _GetReflectionMatrix(Vector4 plane) {
			Matrix4x4 mtx = Matrix4x4.zero;

			mtx.m00 = (-2f * plane[0] * plane[0] + 1f);
			mtx.m01 = (-2f * plane[0] * plane[1]);
			mtx.m02 = (-2f * plane[0] * plane[2]);
			mtx.m03 = (-2f * plane[0] * plane[3]);
			mtx.m10 = (-2f * plane[1] * plane[0]);
			mtx.m11 = (-2f * plane[1] * plane[1] + 1f);
			mtx.m12 = (-2f * plane[1] * plane[2]);
			mtx.m13 = (-2f * plane[1] * plane[3]);
			mtx.m20 = (-2f * plane[2] * plane[0]);
			mtx.m21 = (-2f * plane[2] * plane[1]);
			mtx.m22 = (-2f * plane[2] * plane[2] + 1f);
			mtx.m23 = (-2f * plane[2] * plane[3]);
			mtx.m30 = 0f;
			mtx.m31 = 0f;
			mtx.m32 = 0f;
			mtx.m33 = 1f;

			return mtx;
		}

		//Set the projection matrix to oblique
		public static void _SetObliqueMatrix1(ref Matrix4x4 mtx, Vector4 plane) {
			Vector4 q;
			q.x = (_MathUtil._Sign(plane.x) + mtx[8]) / mtx[0];
			q.y = (_MathUtil._Sign(plane.y) + mtx[9]) / mtx[5];
			q.z = -1f;
			q.w = (1.0f - mtx[10]) / mtx[14];
			Vector4 c = plane * (2.0f / (Vector4.Dot(plane, q)));
			mtx[2] = c.x;
			mtx[6] = c.y;
			mtx[10] = c.z + 1.0f;
			mtx[14] = c.w;
		}

		//http://aras-p.info/texts/obliqueortho.html
		public static void _SetObliqueMatrix(ref Matrix4x4 mtx, Vector4 plane) {
			Vector4 q = mtx.inverse * new Vector4(_MathUtil._Sign(plane.x), _MathUtil._Sign(plane.y), 1.0f, 1.0f);
			Vector4 c = plane * (2.0f / (Vector4.Dot(plane, q)));
			// third row = clip plane - fourth row
			mtx[2] = c.x - mtx[3];
			mtx[6] = c.y - mtx[7];
			mtx[10] = c.z - mtx[11];
			mtx[14] = c.w - mtx[15];
		}

	}

}