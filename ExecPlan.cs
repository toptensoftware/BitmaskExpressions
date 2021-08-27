using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// The plan for executing a node
    /// </summary>
    class ExecPlan
    {
        /// <summary>
        /// Constructs a new node plan
        /// </summary>
        /// <param name="kind">The plan mode</param>
        /// <param name="mask">The mask</param>
        /// <param name="value">The value</param>
        public ExecPlan(ExecPlanKind kind, uint mask = 0, uint value = 0)
        {
            Mode = kind;
            Mask = mask;
            Value = value;
        }

        /// <summary>
        /// Constructs a new node plan
        /// </summary>
        /// <param name="kind">The plan mode</param>
        /// <param name="subPlans">The input plans</param>
        public ExecPlan(ExecPlanKind kind, List<ExecPlan> subPlans)
        {
            Mode = kind;
            SubPlans = subPlans;
        }

        /// <summary>
        /// The mode
        /// </summary>
        public ExecPlanKind Mode;

        /// <summary>
        /// The mask for MaskAndValue and MaskAndNZ
        /// </summary>
        public uint Mask;

        /// <summary>
        /// The value for MaskAndValue
        /// </summary>
        public uint Value;

        /// <summary>
        /// A list of subplans for nodes that can't be executed
        /// directly through bit operations
        /// </summary>
        public List<ExecPlan> SubPlans;

        /// <summary>
        /// Try to convert a MaskNotEqual plan to MaskEqual
        /// (can only do this when a single bit is set)
        /// </summary>
        /// <returns>A converted plan, or the same plan</returns>
        public ExecPlan TryConvertMaskEqual()
        {
            if (Mode != ExecPlanKind.MaskNotEqual)
                return this;

            if (IsSingleBit(Mask))
            {
                return new ExecPlan(ExecPlanKind.MaskEqual, Mask, Value ^ Mask);
            }

            return this;
        }

        /// <summary>
        /// Try to convert a MaskEqual plan to MaskNotEqual
        /// (can only do this when a single bit is set)
        /// </summary>
        /// <returns>A converted plan, or the same plan</returns>
        public ExecPlan TryConvertMaskNotEqual()
        {
            if (Mode != ExecPlanKind.MaskEqual)
                return this;

            if (IsSingleBit(Mask))
            {
                return new ExecPlan(ExecPlanKind.MaskNotEqual, Mask, Value ^ Mask);
            }

            return this;
        }

        /// <summary>
        /// Evaluate this execution plan
        /// </summary>
        /// <param name="input">The input number</param>
        /// <returns>The result of the expression</returns>
        public bool Evaluate(uint input)
        {
            switch (Mode)
            {
                case ExecPlanKind.True: return true;
                case ExecPlanKind.False: return false;
                case ExecPlanKind.MaskEqual: return (input & Mask) == Value;
                case ExecPlanKind.MaskNotEqual: return (input & Mask) != Value;
                case ExecPlanKind.EvalAnd: return SubPlans.All(x => x.Evaluate(input));
                case ExecPlanKind.EvalOr: return SubPlans.Any(x => x.Evaluate(input));
                case ExecPlanKind.EvalNot: return !SubPlans[0].Evaluate(input);
            }

            throw new InvalidOperationException("Can't convert eval plan to function");
        }

        /// <summary>
        /// Check if a uint has exactly one bit set
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSingleBit(uint value)
        {
            return System.Runtime.Intrinsics.X86.Popcnt.PopCount(value) == 1;
        }

        /// <summary>
        /// Provide a description of this execution plan
        /// </summary>
        /// <returns>A string description</returns>
        public override string ToString()
        {
            switch (Mode)
            {
                case ExecPlanKind.True: return "True";
                case ExecPlanKind.False: return "False";
                case ExecPlanKind.MaskEqual: return $"(input & 0x{Mask:X2}) == 0x{Value:X2}";
                case ExecPlanKind.MaskNotEqual: return $"(input & 0x{Mask:X2}) != 0x{Value:X2}";
                case ExecPlanKind.EvalAnd: return string.Join(" && ", SubPlans.Select(x => $"({x})"));
                case ExecPlanKind.EvalOr: return string.Join(" || ", SubPlans.Select(x => $"({x})"));
                case ExecPlanKind.EvalNot: return $"!({SubPlans[0]})";
            }
            throw new NotImplementedException();
        }
    }
}
