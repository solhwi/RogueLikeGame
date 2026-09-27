using System;
using RogueLike.Core;

namespace RogueLike.Managers
{
    // Tracks how many enemies the player has killed this run. Enemies report
    // their own death here (EnemyChaseAI.HandleDied) rather than this class
    // polling for them.
    public class KillCounter : Singleton<KillCounter>
    {
        public event Action<int> OnKillCountChanged;

        public int Count { get; private set; }

        public void ResetRun()
        {
            Count = 0;
            OnKillCountChanged?.Invoke(Count);
        }

        public void RegisterKill()
        {
            Count++;
            OnKillCountChanged?.Invoke(Count);
        }
    }
}
