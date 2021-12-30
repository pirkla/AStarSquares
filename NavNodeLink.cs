namespace AStarSquares
{
    public struct NavNodeLink {
        public NavNodeLink(INavNode linkedNavNode, float moveCost, float vertical) {
            LinkedNavNode = linkedNavNode;
            Distance = moveCost;
            Vertical = vertical;
        }
        public float Vertical;
        public INavNode LinkedNavNode;
        public float Distance;
    }    
}