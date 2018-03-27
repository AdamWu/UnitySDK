using UnityEngine;

public class MathUtils {

	public static float PointDistPlane(Vector3 planeN, Vector3 planeP, Vector3 P) {
		return Vector3.Dot (P - planeP, planeN);
	}

	public static Vector3 PointProjPlane(Vector3 planeN, Vector3 planeP, Vector3 P) {
		float d = Vector3.Dot (P - planeP, planeN);
		return P - d * planeN;
	}

	public static Vector3 LineIntersectPlane(Vector3 planeN, Vector3 planeP, Vector3 lineDir, Vector3 lineP) {
		float vt = Vector3.Dot (planeN, lineDir);
		if (vt == 0) {
			Debug.Log ("error");
		}
		float pn = Vector3.Dot (planeP - lineP, planeN);
		float t = pn / vt;
		return lineP + t * lineDir;
	}

	public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P) {
		Vector3 PA = A - P;
		Vector3 PB = B - P;
		Vector3 PC = C - P;
		Vector3 crossAB = Vector3.Cross (PA, PB);
		Vector3 crossBC = Vector3.Cross (PB, PC);
		Vector3 crossCA = Vector3.Cross (PC, PA);
		return Vector3.Dot(crossAB, crossBC) >= 0.0f && Vector3.Dot(crossBC, crossCA) >= 0.0f;
	}

	public static bool LineIntersect(Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
		Vector3 crossA = Vector3.Cross (d - c, a - c);
		Vector3 crossB = Vector3.Cross (d - c, b - c);

		if (Vector3.Dot(crossA, crossB) >= 0)
			return false;

		Vector3 crossC = Vector3.Cross (b - a, c - a);
		Vector3 crossD = Vector3.Cross (b - a, d - a);

		if (Vector3.Dot (crossC, crossD) >= 0)
			return false;

		return true;
	}

}