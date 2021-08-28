using System;
using System.Diagnostics;

namespace BitmaskExpressions
{
    [Flags]
    enum Fruit
    {
        Apples = 0x01,
        Pears = 0x02,
        Bananas = 0x04,
    }

    class Program
    {
        bool Test(uint a, uint b)
        {
            return a != b;
        }

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

            // Compile it
            var compiler = new Compiler();
            var func = compiler.Compile(plan);


            Console.WriteLine("Test: (expected vs actual vs compiled)\n");
            var evaluator = new Evaluator();
            for (uint i = 0; i < 16; i++)
            {
                var expected = evaluator.Evaluate(expr, bitNames, i) ? "TRUE " : "false";
                var actual = plan.Evaluate(i) ? "TRUE " : "false";
                var comp = func(i) ? "TRUE " : "false";
                bool pass = actual == expected && comp == expected;
                Console.WriteLine($"  {Convert.ToString(i, 2).PadLeft(4, '0')} => {expected} vs {actual} vs {comp} {(pass ? "✓" : "Fail")}");
            }
        }

        static void TestConcise(string expression)
        {
            Console.Write($"{expression}  =>  ");

            // Parse expression
            var parser = new Parser(expression);
            var expr = parser.ParseExpression();

            var bitNames = new BitFromLetter();

            // Plan it
            var planner = new Planner();
            var plan = planner.GetExecPlan(expr, bitNames);
            Console.Write($"{plan}");

            // Evaluator
            var evaluator = new Evaluator();

            // Compile it
            var compiler = new Compiler();
            var func = compiler.Compile(plan);


            var swTree = new Stopwatch();
            var swOptTree = new Stopwatch();
            var swComp = new Stopwatch();

            for (int rep = 0; rep < 1000; rep++)
            {
                for (uint i = 0; i < 1024; i++)
                {
                    swTree.Start();
                    var expected = evaluator.Evaluate(expr, bitNames, i);
                    swTree.Stop();

                    swOptTree.Start();
                    var actual = plan.Evaluate(i);
                    swOptTree.Stop();

                    swComp.Start();
                    var comp = func(i);
                    swComp.Stop();

                    if (actual != expected || comp != expected)
                        Console.WriteLine($"  Failed for {Convert.ToString(i, 2).PadLeft(8, '0')}");
                }
            }

            Console.WriteLine($"  =>  {swTree.ElapsedMilliseconds} {swOptTree.ElapsedMilliseconds} {swComp.ElapsedMilliseconds}");
        }

        static bool Test(Fruit f)
        {
            return (f & Fruit.Apples) == Fruit.Apples;
        }

        static void Main(string[] args)
        {
            TestConcise("A && !A");
            TestConcise("A || !A");
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
            TestConcise("(A || B) && (C || D) && (A || C) && (A || D)");

            var func = Compiler.Compile<Fruit>("Apples && (Bananas || Pears)");
            Console.WriteLine(func(Fruit.Apples|Fruit.Bananas));
        }
    }
}
