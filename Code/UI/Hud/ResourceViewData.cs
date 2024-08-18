using System;
using UnityEngine;

namespace UI.Data
{
    public struct ResourceViewData : ISendData
    {
        public readonly Vector3 Position;
        public readonly RectTransform EndPosition;
        public readonly Sprite Sprite;
        public readonly int Count;

        public ResourceViewData(Vector3 position, RectTransform endPosition, Sprite sprite, int count)
        {
            Position = position;
            EndPosition = endPosition;
            Sprite = sprite;
            Count = count;
        }
    }
}