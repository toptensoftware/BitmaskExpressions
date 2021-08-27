using System;

namespace BitmaskExpressions
{
    class Program
    {
        static void TestVerbose(string expression)
        {
            Console.WriteLine($"Expression:\n\n  {expression}\n");

            // Parse expression
            var parser = new Parser(expression);
            var expr = parser.ParseExpression();

            // Log it
            Console.WriteLine("AST:\n");
            var logger = new Logger();
            logger.Log(expr);
            Console.WriteLine();

            var bitNames = new BitFromLetter();

            // Plan it
            var planner = new Planner();
            var plan = planner.GetExecPlan(expr, bitNames);
            Console.WriteLine($"Plan:\n\n  {plan}\n");

            Console.WriteLine("Test: (expected vs actual)\n");
            var evaluator = new Evaluator();
            for (uint i = 0; i < 1024; i++)
            {
                var expected = evaluator.Evaluate(expr, bitNames, i) ? "TRUE " : "false";
                var actual = plan.Evaluate(i) ? "TRUE " : "false";
                Console.WriteLine($"  {Convert.ToString(i, 2).PadLeft(4, '0')} => {expected} vs {actual} {(expected != actual ? "Fail" : "✓")}");
            }
        }

        static void TestConcise(string expression)
        {
            Console.Write($"{expression}    =>    ");

            // Parse expression
            var parser = new Parser(expression);
            var expr = parser.ParseExpression();

            var bitNames = new BitFromLetter();

            // Plan it
            var planner = new Planner();
            var plan = planner.GetExecPlan(expr, bitNames);
            Console.WriteLine($"{plan}");

            var evaluator = new Evaluator();
            for (uint i = 0; i < 256; i++)
            {
                var expected = evaluator.Evaluate(expr, bitNames, i);
                var actual = plan.Evaluate(i);
                if (expected != actual)
                    Console.WriteLine($"  Failed for {Convert.ToString(i, 2).PadLeft(8, '0')}");
            }
        }

        static void Main(string[] args)
        {
            TestConcise("A");
            TestConcise("B");
            TestConcise("C");
            TestConcise("D");
            TestConcise("A && B");
            TestConcise("A || B");
            TestConcise("A && (B || C)");
            TestConcise("(A && B) || C");
            TestConcise("(A && B) || (C && D)");
            TestConcise("(A || B) && (C || D)");
        }
    }
}
