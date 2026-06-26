using UnityEngine;

namespace LogicGatesGame.Scripts
{
    [CreateAssetMenu(menuName = "LogicGatesGame/Supabase Config", fileName = "SupabaseConfig")]
    public class SupabaseConfig : ScriptableObject
    {
        [SerializeField] private string projectUrl;
        [SerializeField] private string anonKey;

        public string ProjectUrl => projectUrl;
        public string AnonKey => anonKey;
    }
}
