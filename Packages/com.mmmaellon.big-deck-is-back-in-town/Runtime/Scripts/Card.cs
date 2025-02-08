
using MMMaellon.LightSync;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace MMMaellon.BigDeckIsBackInTown
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Card : ObjectPoolObject
    {
        [UdonSynced, FieldChangeCallback(nameof(visible_to_owner))]
        public bool _visible_to_owner = false;
        public bool visible_to_owner
        {
            get => _visible_to_owner;
            set
            {
                _visible_to_owner = value;
                // SetVisibility();
                if (sync.IsOwner())
                {
                    RequestSerialization();
                }
            }
        }
        [UdonSynced, FieldChangeCallback(nameof(visible_to_others))]
        public bool _visible_to_others = false;
        public bool visible_to_others
        {
            get => _visible_to_others;
            set
            {
                _visible_to_others = value;
                // SetVisibility();
                if (sync.IsOwner())
                {
                    RequestSerialization();
                }
            }
        }
        [UdonSynced, FieldChangeCallback(nameof(pickupable_by_owner))]
        public bool _pickupable_by_owner = false;
        public bool pickupable_by_owner
        {
            get => _pickupable_by_owner;
            set
            {
                _pickupable_by_owner = value;
                // SetPickupable();
                if (sync.IsOwner())
                {
                    RequestSerialization();
                }
            }
        }
        [UdonSynced, FieldChangeCallback(nameof(pickupable_by_others))]
        public bool _pickupable_by_others = false;
        public bool pickupable_by_others
        {
            get => _pickupable_by_others;
            set
            {
                _pickupable_by_others = value;
                // SetPickupable();
                if (sync.IsOwner())
                {
                    RequestSerialization();
                }
            }
        }
        public bool card_physics = true;
        public Renderer render_component;
        public GameObject child;
        public CardThrowing throwing;
        public Collider collider_component;

        [HideInInspector]
        public Deck deck;

        public override void OnSpawnPoolObject()
        {
            base.OnSpawnPoolObject();
            if (deck.next_card == id)
            {
                render_component.enabled = true;
                SetPickupability(sync.IsOwner());
            }
        }

        public override void OnDespawnPoolObject()
        {
            base.OnDespawnPoolObject();
            if (deck.next_card == id)
            {
                render_component.enabled = false;
                gameObject.SetActive(true);
                SetPickupability(Networking.IsOwner(pool.gameObject));
            }
        }

        [SerializeField]
        Material startingMaterial;

        public void SetMaterial(bool owner)
        {
            if (owner)
            {
                child.SetActive(visible_to_owner);
                if (visible_to_owner)
                {
                    if (deck.card_material)
                    {
                        render_component.material = deck.card_material;
                    }
                    else
                    {
                        render_component.material = startingMaterial;
                    }
                }
                else if (deck.hidden_card_material)
                {
                    render_component.material = deck.hidden_card_material;
                }
                else
                {
                    render_component.material = startingMaterial;
                }
            }
            else if (visible_to_others)
            {
                if (deck.card_material)
                {
                    render_component.material = deck.card_material;
                }
                else
                {
                    render_component.material = startingMaterial;
                }
            }
            else if (deck.hidden_card_material)
            {
                render_component.material = deck.hidden_card_material;
            }
            else
            {
                render_component.material = startingMaterial;
            }
        }

        public void SetPickupability(bool owner)
        {
            if (owner)
            {
                if (pickupable_by_owner)
                {
                    collider_component.enabled = true;
                    sync.pickup.pickupable = true;
                }
                else
                {
                    collider_component.enabled = false;
                    sync.pickup.pickupable = false;
                }
            }
            else if (pickupable_by_others)
            {
                collider_component.enabled = true;
                sync.pickup.pickupable = true;
            }
            else
            {
                collider_component.enabled = false;
                sync.pickup.pickupable = false;
            }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {

        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public virtual void Setup()
        {
            if (!render_component)
            {
                render_component = GetComponent<Renderer>();
            }
            if (!collider_component)
            {
                collider_component = GetComponent<Collider>();
            }
            if (!throwing)
            {
                throwing = GetComponent<CardThrowing>();
            }
            if (render_component)
            {
                startingMaterial = render_component.material;
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
#endif
    }
}
