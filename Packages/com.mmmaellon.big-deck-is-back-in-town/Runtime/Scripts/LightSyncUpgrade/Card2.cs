
using MMMaellon.GroupTheory;
using MMMaellon.LightSync;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.BigDeckIsBackInTown
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CardMovement))]
    public class Card2 : Item
    {
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public CardMovement movement;

        public void Reset()
        {
            animator = GetComponent<Animator>();
            movement = GetComponent<CardMovement>();
            movement.card = this;
        }

        public void DisableAnimator()
        {
            animator.enabled = false;
        }

        public void ShowMesh(bool visible)
        {
            animator.enabled = true;
            animator.SetBool("meshVisible", visible);
        }

        public void BlankCard(bool blank)
        {
            animator.enabled = true;
            animator.SetBool("blank", blank);
        }

    }
}
