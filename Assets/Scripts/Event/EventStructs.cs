namespace Event
{
    /// <summary>
    /// Chamado no EnemyManager.
    /// Consumido no PlayerController.
    /// </summary>
    public struct OnEnemyDeathEvent
    {
        public int TotalExperience;
        public int TotalEnemiesKilled;
    }

    public struct OnGameStart
    {

    }

    public struct OnGameEnd
    {

    }

    public struct OnPlayerHit
    {

    }

    public struct OnPlayerDeath
    {

    }

    public struct OnPlayerLevelUp
    {

    }

    public struct OnPlayerAcquireExp
    {

    }


}