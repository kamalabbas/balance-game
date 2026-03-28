using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using RollABall.Licensing;

public class PlayerController : MonoBehaviour
{
	private const string TimeLimitPrefKey = "TimeLimitSeconds";
	private const float EasyTimeLimitSeconds = 270f;   // 4:30

	public float speed;
	public Text countText;
	public Text winText;
	public Text livesText; // Optional legacy UI; lives are not used.
	public GameTimer gameTimer; // Reference to the GameTimer script
	public Text PokemonsCount;
	public Text TimeRemaning;

	public float acceleration = 2f; // Rate of acceleration
	public float deceleration = 2f; // Speed at which the ball decelerates

	private Rigidbody rb;
	private int count;
	private float timeoutDuration = 1f;
	private float originalSpeed;
	private Vector3 movementDirection = Vector3.zero;
	private Vector3 spawnPosition;
	private Quaternion spawnRotation;

	void Start()
	{
		if (!LicenseService.IsActivated(out _))
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(0);
			return;
		}

		rb = GetComponent<Rigidbody>();
		spawnPosition = transform.position;
		spawnRotation = transform.rotation;

		count = 0;
		if (winText != null) winText.text = "";
		if (PokemonsCount != null) PokemonsCount.text = "";
		if (TimeRemaning != null) TimeRemaning.text = "";
		if (livesText != null) livesText.text = "";

		if (!PlayerPrefs.HasKey(TimeLimitPrefKey))
		{
			PlayerPrefs.SetFloat(TimeLimitPrefKey, EasyTimeLimitSeconds);
			PlayerPrefs.Save();
		}

		SetCountText();

		if (gameTimer != null)
		{
			gameTimer.StartTimer();
		}

		originalSpeed = speed;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			return;
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene(0);
		}
	}

	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

		movementDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

		if (movementDirection != Vector3.zero)
		{
			Vector3 targetVelocity = movementDirection * speed;
			Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			Vector3 force = targetVelocity - horizontalVelocity;
			rb.AddForce(force * acceleration * Time.deltaTime, ForceMode.VelocityChange);
		}
		else
		{
			Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			Vector3 decelerationForce = -horizontalVelocity * deceleration * Time.deltaTime;
			rb.AddForce(decelerationForce, ForceMode.VelocityChange);
		}

		Vector3 currentVelocity = rb.velocity;
		if (new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude > speed)
		{
			Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized * speed;
			rb.velocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Pick Up"))
		{
			other.gameObject.SetActive(false);
			count = count + 1;
			SetCountText();
			return;
		}

		if (other.gameObject.CompareTag("WinGame"))
		{
			if (winText != null) winText.text = "You Win!";
			if (PokemonsCount != null) PokemonsCount.text = "Coins: <color=white>" + count.ToString() + "</color>";

			if (gameTimer != null && TimeRemaning != null)
			{
				int minutes = Mathf.FloorToInt(gameTimer.timeRemaining / 60f);
				int seconds = Mathf.FloorToInt(gameTimer.timeRemaining % 60f);
				string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
				TimeRemaning.text = "Time Remaining: <color=white>" + timeFormatted + "</color>";
			}

			Time.timeScale = 0f;
			StartCoroutine(LoadSceneAfterDelay(8f, 0));
			return;
		}

		if (other.gameObject.CompareTag("PowerUp"))
		{
			speed = 6;
			Invoke(nameof(RevertSpeed), timeoutDuration);
			return;
		}

		if (other.gameObject.CompareTag("EndGame"))
		{
			RespawnToStart();
		}
	}

	void RevertSpeed()
	{
		speed = originalSpeed;
	}

	void SetCountText()
	{
		if (countText != null)
		{
			countText.text = "Coins: " + count.ToString();
		}
	}

	private void RespawnToStart()
	{
		if (rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}

		transform.SetPositionAndRotation(spawnPosition, spawnRotation);
	}

	public IEnumerator LoadSceneAfterDelay(float delay, int sceneIndex)
	{
		yield return new WaitForSecondsRealtime(delay);
		Time.timeScale = 1f;
		SceneManager.LoadScene(sceneIndex);
	}
}