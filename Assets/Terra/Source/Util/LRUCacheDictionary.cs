using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class LRUCacheDictionary<K, V> {
	private int capacity;
	private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
	private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

	public LRUCacheDictionary(int capacity) {
		this.capacity = capacity;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public V Get(K key) {
		LinkedListNode<LRUCacheItem<K, V>> node;
		if (cacheMap.TryGetValue(key, out node)) {
			V value = node.Value.value;
			lruList.Remove(node);
			lruList.AddLast(node);
			return value;
		}
		return default(V);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	public void Add(K key, V val) {
		if (cacheMap.Count >= capacity) {
			RemoveFirst();
		}

		LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
		LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
		lruList.AddLast(node);
		cacheMap.Add(key, node);
	}

	private void RemoveFirst() {
		// Remove from LRUPriority
		LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
		lruList.RemoveFirst();

		// Remove from cache
		cacheMap.Remove(node.Value.key);
	}
}

class LRUCacheItem<K, V> {
	public LRUCacheItem(K k, V v) {
		key = k;
		value = v;
	}
	public K key;
	public V value;
}