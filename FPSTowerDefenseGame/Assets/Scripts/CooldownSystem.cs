using System;
using System.Collections.Generic;
using UnityEngine;

    public class CooldownSystem : MonoBehaviour
    {
        private readonly List<CooldownData> cooldowns = new List<CooldownData>();

        private void Update()
        {
            ProcessCooldowns();

        }

        public void PutOnCooldown(activeCooldown cooldown)
        {
            cooldowns.Add(new CooldownData(cooldown)); //Adds cooldown to list
        }

        /// <summary>
        /// Checks if there is a cooldown in the list
        /// </summary>
        public bool IsOnCooldown(int id)
        {
            foreach(CooldownData cooldown in cooldowns)
            {
                if (cooldown.Id == id) return true;
            }

            return false;
        }

        /// <summary>
        /// Keeps tracks of how much time is left
        /// </summary>
        public float GetRemainingTime(int id)
        {
            foreach (CooldownData cooldown in cooldowns)
            {
                if (cooldown.Id != id) continue;

                return cooldown.timeRemaining;
            }

            return 0f;
        }


        //Function that removes cooldown from list once timer is at 0
        private void ProcessCooldowns()
        {
            float deltaTime = Time.deltaTime;

            for (int i = cooldowns.Count - 1; i >= 0; i--)
            {
                if (cooldowns[i].Decrease(deltaTime))
                {
                    cooldowns.RemoveAt(i);
                }
            }
        }
    }


    /// <summary>
    /// Gets Id and cooldown duration from script
    /// </summary>
    public class CooldownData
    {
        public CooldownData(activeCooldown cooldown)
        {
            Id = cooldown.Id;
            timeRemaining = cooldown.CooldownDuration;
        }

        public int Id { get; }
        public float timeRemaining { get; private set; }

        
        /// <summary>
        /// Decrases the time and keeps at 0f
        /// </summary>

        public bool Decrease(float deltaTime)
        {
            timeRemaining = Mathf.Max(timeRemaining - deltaTime, 0);

            return timeRemaining == 0f;
        }
    }
