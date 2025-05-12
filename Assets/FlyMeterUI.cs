using UnityEngine;
using UnityEngine.UI;

public class FlyMeterUI : MonoBehaviour
{
    public PlayerController player;   // Reference to the player
    public Slider flySlider;          // Reference to the UI slider

    void Update()
    {
        if (player != null && flySlider != null)
        {
            flySlider.value = player.GetFlyMeterPercent();
        }
    }
}
