namespace AStarSquares
{
    public class NavNodeLink {
        public NavNodeLink(INavNode linkedNavNode, int moveCost, int vertical) {
            LinkedNavNode = linkedNavNode;
            Distance = moveCost;
            Vertical = vertical;
        }
        public int Vertical;
        public INavNode LinkedNavNode;
        public int Distance;

        public bool IsJump => Distance > 14;
    }    
}