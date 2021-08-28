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
        public ExecPlan(ExecPlanKind kind, ulong mask = 0, ulong value = 0)
        {
            Kind = kind;
            Mask = mask;
            TestValue = value;
        }

        /// <summary>
        /// Constructs a new node plan
        /// </summary>
        /// <param name="kind">The plan mode</param>
        /// <param name="inputPlans">The input plans</param>
        public ExecPlan(ExecPlanKind kind, List<ExecPlan> inputPlans)
        {
            Kind = kind;
            InputPlans = inputPlans;
        }

        /// <summary>
        /// The kind of execution plan
        /// </summary>
        public ExecPlanKind Kind;

        /// <summary>
        /// The mask for MaskAndValue and MaskAndNZ
        /// </summary>
        public ulong Mask;

        /// <summary>
        /// The test value for MaskAndValue
        /// </summary>
        public ulong TestValue;

        /// <summary>
        /// A list of input plans for nodes that can't be executed
        /// directly through bit operations (EvalAnd, EvalOr and EvalNot)
        /// </summary>
        public List<ExecPlan> InputPlans;

        /// <summary>
        /// Try to convert a MaskNotEqual plan to MaskEqual
        /// (can only do this when a single bit is set)
        /// </summary>
        /// <returns>A converted plan, or the same plan</returns>
        public ExecPlan ConvertMaskEqualIfCan()
        {
            if (Kind != ExecPlanKind.MaskNotEqual)
                return this;

            if (IsSingleBit(Mask))
            {
                return new ExecPlan(ExecPlanKind.MaskEqual, Mask, TestValue ^ Mask);
            }

            return this;
        }

        /// <summary>
        /// Try to convert a MaskEqual plan to MaskNotEqual
        /// (can only do this when a single bit is set)
        /// </summary>
        /// <returns>A converted plan, or the same plan</returns>
        public ExecPlan ConvertMaskNotEqualIfCan()
        {
            if (Kind != ExecPlanKind.MaskEqual)
                return this;

            if (IsSingleBit(Mask))
            {
                return new ExecPlan(ExecPlanKind.MaskNotEqual, Mask, TestValue ^ Mask);
            }

            return this;
        }

        /// <summary>
        /// Evaluate this execution plan
        /// </summary>
        /// <param name="input">The input number</param>
        /// <returns>The result of the expression</returns>
        public bool Evaluate(ulong input)
        {
            switch (Kind)
            {
                case ExecPlanKind.True: 
                    return true;

                case ExecPlanKind.False: 
                    return false;

                case ExecPlanKind.MaskEqual: 
                    return (input & Mask) == TestValue;

                case ExecPlanKind.MaskNotEqual: 
                    return (input & Mask) != TestValue;

                case ExecPlanKind.EvalAnd: 
                    return InputPlans.All(x => x.Evaluate(input));

                case ExecPlanKind.EvalOr: 
                    return InputPlans.Any(x => x.Evaluate(input));
                case ExecPlanKind.EvalNot: return !InputPlans[0].Evaluate(input);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if a ulong has exactly one bit set
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSingleBit(ulong value)
        {
            return (System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)value) + 
                    System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)(value >> 32))) == 1;
        }

        /// <summary>
        /// Provide a description of this execution plan
        /// </summary>
        /// <returns>A string description</returns>
        public override string ToString()
        {
            switch (Kind)
            {
                case ExecPlanKind.True: return "true";
                case ExecPlanKind.False: return "false";
                case ExecPlanKind.MaskEqual: return $"(input & 0x{Mask:X}) == 0x{TestValue:X}";
                case ExecPlanKind.MaskNotEqual: return $"(input & 0x{Mask:X}) != 0x{TestValue:X}";
                case ExecPlanKind.EvalAnd: return string.Join(" && ", InputPlans.Select(x => $"({x})"));
                case ExecPlanKind.EvalOr: return string.Join(" || ", InputPlans.Select(x => $"({x})"));
                case ExecPlanKind.EvalNot: return $"!({InputPlans[0]})";
            }
            throw new NotImplementedException();
        }
    }
}
