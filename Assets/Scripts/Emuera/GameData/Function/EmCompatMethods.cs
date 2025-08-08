using System;
using MinorShift.Emuera.GameData.Expression;

namespace MinorShift.Emuera.GameData.Function
{
    // Basic EM/EE compatibility helpers (stubbed minimal implementations)

    internal sealed class GetVarMethod : FunctionMethod
    {
        public GetVarMethod()
        {
            ReturnType = typeof(Int64);
            argumentTypeArray = new Type[] { typeof(string) };
            CanRestructure = true;
        }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
        {
            // No global store available here – return 0 by default
            return 0L;
        }
    }

    internal sealed class ExistVarMethod : FunctionMethod
    {
        public ExistVarMethod()
        {
            ReturnType = typeof(Int64);
            argumentTypeArray = new Type[] { typeof(string) };
            CanRestructure = true;
        }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments)
        {
            return 0L;
        }
    }

    internal sealed class MapHasMethod : FunctionMethod
    {
        public MapHasMethod() { ReturnType = typeof(Int64); argumentTypeArray = null; CanRestructure = true; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }
    internal sealed class MapExistMethod : FunctionMethod
    {
        public MapExistMethod() { ReturnType = typeof(Int64); argumentTypeArray = null; CanRestructure = true; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }
    internal sealed class MapGetMethod : FunctionMethod
    {
        public MapGetMethod() { ReturnType = typeof(string); argumentTypeArray = null; CanRestructure = true; }
        public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments) { return string.Empty; }
    }

    internal sealed class XmlExistMethod : FunctionMethod
    {
        public XmlExistMethod() { ReturnType = typeof(Int64); argumentTypeArray = null; CanRestructure = true; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }
    internal sealed class XmlGetByNameMethod : FunctionMethod
    {
        public XmlGetByNameMethod() { ReturnType = typeof(string); argumentTypeArray = null; CanRestructure = true; }
        public override string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments) { return string.Empty; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }

    internal sealed class EnumFilesMethod : FunctionMethod
    {
        public EnumFilesMethod() { ReturnType = typeof(Int64); argumentTypeArray = null; CanRestructure = true; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }

    internal sealed class ClearMemoryMethod : FunctionMethod
    {
        public ClearMemoryMethod() { ReturnType = typeof(Int64); argumentTypeArray = null; CanRestructure = true; }
        public override Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { return 0L; }
    }
}


