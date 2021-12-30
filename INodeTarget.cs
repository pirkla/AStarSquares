using UnityEngine.EventSystems;


namespace AStarSquares
{
    public interface INodeTarget : IEventSystemHandler
    {
        void NodeSelected(INavNode node);
    }
}
