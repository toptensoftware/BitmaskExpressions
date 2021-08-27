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
        /// Get the execution plan for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public ExecPlan GetExecPlan(AstNode node)
        {
            return node.Visit(this);
        }

        public ExecPlan Visit(AstNodeIdentifier node)
        {
            return new ExecPlan(ExecPlanKind.MaskEqual, node.Bit, node.Bit);
        }

        public ExecPlan Visit(AstNodeAnd node)
        {
            // Get plans for every node
            var plans = node.Operands.Select(x => GetExecPlan(x).TryConvertMaskEqual()).ToList();

            // If any node is false, then result is false
            if (plans.Any(x => x.Mode == ExecPlanKind.False))
                return new ExecPlan(ExecPlanKind.False);

            // If all nodes are true, then result is true
            if (plans.All(x => x.Mode == ExecPlanKind.True))
                return new ExecPlan(ExecPlanKind.True);

            // Remove any nodes that are true as they don't contribute anything
            plans.RemoveAll(x => x.Mode == ExecPlanKind.True);

            // Combine any MaskEqual plans
            uint newMask = 0;
            uint newValue = 0;
            for (int i = plans.Count - 1; i >= 0; i--)
            {
                var p = plans[i];
                if (p.Mode == ExecPlanKind.MaskEqual)
                {
                    // Remove from the collection
                    plans.RemoveAt(i);

                    // Impossible condition?
                    if ((newValue & p.Mask & newMask) != (p.Value & p.Mask & newMask))
                    {
                        return new ExecPlan(ExecPlanKind.False);
                    }
                    newMask |= p.Mask;
                    newValue |= p.Value;
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

        public ExecPlan Visit(AstNodeOr node)
        {
            // Get plans for every node
            var plans = node.Operands.Select(x => GetExecPlan(x).TryConvertMaskNotEqual()).ToList();

            // If any node is true, then result is true
            if (plans.Any(x => x.Mode == ExecPlanKind.True))
                return new ExecPlan(ExecPlanKind.True);

            // If all nodes are false, then result is false
            if (plans.All(x => x.Mode == ExecPlanKind.False))
                return new ExecPlan(ExecPlanKind.False);

            // Remove any nodes that are false as they don't contribute anything
            plans.RemoveAll(x => x.Mode == ExecPlanKind.False);

            // Combine MaskNotEqual plans
            uint newMask = 0;
            uint newValue = 0;
            for (int i = plans.Count - 1; i >= 0; i--)
            {
                var p = plans[i];
                if (p.Mode == ExecPlanKind.MaskNotEqual)
                {
                    // Remove from list
                    plans.RemoveAt(i);

                    // Always true condition
                    if ((newValue & p.Mask & newMask) != (p.Value & p.Mask & newMask))
                    {
                        return new ExecPlan(ExecPlanKind.True);
                    }
                    newMask |= p.Mask;
                    newValue |= p.Value;
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

        public ExecPlan Visit(AstNodeNot node)
        {
            // Get the input plan
            var inPlan = GetExecPlan(node.Operand);

            // Handle input mode
            switch (inPlan.Mode)
            {
                case ExecPlanKind.True:
                    return new ExecPlan(ExecPlanKind.False);

                case ExecPlanKind.False:
                    return new ExecPlan(ExecPlanKind.True);

                case ExecPlanKind.MaskEqual:
                    return new ExecPlan(ExecPlanKind.MaskNotEqual, inPlan.Mask, inPlan.Value);

                case ExecPlanKind.MaskNotEqual:
                    return new ExecPlan(ExecPlanKind.MaskEqual, inPlan.Mask, inPlan.Value);
            }

            // Eval directly
            return new ExecPlan(ExecPlanKind.EvalNot, new List<ExecPlan>() { inPlan });
        }
    }
}
