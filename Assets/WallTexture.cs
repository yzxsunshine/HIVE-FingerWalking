using UnityEngine;
using System.Collections;

public class WallTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
		//this.GetComponent<RenderTexture> ().wrapMode = TextureWrapMode.Repeat;
	}
	
	// Update is called once per frame
	void Update () {
		this.GetComponent<Renderer> ().material.mainTextureScale = new Vector2 (transform.localScale.x, transform.localScale.y);
	}
}
