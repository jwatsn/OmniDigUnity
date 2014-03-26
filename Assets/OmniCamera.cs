using UnityEngine;
using System.Collections;

public class OmniCamera : MonoBehaviour {
	
	public OmniObject owner;
    public static Camera cam;
    public static Camera mirrorCamX;
    public static Camera mirrorCamCorner;
    public static Camera mirrorCamY;
    public static int mirrorX = 0;
    public static int mirrorY = 0;
    public static int mirrorCorner = 0;

	void Start () {
        cam = camera;
        mirrorCamX = GameObject.Find("MirrorCamX").GetComponent<Camera>();
        mirrorCamCorner = GameObject.Find("MirrorCamCorner").GetComponent<Camera>();
        mirrorCamY = GameObject.Find("MirrorCamY").GetComponent<Camera>();


        //mirrorCam.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        
	}
}
