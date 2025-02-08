
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Data;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VRC.Udon.Serialization.OdinSerializer;



#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace MMMaellon.BigDeckIsBackInTown
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Deck : MMMaellon.LightSync.ObjectPool
    {
        public bool allowDrawingCards = true;
        [Header("Optional")]
        public GameObject deck_model;
        public GameObject empty_deck_model;
        public Material card_material;
        public Material hidden_card_material;
        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(next_card))]
        public int _next_card = -1001;
        public int next_card
        {
            get => _next_card;
            set
            {
                if (_next_card >= 0 && _next_card < objects.Length)
                {
                    objects[_next_card].SetVisibility();
                }
                _next_card = value;
                if (_next_card >= 0 && _next_card < objects.Length)
                {
                    objects[_next_card].SetVisibility();
                }
            }
        }

        [HideInInspector]
        public LightSync.LightSync deck_sync;
        public void OnEnable()
        {
            next_card = next_card;
        }

        public void PickNextCard()
        {

        }

    }
}


