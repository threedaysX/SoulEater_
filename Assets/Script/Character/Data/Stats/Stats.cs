﻿using System;
using System.Linq;
using System.Collections.Generic;
using StatsModifierModel;
using UnityEngine;
using System.Collections;

namespace StatsModel
{
    [Serializable]
    public class Stats
    {
        public float BaseValue;
        public float AdditionalValue;
        public float Value
        {
            get
            {
                if (toForceChangeValue)
                {
                    return ForceValue;
                }
                if (isDirty || BaseValue != lastBaseValue || AdditionalValue != lastAdditionalValue)
                {
                    lastBaseValue = BaseValue;
                    lastAdditionalValue = AdditionalValue;
                    ResetDirtyFinalValue();
                }
                return FinalValue;
            }
        }

        [SerializeField] private float FinalValue;
        [SerializeField] private float ForceValue;    // Force to change value to this value (this would ignore BaseValue, Addition, Modifiers....)
        [SerializeField] protected bool toForceChangeValue = false;
        protected float lastBaseValue = float.MinValue;
        protected float lastAdditionalValue = float.MinValue;
        protected bool isDirty = true;
        public List<StatModifier> modifiers = new List<StatModifier>();

        public Stats()
        {
            modifiers = new List<StatModifier>();
        }
        public Stats(float baseValue) : this()
        {
            BaseValue = baseValue;
        }

        private float GetFinalValue()
        {
            float finalValue = BaseValue + AdditionalValue;
            float addMod = 0;
            float timesMod = 1;
            float timesOfAddMod = 0;

            foreach (var mod in modifiers)
            {
                switch (mod.Type)
                {
                    case StatModType.FlatAdd:
                        addMod += mod.Value;
                        break;
                    case StatModType.TimesTime:
                        timesMod *= (1 + mod.Value);
                        break;
                    case StatModType.TimesAdd:
                        timesOfAddMod += mod.Value;
                        break;
                    case StatModType.PercentageTime:
                        timesMod *= (1 + (mod.Value / 100));
                        break;
                    case StatModType.PercentageAdd:
                        timesOfAddMod += (mod.Value / 100);
                        break;
                }
            }

            // 數值變化皆為: 先[加減算] 後[乘算]
            if (addMod != 0)
            {
                finalValue += addMod;
            }
            if (timesOfAddMod != 0)
            {
                finalValue *= (1 + timesOfAddMod);
            }
            if (timesMod != 1)
            {
                finalValue *= timesMod;
            }

            // 取至小數點後4位
            return (float)Math.Round(finalValue, 4);
        }

        /// <summary>
        /// 增、減能力。
        /// </summary>
        /// <param name="mod">增加的數值</param>
        public void AddModifier(StatModifier mod)
        {
            if (mod.Value != 0)
            {
                modifiers.Add(mod);
                ResetDirtyFinalValue();
            }
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (mod.Value != 0)
            {
                var getTrueModInList = modifiers.FirstOrDefault(
                    item => item.SourceName != null 
                    && item.SourceName.Equals(mod.SourceName) 
                    && item.Type.Equals(mod.Type) && item.Value.Equals(mod.Value));
                if (modifiers.Remove(getTrueModInList))
                {
                    ResetDirtyFinalValue();
                    return true;
                }
            }
            return false;
        }


        public void ForceToChangeValue(float value)
        {
            toForceChangeValue = true;
            ForceValue = value;
        }

        public void CancelForceValue()
        {
            toForceChangeValue = false;
        }

        /// <summary>
        /// Notice: Always use this to clean stats on Awake, and then if you had some item changed this cleaned stats, plz set the item data again.
        /// </summary>
        public void ResetDirtyStats()
        {
            toForceChangeValue = false;
            modifiers.Clear();
            isDirty = true;
        }

        private void ResetDirtyFinalValue()
        {
            FinalValue = GetFinalValue();
            isDirty = false;
        }
    }
}
