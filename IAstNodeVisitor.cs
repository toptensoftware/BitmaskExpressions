namespace BitmaskExpressions
{
    /// <summary>
    /// Visitor callback interface
    /// </summary>
    /// <typeparam name="T">The return type of the visitors</typeparam>
    interface IAstNodeVisitor<T>
    {
        T Visit(AstNodeIdentifier node);
        T Visit(AstNodeAnd node);
        T Visit(AstNodeOr node);
        T Visit(AstNodeNot node);
    }
}
