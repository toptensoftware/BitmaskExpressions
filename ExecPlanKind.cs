namespace BitmaskExpressions
{
    /// <summary>
    /// The mode with which a node a node can be executed
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
        /// and must be split into a multiple operations that must
        /// all be true
        /// </summary>
        EvalAnd,

        /// <summary>
        /// The node can't be evaluated directly by bitwise math
        /// and must be split into a multiple operations
        /// </summary>
        EvalOr,

        EvalNot,
    }
}
