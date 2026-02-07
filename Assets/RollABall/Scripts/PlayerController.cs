using UnityEngine;

// Include the namespace required to use Unity UI
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using RollABall.Licensing;

public class PlayerController : MonoBehaviour {

	private const string TimeLimitPrefKey = "TimeLimitSeconds";
	private const float EasyTimeLimitSeconds = 270f;   // 4:30
	private const float MediumTimeLimitSeconds = 180f; // 3:00
	private const float HardTimeLimitSeconds = 90f;    // 1:30

	// Create public variables for player speed, and for the Text UI game objects
	public float speed;
	public Text countText;
	public Text winText;
	public Text livesText;
	public GameTimer gameTimer; // Reference to the GameTimer script

	public Text PokemonsCount;

	public Text TimeRemaning;


	// Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
	private Rigidbody rb;
	private int count;
	private float timeoutDuration = 1f;
	private float originalSpeed;
	public float acceleration = 2f; // Rate of acceleration
	public float deceleration = 2f; // Speed at which the ball decelerates
    private Vector3 movementDirection = Vector3.zero; // To store the movement direction
    private Vector3 velocity = Vector3.zero; // To store the velocity for deceleration
	public int playerLives = 2; // Default lives
	// At the start of the game..

	void Start ()
	{
		if (!LicenseService.IsActivated(out _))
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(0);
			return;
		}

		rb = GetComponent<Rigidbody>(); // Assign the Rigidbody component to our private rb variable

		count = 0; // Set the count to zero
		winText.text = ""; // Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
		PokemonsCount.text = "";
		TimeRemaning.text = "";

		playerLives = PlayerPrefs.GetInt("PlayerLives");

		if (!PlayerPrefs.HasKey(TimeLimitPrefKey))
		{
			PlayerPrefs.SetFloat(TimeLimitPrefKey, EasyTimeLimitSeconds);
			PlayerPrefs.Save();
		}



		// Reset lives to 3 if it's 0 or less
        if (playerLives <= 0)
        {
            playerLives = 3;
            PlayerPrefs.SetInt("PlayerLives", playerLives);
        }

		livesText.text = "Lives: " + playerLives.ToString(); // Initialize lives display

		SetCountText (); // Run the SetCountText function to update the UI (see below)

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
            SceneManager.LoadScene(1);
        }

		if (Input.GetKeyDown(KeyCode.E))
		{
			ApplyDifficulty(EasyTimeLimitSeconds);
		}

		if (Input.GetKeyDown(KeyCode.M))
		{
			ApplyDifficulty(MediumTimeLimitSeconds);
		}

		if (Input.GetKeyDown(KeyCode.H))
		{
			ApplyDifficulty(HardTimeLimitSeconds);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			 SceneManager.LoadScene(0);
		}
    }

	private void ApplyDifficulty(float timeLimitSeconds)
	{
		PlayerPrefs.SetFloat(TimeLimitPrefKey, timeLimitSeconds);
		PlayerPrefs.Save();
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	// Each physics step..
	void FixedUpdate ()
	{
		// Get input values for movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Determine the movement direction based on input
        movementDirection = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        // Apply horizontal force for movement
        if (movementDirection != Vector3.zero)
        {
            // Calculate the desired horizontal velocity
            Vector3 targetVelocity = movementDirection * speed;
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 force = targetVelocity - horizontalVelocity;
            rb.AddForce(force * acceleration * Time.deltaTime, ForceMode.VelocityChange);
        }
        else
        {
            // Apply horizontal deceleration if no input is provided
            Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 decelerationForce = -horizontalVelocity * deceleration * Time.deltaTime;
            rb.AddForce(decelerationForce, ForceMode.VelocityChange);
        }

        // Limit the maximum horizontal speed
        Vector3 velocity = rb.velocity;
        if (new Vector3(velocity.x, 0, velocity.z).magnitude > speed)
        {
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z).normalized * speed;
            rb.velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
	}

	// When this game object intersects a collider with 'is trigger' checked,
	// store a reference to that collider in a variable named 'other'..
	void OnTriggerEnter(Collider other)
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{
			// Make the other game object (the pick up) inactive, to make it disappear
			other.gameObject.SetActive (false);

			// Add one to the score variable 'count'
			count = count + 1;

			// Run the 'SetCountText()' function (see below)
			SetCountText ();
		}

		if (other.gameObject.CompareTag ("WinGame"))
		{
			winText.text = "You Win!";
			PokemonsCount.text = "Coins: <color=white>" + count.ToString() + "</color>";


			int minutes = Mathf.FloorToInt(gameTimer.timeRemaining / 60f);
    		int seconds = Mathf.FloorToInt(gameTimer.timeRemaining % 60f);

			string timeFormatted = string.Format("{0:00}:{1:00}", minutes, seconds);
			TimeRemaning.text = "Time Remaining: <color=white>" + timeFormatted + "</color>";

			Time.timeScale = 0f;
			StartCoroutine(LoadSceneAfterDelay(15f, 0));
		}

		if (other.gameObject.CompareTag ("PowerUp"))
		{
			speed = 6;
			Invoke("RevertSpeed", timeoutDuration);
		}

		if (other.gameObject.CompareTag("EndGame")) {
			playerLives--;
			PlayerPrefs.SetInt("PlayerLives", playerLives); // Save the updated lives count
			livesText.text = "Lives: " + playerLives.ToString(); // Update lives display

			if(playerLives > 0) {

				// Reload the current scene to reset everything
            	SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			} else {

			SceneManager.LoadScene(0);
			}
		}
	}

	void RevertSpeed()
    {
        // Revert the speed back to its original value
        speed = originalSpeed;
    }

	// Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
	void SetCountText()
	{
		// Update the text field of our 'countText' variable
		if(countText != null) {

		countText.text = "Coins: " + count.ToString();
		}

		// Check if our 'count' is equal to or exceeded 12
		// if (count >= 18)
		// {
		// 	// Set the text value of our 'winText'
		// 	winText.text = "You Win!";
		// }
	}

	public IEnumerator LoadSceneAfterDelay(float delay, int sceneIndex)
    {
        yield return new WaitForSecondsRealtime(delay); // Wait for real time instead of game time
        Time.timeScale = 1f; // Resume the game just before loading the scene
        SceneManager.LoadScene(sceneIndex);
    }
}