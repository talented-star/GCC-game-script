using GrabCoin.Model;
using Mirror;
using System;
using UnityEngine;

namespace GrabCoin.GameWorld.Player
{
    public interface IInteractable
    {
        bool IsCanInteract { get; }
        string Name { get; }

        void Use(GameObject netIdentity, AuthInfo authInfo, Action<bool, IInteractable> answerStartUsing, Action<bool, IInteractable> answerFinishUsing);
        void Hightlight(bool isActive);
        string GetName();
        float GetWeight();
    }
}