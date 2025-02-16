
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.BigDeckIsBackInTown
{
    public class CardGroup : GroupTheory.IGroup
    {
        public CardGroup defaultSourceDeck;

        public int handSize = 5;
        public float dealAnimationDuration = 1.0f;
        public Vector3 cardDealSpacing = new Vector3(0.2f, 0, 0);
        public Vector3 cardDealFanRotation = new Vector3(0, 0, -10f);

        public bool HiddenToGroupOwner = false;
        public bool HiddenToNonOwners = false;

        public bool BlankToGroupOwner = false;
        public bool BlankToNonOwner = false;

        public void Start()
        {
            Debug.Log("STARTING ITEMS: " + itemIds.Count);
        }

        public override void OnAddItem(GroupTheory.Item item)
        {
            Card2 card = (Card2)item;
            if (!card)
            {
                return;
            }
            Debug.Log("OnAddItem " + name);
            //Remove from all other card groups
            var existingGroups = item.GetGroups();
            for (int i = 0; i < existingGroups.Count; i++)
            {
                var group = (CardGroup)existingGroups[i].Reference;
                if (group != this)
                {
                    Debug.Log("removing " + group.name);
                    item.RemoveFromGroup(group);
                }
                //will try to remove from null if group is not a card group
            }

#if !UNITY_EDITOR || COMPILER_UDONSHARP
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                card.ShowMesh(HiddenToGroupOwner);
                card.BlankCard(BlankToGroupOwner);
            }
            else
            {
                card.ShowMesh(HiddenToNonOwners);
                card.BlankCard(BlankToNonOwner);
            }
#endif
        }

        public override void OnRemoveItem(GroupTheory.Item item)
        {
            Card2 card = (Card2)item;
            if (!card)
            {
                return;
            }

#if !UNITY_EDITOR || COMPILER_UDONSHARP
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                if (HiddenToGroupOwner)
                {
                    card.ShowMesh(true);
                }
                if (BlankToGroupOwner)
                {
                    card.BlankCard(true);
                }
            }
            else
            {
                if (HiddenToNonOwners)
                {
                    card.ShowMesh(true);
                }
                if (BlankToNonOwner)
                {
                    card.BlankCard(true);
                }
            }
#endif
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            var cards = GetItems().ToArray();
            foreach (var token in cards)
            {
                var card = (Card2)token.Reference;
                if (!card)
                {
                    continue;
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    card.ShowMesh(HiddenToGroupOwner);
                    card.BlankCard(BlankToGroupOwner);
                }
                else
                {
                    card.ShowMesh(HiddenToNonOwners);
                    card.BlankCard(BlankToNonOwner);
                }
            }
        }

        public Card2 GetRandomCard()
        {
            if (itemList.Count == 0)
            {
                return null;
            }
            return (Card2)itemList[Random.Range(0, itemList.Count)].Reference;
        }

        public DataList GetRandomCards(int count)
        {
            if (itemList.Count == 0)
            {
                return itemList.ShallowClone();
            }
            var outputList = new DataList();
            var clone = itemList.ShallowClone();
            int randomIndex;
            for (int i = 0; i < count; i++)
            {
                if (clone.Count == 0)
                {
                    break;
                }
                randomIndex = Random.Range(0, clone.Count);
                outputList.Add(clone[randomIndex]);
                clone.RemoveAt(randomIndex);
            }
            return outputList;
        }

        public Card2 DealRandomCard(CardGroup source)
        {
            if (!source)
            {
                return null;
            }
            var card = source.GetRandomCard();
            if (!card)
            {
                return null;
            }
            card.AddToGroup(this);
            card.movement.YeetIntoGroup(this, Vector3.zero, Quaternion.identity, false);
            return card;
        }

        CardGroup dealSource;
        int lastDealtIndex = -1;
        int totalCardsToDeal;
        float dealStartTime;

        public void DealRandomCards(CardGroup source, int count)
        {
            if (!source || count <= 0)
            {
                return;
            }
            lastDealCardTime = Time.timeSinceLevelLoad;
            cardsDealt = 0;
            dealSource = source;
            startDealIndex = itemIds.Count;
            if (endDealTime > Time.timeSinceLevelLoad)
            {
                totalCardsToDeal = count + totalCardsToDeal;
            }
            else
            {
                totalCardsToDeal = count + itemIds.Count;
                SendCustomEventDelayedFrames(nameof(DealHandLoop), 1);
            }
            endDealTime = Time.timeSinceLevelLoad + dealAnimationDuration;
        }

        public void DealOne()
        {
            DealRandomCard(defaultSourceDeck);
        }

        public void Deal()
        {
            if (handSize <= 1)
            {
                DealRandomCard(defaultSourceDeck);
            }
            else
            {
                DealRandomCards(defaultSourceDeck, handSize);
            }
        }

        public void DealUntilHandFull()
        {
            DealRandomCards(defaultSourceDeck, handSize - itemIds.Count);
        }

        public void DealAnotherHand()
        {
            DealRandomCards(defaultSourceDeck, handSize);
        }

        float lastDealCardTime;
        float endDealTime;
        int cardsDealt;
        int startDealIndex;
        public void DealHandLoop()
        {
            if (!dealSource || itemIds.Count >= totalCardsToDeal)
            {
                return;
            }

            var nextIndex = Mathf.Min(totalCardsToDeal, Mathf.FloorToInt((totalCardsToDeal - itemIds.Count) * (Time.timeSinceLevelLoad - lastDealCardTime) / (endDealTime - lastDealCardTime)));
            if (nextIndex < 0)
            {
                return;
            }
            if (nextIndex < 1 && cardsDealt == 0)
            {
                nextIndex = 1;
            }
            for (int i = 0; i < nextIndex; i++)
            {
                var card = dealSource.GetRandomCard();
                if (!card)
                {
                    //stop the loop
                    return;
                }
                card.AddToGroup(this);
                var offset = cardsDealt - ((totalCardsToDeal - 1 - startDealIndex) / 2f);
                card.movement.YeetIntoGroup(this, cardDealSpacing * offset, Quaternion.Euler(cardDealFanRotation * offset), false);
                lastDealCardTime = Time.timeSinceLevelLoad;
                cardsDealt++;
            }
            SendCustomEventDelayedFrames(nameof(DealHandLoop), 0);
        }
    }
}
