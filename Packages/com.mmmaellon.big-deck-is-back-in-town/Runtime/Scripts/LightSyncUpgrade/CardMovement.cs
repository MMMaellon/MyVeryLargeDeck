
using MMMaellon.GroupTheory;
using MMMaellon.LightSync;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.BigDeckIsBackInTown
{
    [AddComponentMenu("")]
    public class CardMovement : LightSyncState
    {
        public bool disableCollisionsDuringYeet = true;
        public float yeetDuration = 0.5f;
        public Vector3 outgoingVel = new Vector3(0f, 3f, 0f);
        public Vector3 incomingVel = new Vector3(0f, -3f, 0f);
        [HideInInspector]
        public Card2 card;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public override void Reset()
        {
            base.Reset();
            if (!card)
            {
                card = GetComponent<Card2>();
                if (card)
                {
                    card.movement = this;
                }
            }
        }
#endif

        [System.NonSerialized]
        public Transform targetTransform;
        [System.NonSerialized]
        public IGroup targetGroup;
        [UdonSynced, FieldChangeCallback(nameof(target))]
        public int _target;
        public int target
        {
            get => _target;
            set
            {
                _target = value;

                if (value >= 0)
                {
                    targetGroup = card.singleton.GetGroupById(value);
                    if (targetGroup)
                    {
                        targetTransform = targetGroup.transform;
                    }
                    else
                    {
                        targetTransform = null;
                    }
                }
                else
                {
                    //negative value means we don't join group at end
                    targetGroup = null;
                    var tempGroup = card.singleton.GetGroupById(-value);
                    if (tempGroup)
                    {
                        targetTransform = tempGroup.transform;
                    }
                    else
                    {
                        targetTransform = null;
                    }
                }
            }
        }

        Vector3 startGlobalPos;
        Quaternion startGlobalRot;
        Vector3 targetGlobalPos;
        Quaternion targetGlobalRot;

        Vector3 startGlobalVel;
        Vector3 targetGlobalVel;

        public void YeetToGlobalTransform(Vector3 targetPos, Quaternion targetRot)
        {
            target = 0;
            sync.pos = targetPos;
            sync.rot = targetRot;
            sync.vel = transform.rotation * outgoingVel;
            EnterState();
        }

        public void YeetIntoGroup(IGroup group, Vector3 targetPosLocalToGroup, Quaternion targetRotLocalToGroup, bool joinGroupAtEnd)
        {
            if (!group)
            {
                return;
            }

            if (joinGroupAtEnd)
            {
                target = group.GetGroupId();
            }
            else
            {
                target = -group.GetGroupId();
            }

            sync.pos = targetPosLocalToGroup;
            sync.rot = targetRotLocalToGroup;
            sync.vel = transform.rotation * outgoingVel;
            EnterState();
        }

        public override void OnEnterState()
        {
            startGlobalPos = transform.position;
            startGlobalRot = transform.rotation;
            startGlobalVel = sync.vel;
            delay = 0;
            if (disableCollisionsDuringYeet)
            {
                sync.rigid.detectCollisions = false;
            }
        }

        public override void OnExitState()
        {
            //in case we get interrupted by something
            transform.SetParent(targetTransform);
            if (targetGroup)
            {
                card.AddToGroup(targetGroup);
            }
            if (disableCollisionsDuringYeet)
            {
                sync.rigid.detectCollisions = true;
            }
        }

        float delay;
        float interpolation;
        public override bool OnLerp(float elapsedTime, float autoSmoothedLerp)
        {
            if (yeetDuration <= 0)
            {
                interpolation = 1.0f;
            }
            else
            {
                interpolation = (elapsedTime - delay) / yeetDuration;
            }

            if (interpolation >= 1.0f)
            {
                transform.SetParent(targetTransform);
                transform.SetLocalPositionAndRotation(sync.pos, sync.rot);
                if (disableCollisionsDuringYeet)
                {
                    sync.rigid.detectCollisions = true;
                }
                if (sync.IsOwner() && targetGroup)
                {
                    card.AddToGroup(targetGroup);
                }
                return false;
            }
            else
            {
                if (targetTransform)
                {
                    targetGlobalPos = targetTransform.TransformPoint(sync.pos);
                    targetGlobalRot = targetTransform.rotation * sync.rot;
                    targetGlobalVel = targetGlobalRot * incomingVel;
                }
                else
                {
                    targetGlobalPos = sync.pos;
                    targetGlobalRot = sync.rot;
                    targetGlobalVel = targetGlobalRot * incomingVel;
                }
                transform.SetPositionAndRotation(HermiteInterpolatePosition(), Quaternion.Slerp(startGlobalRot, targetGlobalRot, interpolation));
            }
            return true;
        }

        Vector3 posControl1;
        Vector3 posControl2;
        public Vector3 HermiteInterpolatePosition()
        {//Shout out to Kit Kat for suggesting the improved hermite interpolation
            posControl1 = startGlobalPos + interpolation * yeetDuration * startGlobalVel / 3f;
            posControl2 = targetGlobalPos - (1.0f - interpolation) * yeetDuration * targetGlobalVel / 3f;
            return Vector3.Lerp(Vector3.Lerp(posControl1, targetGlobalPos, interpolation), Vector3.Lerp(startGlobalPos, posControl2, interpolation), interpolation);
        }
    }
}
