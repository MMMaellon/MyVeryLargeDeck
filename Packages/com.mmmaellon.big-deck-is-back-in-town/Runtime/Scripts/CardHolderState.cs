﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(Card))]
    public class CardHolderState : SmartObjectSyncState
    {
        public CardHolderManager manager;
        public Card card;
        [System.NonSerialized, UdonSynced, FieldChangeCallback(nameof(holder_id))]
        public short _holder_id = -1001;
        public short holder_id
        {
            get => _holder_id;
            set
            {
                _holder_id = value;
                if (value < 0 || value >= manager.holders.Length)
                {
                    holder = null;
                    Detach();
                }
                else
                {
                    holder = manager.holders[value].transform;
                    AttachIfSynced();
                }
                if (sync.IsOwnerLocal())
                {
                    RequestSerialization();
                }
            }
        }
        [System.NonSerialized]
        public Transform holder = null;
        public override void Interpolate(float interpolation)
        {
            if (holder != null)
            {
                transform.localPosition = sync.HermiteInterpolatePosition(start_pos, Vector3.zero, sync.pos, Vector3.zero, interpolation);
                transform.localRotation = sync.HermiteInterpolateRotation(start_rot, Vector3.zero, sync.rot, Vector3.zero, interpolation);
            }
        }

        Vector3 start_pos;
        Quaternion start_rot;
        public override void OnEnterState()
        {
            AttachIfSynced();
        }

        public override void OnExitState()
        {
            Detach();
        }

        public override bool OnInterpolationEnd()
        {
            transform.localPosition = sync.pos;
            transform.localRotation = sync.rot;
            return false;
        }

        public override void OnInterpolationStart()
        {
        }

        public override void OnSmartObjectSerialize()
        {
            sync.pos = transform.localPosition;
            sync.rot = transform.localRotation;
        }

        public void Attach(CardHolder holder)
        {
            if (!sync.IsLocalOwner())
            {
                sync.TakeOwnership(false);
            }
            holder_id = holder.id;
            EnterState();
        }

        public void AttachIfSynced()
        {
            if (holder != null && IsActiveState())
            {
                transform.SetParent(holder, true);
                sync.rigid.isKinematic = true;
                if (sync.interpolation >= 1)
                {
                    OnInterpolationEnd();
                }
                else
                {
                    start_pos = transform.localPosition;
                    start_rot = transform.localRotation;
                }
            }
        }
        public void Detach()
        {
            if (holder_id >= 0)
            {
                holder_id = -1001;
            }
            if (sync.state < SmartObjectSync.STATE_CUSTOM)
            {
                transform.SetParent(card.deck.cards_outside_deck_parent);
                sync.rigid.isKinematic = !card.card_physics;
            }
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void OnValidate()
        {
            manager = GameObject.FindObjectOfType<CardHolderManager>();
        }
        public override void Reset()
        {
            card = GetComponent<Card>();
            base.Reset();
        }
#endif
    }
}
