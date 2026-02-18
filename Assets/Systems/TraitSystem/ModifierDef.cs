using System;
using UnityEngine;

namespace TechMogul.Traits
{
    [Serializable]
    public class ModifierDef
    {
        public StatType stat;
        public ModifierOp op;
        public float value;
        public EventCategory eventCategory;
        public ConditionDef condition;
        
        public bool IsConditional => condition != null;
        
        public bool ConditionMet(EmployeeConditionContext context)
        {
            if (condition == null)
            {
                return true;
            }
            return condition.Evaluate(context);
        }
        
        public float GetAbsoluteValue()
        {
            return Mathf.Abs(value);
        }
    }
}
