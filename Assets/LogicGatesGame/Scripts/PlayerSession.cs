namespace LogicGatesGame.Scripts
{
    /// <summary>
    /// Holds the player name selected on the main menu so it survives the scene
    /// load into the game scene, where the telemetry session is built.
    /// Mirrors the <see cref="DifficultyManager"/> static-holder pattern.
    /// </summary>
    public static class PlayerSession
    {
        public static string SelectedPlayerName { get; set; } = string.Empty;
    }
}
