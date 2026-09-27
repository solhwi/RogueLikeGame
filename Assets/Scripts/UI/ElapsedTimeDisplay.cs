using UnityEngine;
using UnityEngine.UI;

namespace RogueLike.UI
{
    // Counts up and displays mm:ss for as long as the chapter runs. Purely a
    // display — nothing reads this value, it does not gate any win/lose
    // condition. Naturally pauses with the rest of gameplay since it uses
    // scaled deltaTime, which GameManager zeroes out (Time.timeScale = 0)
    // while paused/leveling up.
    public class ElapsedTimeDisplay : MonoBehaviour
    {
        [SerializeField] private Text timeText;

        private float elapsed;

        private void Update()
        {
            elapsed += Time.deltaTime;

            if (timeText == null)
            {
                return;
            }

            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
