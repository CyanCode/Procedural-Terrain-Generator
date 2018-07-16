using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
	public GameObject Parent;

	/// <summary>
	/// Wrapper class for handling the passing of 
	/// gameobjects and their associated placement type
	/// </summary>
	public class ObjectContainer {
		ObjectPlacementType Type;
		Transform Parent;
		LinkedList<GameObject> Active = new LinkedList<GameObject>();
		LinkedList<GameObject> Inactive = new LinkedList<GameObject>();

		public ObjectContainer(ObjectPlacementType type, Transform parent) {
			Type = type;
			Parent = parent;
		}

		/// <summary>
		/// "Warms up" this object container by instantiating a 
		/// number of Gameobjects.
		/// </summary>
		/// <param name="count">Amount of gameobjects to create</param>
		/// <param name="type">Object placement type to create</param>
		/// <param name="active">Optionally set gameobjects to active at start</param>
		public void Warmup(int count, bool active = true) {
			for (int i = 0; i < count; i++) {
				GameObject go = Object.Instantiate(Type.Prefab, Parent.transform);
				go.transform.parent = Parent;
				go.SetActive(active);

				if (active)
					Active.AddLast(go);
				else
					Inactive.AddLast(go);
			}
		}
	}


	public ObjectPool(GameObject parent) {
		Parent = parent;
	}

}