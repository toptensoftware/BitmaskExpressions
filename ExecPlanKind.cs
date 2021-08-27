namespace BitmaskExpressions
{
    /// <summary>
    /// Represents the operation to be performance by an ExecPlan node.
    /// </summary>
    enum ExecPlanKind
    {
        /// <summary>
        /// The node always evaluates to true
        /// </summary>
        True,

        /// <summary>
        /// The node always evaluates to false
        /// </summary>
        False,

        /// <summary>
        /// The node is true if `(input & mask) == value`
        /// </summary>
        MaskEqual,

        /// <summary>
        /// The node is true if `(input & mask) != value`
        /// </summary>
        MaskNotEqual,

        /// <summary>
        /// The node can't be evaluated directly by bitwise math
        /// and must be split into a multiple operations. The result
        /// is true if all input plans are true.
        /// </summary>
        EvalAnd,

        /// <summary>
        /// The node can't be evaluated directly by bitwise math
        /// and must be split into a multiple operations. The result
        /// is true if any input plan is true.
        /// </summary>
        EvalOr,

        /// <summary>
        /// The node can't be evaluated directly by bitwise math
        /// and must be split into a multiple operations.  The result
        /// is the opposite of the input plan result.
        /// </summary>
        EvalNot,
    }
}
