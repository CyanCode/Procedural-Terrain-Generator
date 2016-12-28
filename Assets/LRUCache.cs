//The MIT License (MIT)
//
//Copyright (c) 2015 Justin Nelson
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Specialized;

public class LRUCache<TKey, TValue> {
	private readonly int _maxObjects;
	private OrderedDictionary _data;

	public LRUCache(int maxObjects = 1000) {
		_maxObjects = maxObjects;
		_data = new OrderedDictionary();
	}

	public void Add(TKey key, TValue value) {
		if (_data.Count >= _maxObjects) {
			_data.RemoveAt(0);
		}
		_data.Add(key, value);
	}

	public TValue Get(TKey key) {
		if (!_data.Contains(key)) {
			throw new Exception("Could not find item with key " + key);
		}
		var result = _data[key];

		_data.Remove(key);
		_data.Add(key, result);

		return (TValue)result;
	}

	public bool Contains(TKey key) {
		return _data.Contains(key);
	}

	public TValue this[TKey key] {
		get { return Get(key); }
		set { Add(key, value); }
	}
}