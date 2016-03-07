using UnityEngine;
using System.Collections;

public class DataSmooth {
	public static float SmoothQuadratic (float val) {
		float ret = Mathf.Sign(val) * val * val;
		return ret;
	}

	public static float SmoothIdentical (float val) {
		return val;
	}
}
