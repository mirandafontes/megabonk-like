using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenericPool
{
    public class MonoBehaviourPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly Func<T> factory;
        private readonly Queue<T> availableObjects = new Queue<T>();
        private readonly Transform parent;

        public int InitialSize { get; private set; }
        public bool CanExpand { get; private set; }

        public MonoBehaviourPool(Func<T> factory, int initialSize, bool canExpand, Transform parent)
        {
            this.factory = factory;
            InitialSize = initialSize;
            CanExpand = canExpand;
            this.parent = parent;

            for (int i = 0; i < InitialSize; i++)
            {
                var obj = this.factory.Invoke();
                obj.transform.SetParent(this.parent);
                ReturnToPool(obj);
            }
        }

        public T Get()
        {
            T obj = null;

            //Short-cut
            if (availableObjects.Count <= 0 && !CanExpand)
            {
                Debug.LogError($"[MonobehaviourPool] The pool for {typeof(T).Name} is empty and cannot be expanded.");
                return null;
            }

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else if (CanExpand)
            {
                obj = factory.Invoke();
                obj.transform.SetParent(parent);
                InitialSize++;
            }
            //Else para evitar warning do compilador
            else
            {
                Debug.LogError($"[MonobehaviourPool] The pool for {typeof(T).Name} is empty and cannot be expanded.");
                return null;
            }

            obj.gameObject.SetActive(true);
            obj.OnGetFromPool();
            return obj;
        }

        public void ReturnToPool(T obj)
        {
            if (availableObjects.Contains(obj))
            {
                Debug.LogError($"[MonobehaviourPool] {obj.name} was already in the pool.");
                return;
            }

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parent);
            obj.OnReturnToPool();
            availableObjects.Enqueue(obj);
        }
    }
}