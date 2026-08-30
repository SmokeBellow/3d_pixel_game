// PROTOTYPE - NOT FOR PRODUCTION
// Question: Do cross-player elemental synergies discovered in real-time combat feel spontaneous and fun?
// Date: 2026-08-26

using UnityEngine;

/// <summary>
/// Prototype-only debug HUD: shows controls and live cooldowns for both players via
/// OnGUI, plus a reminder to watch the Console for [SYNERGY] messages when Chain Shock
/// triggers. No real UI system needed for this throwaway build.
/// </summary>
public class HudOverlay : MonoBehaviour
{
    public PlayerSpellController player1;
    public PlayerSpellController player2;

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Box(new Rect(10, 10, 320, 130), "");
        GUI.Label(new Rect(20, 15, 300, 20), "PLAYER 1 — WASD move | 1=Fire 2=Water 3=Lightning");
        if (player1 != null)
        {
            GUI.Label(new Rect(20, 35, 300, 20), $"Fire: {player1.FireCooldownRemaining:0.0}s  Water: {player1.WaterCooldownRemaining:0.0}s  Lightning: {player1.LightningCooldownRemaining:0.0}s");
        }

        GUI.Box(new Rect(Screen.width - 330, 10, 320, 130), "");
        GUI.Label(new Rect(Screen.width - 320, 15, 300, 20), "PLAYER 2 — Arrows move | Numpad1=Fire Numpad2=Water Numpad3=Lightning");
        if (player2 != null)
        {
            GUI.Label(new Rect(Screen.width - 320, 35, 300, 20), $"Fire: {player2.FireCooldownRemaining:0.0}s  Water: {player2.WaterCooldownRemaining:0.0}s  Lightning: {player2.LightningCooldownRemaining:0.0}s");
        }

        GUI.Label(new Rect(10, 150, 500, 20), "Watch the Console for [SYNERGY] messages — Lightning on a Wet target triggers Chain Shock!");
    }
}
