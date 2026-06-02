// Pure in-memory session state. Resets automatically on every game restart.
// No MonoBehaviour, no DontDestroyOnLoad — just a static container.
public static class SessionState
{
    // Set to true after the player completes (or skips) the prologue for the first time this session.
    // MainMenuController uses this to route Start → SampleScene directly on return visits.
    public static bool ProloguePlayed = false;

    // Set to true the moment FirstRunSetup is shown this session.
    // Prevents the panel from appearing twice if the player somehow runs Prologue twice in one session.
    public static bool ApiSetupShown = false;
}
