using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// AST visitor to calculate an optimal execution plan
    /// </summary>
    class Planner : IAstNodeVisitor<ExecPlan>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Planner()
        {
        }

        /// <summary>
        /// Get the execution plan for an expression
        /// </summary>
        /// <param name="node"></param>
        /// <param name="bitNames">Bit name to mask mapper</param>
        /// <returns>An execution plan</returns>
        public ExecPlan GetExecPlan(AstNode node, IBitNames bitNames)
        {
            _bitNames = bitNames;
            return GetExecPlan(node);
        }

        /// <summary>
        /// Get the execution plan for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>An execution plan</returns>
        ExecPlan GetExecPlan(AstNode node)
        {
            return node.Visit(this);
        }

        ExecPlan IAstNodeVisitor<ExecPlan>.Visit(AstNodeIdentifier node)
        {
            var bit = _bitNames.BitFromName(node.Name);
            return new ExecPlan(ExecPlanKind.MaskEqual, bit, bit);
        }

        ExecPlan IAstNodeVisitor<ExecPlan>.Visit(AstNodeAnd node)
        {
            // Get plans for every node
            var plans = node.Operands.Select(x => GetExecPlan(x).ConvertMaskEqualIfCan()).ToList();

            // If any node is false, then result is false
            if (plans.Any(x => x.Kind == ExecPlanKind.False))
                return new ExecPlan(ExecPlanKind.False);

            // If all nodes are true, then result is true
            if (plans.All(x => x.Kind == ExecPlanKind.True))
                return new ExecPlan(ExecPlanKind.True);

            // Remove any nodes that are true as they don't contribute anything
            plans.RemoveAll(x => x.Kind == ExecPlanKind.True);

            // Combine any MaskEqual plans
            ulong newMask = 0;
            ulong newValue = 0;
            for (int i = plans.Count - 1; i >= 0; i--)
            {
                var p = plans[i];
                if (p.Kind == ExecPlanKind.MaskEqual)
                {
                    // Remove from the collection
                    plans.RemoveAt(i);

                    // Impossible condition?
                    if ((newValue & p.Mask & newMask) != (p.TestValue & p.Mask & newMask))
                    {
                        return new ExecPlan(ExecPlanKind.False);
                    }
                    newMask |= p.Mask;
                    newValue |= p.TestValue;
                }
            }

            // Add back the combined plan
            if (newMask != 0)
            {
                plans.Insert(0, new ExecPlan(ExecPlanKind.MaskEqual, newMask, newValue));
            }

            // If there's only one contributing plan, then that's the result
            if (plans.Count == 1)
                return plans[0];

            // Create a complex plan
            return new ExecPlan(ExecPlanKind.EvalAnd, plans);
        }

        ExecPlan IAstNodeVisitor<ExecPlan>.Visit(AstNodeOr node)
        {
            // Get plans for every node
            var plans = node.Operands.Select(x => GetExecPlan(x).ConvertMaskNotEqualIfCan()).ToList();

            // If any node is true, then result is true
            if (plans.Any(x => x.Kind == ExecPlanKind.True))
                return new ExecPlan(ExecPlanKind.True);

            // If all nodes are false, then result is false
            if (plans.All(x => x.Kind == ExecPlanKind.False))
                return new ExecPlan(ExecPlanKind.False);

            // Remove any nodes that are false as they don't contribute anything
            plans.RemoveAll(x => x.Kind == ExecPlanKind.False);

            // Combine MaskNotEqual plans
            ulong newMask = 0;
            ulong newValue = 0;
            for (int i = plans.Count - 1; i >= 0; i--)
            {
                var p = plans[i];
                if (p.Kind == ExecPlanKind.MaskNotEqual)
                {
                    // Remove from list
                    plans.RemoveAt(i);

                    // Always true condition
                    if ((newValue & p.Mask & newMask) != (p.TestValue & p.Mask & newMask))
                    {
                        return new ExecPlan(ExecPlanKind.True);
                    }
                    newMask |= p.Mask;
                    newValue |= p.TestValue;
                }
            }

            // Add back the combined plan
            if (newMask != 0)
            {
                plans.Insert(0, new ExecPlan(ExecPlanKind.MaskNotEqual, newMask, newValue));
            }

            // If there's only one contributing plan, then that's the result
            if (plans.Count == 1)
                return plans[0];

            return new ExecPlan(ExecPlanKind.EvalOr, plans);
        }

        ExecPlan IAstNodeVisitor<ExecPlan>.Visit(AstNodeNot node)
        {
            // Get the input plan
            var inPlan = GetExecPlan(node.Operand);

            // Handle input mode
            switch (inPlan.Kind)
            {
                case ExecPlanKind.True:
                    return new ExecPlan(ExecPlanKind.False);

                case ExecPlanKind.False:
                    return new ExecPlan(ExecPlanKind.True);

                case ExecPlanKind.MaskEqual:
                    return new ExecPlan(ExecPlanKind.MaskNotEqual, inPlan.Mask, inPlan.TestValue);

                case ExecPlanKind.MaskNotEqual:
                    return new ExecPlan(ExecPlanKind.MaskEqual, inPlan.Mask, inPlan.TestValue);
            }

            // Eval directly
            return new ExecPlan(ExecPlanKind.EvalNot, new List<ExecPlan>() { inPlan });
        }

        IBitNames _bitNames;
    }
}
