using System;

namespace BitmaskExpressions
{
    class Program
    {
        static void Main(string[] args)
        {
            var expression = "A | (B & C)";

            Console.WriteLine($"Expression:\n\n  {expression}\n");

            // Parse expression
            var parser = new Parser(expression);
            var expr = parser.ParseExpression();

            // Log it
            Console.WriteLine("AST:\n");
            var logger = new Logger();
            logger.Log(expr);
            Console.WriteLine();

            // Plan it
            var planner = new Planner();
            var plan = planner.GetExecPlan(expr);
            Console.WriteLine($"Plan:\n\n  {plan}\n");

            Console.WriteLine("Test: (expected vs actual)\n");
            var evaluator = new Evaluator();
            for (uint i = 0; i < 16; i++)
            {
                var expected = evaluator.Evaluate(expr, i) ? "TRUE " : "false";
                var actual = plan.Evaluate(i) ? "TRUE " : "false";
                Console.WriteLine($"  {Convert.ToString(i, 2).PadLeft(4, '0')} => {expected} vs {actual} {(expected != actual ? "Fail" : "✓")}");
            }
        }
    }
}
