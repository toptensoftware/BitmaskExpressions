using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    class Compiler
    {
        public Compiler()
        {
        }

        public static Func<T, bool> Compile<T>(string expression) where T: Enum
        {
            return (Func<T, bool>)Compile(typeof(T), expression);
        }

        public static Delegate Compile(Type typeArg, string expression)
        {
            // Parse
            var parser = new Parser(expression);
            var expr = parser.ParseExpression();

            // Get names
            var names = new EnumNames(typeArg);

            // Create plan
            var planner = new Planner();
            var plan = planner.GetExecPlan(expr, names);

            // Compile
            var compiler = new Compiler();
            return compiler.Compile(typeArg, plan);
        }

        public Func<T, bool> Compile<T>(ExecPlan plan) where T : Enum
        {
            return (Func<T, bool>)Compile(typeof(T), plan);
        }

        public Func<uint, bool> Compile(ExecPlan plan)
        {
            return (Func<uint, bool>)Compile(typeof(uint), plan);
        }

        public Delegate Compile(Type typeArg, ExecPlan plan)
        {
            // Create method
            var method = new DynamicMethod("BitmaskExpression", typeof(bool), new Type[] { typeArg }, true);
            _il = method.GetILGenerator();

            // Generate 32 or 64 bit operations?
            var underlyingType = typeArg;
            if (typeArg.IsEnum)
                underlyingType = Enum.GetUnderlyingType(typeArg);
            var underlyingTypeCode = Type.GetTypeCode(underlyingType);
            _emit64 = underlyingTypeCode == TypeCode.Int64 || underlyingTypeCode == TypeCode.UInt64;

            // Emit ode
            Emit(plan);

            // Return at end
            _il.Emit(OpCodes.Ret);

            // Create delegate
            var delType = typeof(Func<,>).MakeGenericType(typeArg, typeof(bool));
            return method.CreateDelegate(delType);
        }


        ILGenerator _il;
        bool _emit64;

        void EmitLoadConstant(ulong value)
        {
            if (_emit64)
                _il.Emit(OpCodes.Ldc_I8, (long)value);  // HRM: .NET requires long not ulong for reasons unknown
            else
                _il.Emit(OpCodes.Ldc_I4, (uint)value);
        }

        void Emit(ExecPlan plan)
        {
            switch (plan.Kind)
            {
                case ExecPlanKind.True:
                    _il.Emit(OpCodes.Ldc_I4_1);
                    break;

                case ExecPlanKind.False:
                    _il.Emit(OpCodes.Ldc_I4_0);
                    break;

                case ExecPlanKind.MaskEqual:
                    _il.Emit(OpCodes.Ldarg_0);
                    EmitLoadConstant(plan.Mask);
                    _il.Emit(OpCodes.And);
                    EmitLoadConstant(plan.TestValue);
                    _il.Emit(OpCodes.Ceq);
                    break;

                case ExecPlanKind.MaskNotEqual:
                    _il.Emit(OpCodes.Ldarg_0);
                    EmitLoadConstant(plan.Mask);
                    _il.Emit(OpCodes.And);
                    EmitLoadConstant(plan.TestValue);
                    _il.Emit(OpCodes.Ceq);
                    _il.Emit(OpCodes.Ldc_I4_0);
                    _il.Emit(OpCodes.Ceq);
                    break;

                case ExecPlanKind.EvalAnd:
                    {
                        var lblFalse = _il.DefineLabel();
                        var lblDone = _il.DefineLabel();
                        for (int i = 0; i < plan.InputPlans.Count; i++)
                        {
                            // Generate the input plan
                            Emit(plan.InputPlans[i]);
                            _il.Emit(OpCodes.Brfalse, lblFalse);
                        }
                        _il.Emit(OpCodes.Ldc_I4_1);
                        _il.Emit(OpCodes.Br_S, lblDone);
                        _il.MarkLabel(lblFalse);
                        _il.Emit(OpCodes.Ldc_I4_0);
                        _il.MarkLabel(lblDone);
                        return;
                    }

                case ExecPlanKind.EvalOr:
                    {
                        var lblTrue = _il.DefineLabel();
                        var lblDone = _il.DefineLabel();
                        for (int i = 0; i < plan.InputPlans.Count; i++)
                        {
                            // Generate the input plan
                            Emit(plan.InputPlans[i]);
                            _il.Emit(OpCodes.Brtrue, lblTrue);
                        }
                        _il.Emit(OpCodes.Ldc_I4_0);
                        _il.Emit(OpCodes.Br_S, lblDone);
                        _il.MarkLabel(lblTrue);
                        _il.Emit(OpCodes.Ldc_I4_1);
                        _il.MarkLabel(lblDone);
                        break;
                    }

                case ExecPlanKind.EvalNot:
                    {
                        Emit(plan.InputPlans[0]);
                        _il.Emit(OpCodes.Ldc_I4_0);
                        _il.Emit(OpCodes.Ceq);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
