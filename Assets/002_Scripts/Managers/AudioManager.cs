using _002_Scripts.Interface;
using UnityEngine;

namespace _002_Scripts.Managers
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public AudioManager Instance;
        [SerializeField] private AudioListener Listener;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        #region Audio Service Layer

        public void Play(int loop = 1)
        {
            
        }

        #endregion
    }
}