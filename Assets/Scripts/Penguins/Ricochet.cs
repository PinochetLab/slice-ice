using UnityEngine;

namespace Penguins
{
    public class Ricochet
    {
        public float Distance { get; private set; }
        
        public Vector2 NewPosition { get; private set; }
        public Vector2 NewDirection { get; private set; }
        
        public Ricochet(float distance, Vector2 newPosition, Vector2 newDirection)
        {
            Distance = distance;
            NewPosition = newPosition;
            NewDirection = newDirection;
        }
    }
}