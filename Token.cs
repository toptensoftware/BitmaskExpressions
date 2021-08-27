namespace BitmaskExpressions
{
    /// <summary>
    /// Input tokens
    /// </summary>
    enum Token
    {
        /// <summary>
        /// An identifier
        /// </summary>
        Identifier,

        /// <summary>
        /// The boolean And operator `&&`
        /// </summary>
        OperatorAnd,

        /// <summary>
        /// The boolean Or operator `||`
        /// </summary>
        OperatorOr,

        /// <summary>
        /// The boolean Not operator `!`
        /// </summary>
        OperatorNot,

        /// <summary>
        /// An open round parenthesis `(`
        /// </summary>
        OpenRound,

        /// <summary>
        /// A closing round parenthesis `)`
        /// </summary>
        CloseRound,

        /// <summary>
        /// End of input stream
        /// </summary>
        EOF
    }
}
