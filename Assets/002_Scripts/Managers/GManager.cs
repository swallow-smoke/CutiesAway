using _002_Scripts.Interface;
using UnityEngine;

namespace _002_Scripts.Managers
{
    public class GManager : MonoBehaviour, IGameService
    {
        public GManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
    }
}