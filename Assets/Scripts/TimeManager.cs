using UnityEngine;


// From https://github.com/Brackeys/Bullet-Time-Project/blob/master/Bullet%20Time/Assets/TimeManager.cs
public class TimeManager : MonoBehaviour
{

	public HourglassController uiHourglass;

	public float slowdownFactor = 0.05f;
	public float slowdownLength = 2f;


	private bool stopped = false;

	void Update()
	{
		if (!stopped) {
			Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
		}
	}

	public void DoSlowmotion()
	{
		Time.timeScale = slowdownFactor;
		Time.fixedDeltaTime = Time.timeScale * .02f;

		uiHourglass.NotifySlowdownStart(slowdownLength);
	}

	public void StopTime()
    {
		Time.timeScale = 0.001f;
		Time.fixedDeltaTime = Time.timeScale * .02f;
		stopped = true;
	}
	public void ContinueTime()
	{
		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.timeScale * .02f;
		stopped = false;
	}

}