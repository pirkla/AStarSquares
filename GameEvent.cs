using System.Collections.Generic;
using UnityEngine;


namespace AStarSquares
{
	[CreateAssetMenu]
	public class GameEvent : ScriptableObject
	{
		private List<GameEventListener> listeners = new List<GameEventListener>();

		public void Invoke(GameObject go)
		{
			for (int i = 0; i < listeners.Count; i++)
			{
				listeners[i].OnEventRaised(go);
			}
		}

		public void Register(GameEventListener listener)
		{ 
			listeners.Add(listener); 
		}

		public void Deregister(GameEventListener listener)
		{ 
			listeners.Remove(listener); 
		}
	}
}
