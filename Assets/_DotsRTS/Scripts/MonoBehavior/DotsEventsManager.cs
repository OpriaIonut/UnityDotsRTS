using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

namespace DotsRTS
{
    public class DotsEventsManager : MonoBehaviour
    {
        public event EventHandler OnBarracksQueueChanged;
        public UnityAction OnHQDead;

        #region Singleton
        public static DotsEventsManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("More than one singleton of type DotsEventsManager in the scene; deleting from: " + gameObject.name);
                Destroy(this);
            }
            else
                Instance = this;
        }
        #endregion

        public void TriggerOnBarracksUnitQueueChanged(NativeList<Entity> entities)
        {
            foreach(var entity in entities)
            {
                OnBarracksQueueChanged?.Invoke(entity, EventArgs.Empty);
            }
        }

        public void TriggerOnHQDead()
        {
            OnHQDead?.Invoke();
        }
    }
}