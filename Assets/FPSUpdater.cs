using UnityEngine;
using System.Collections;

public class FPSUpdater : MonoBehaviour 
{
	
	// Attach this to a GUIText to make a frames/second indicator.
	//
	// It calculates frames/second over each updateInterval,
	// so the display does not keep changing wildly.
	//
	// It is also fairly accurate at very low FPS counts (<10).
	// We do this not by simply counting frames per interval, but
	// by accumulating FPS for each frame. This way we end up with
	// correct overall FPS even if the interval renders something like
	// 5.5 frames.
	
	public  float updateInterval = 0.5F;
	
	private float accum   = 0; // FPS accumulated over the interval
	private int   frames  = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval

    public static string format = "";

	void Start()
	{

		if( !guiText )
		{
			Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
			enabled = false;
			return;
		}
		timeleft = 0;  
	}
	
	void Update()
	{
		
		
		
		// Interval ended - update GUI text and start new interval
		if( timeleft >= 1.0 )
		{
			// display two fractional digits (f2 format)
			float fps = frames;
			format = System.String.Format("{0:F2} FPS",fps);

			timeleft = 0;
			accum = 0.0F;
			frames = 0;
		}
        timeleft += Time.deltaTime;
        ++frames;
	}

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(format);
        GUILayout.EndHorizontal();
    }
}