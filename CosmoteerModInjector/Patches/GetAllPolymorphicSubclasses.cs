using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Label = System.Reflection.Emit.Label;
using OpCode = Mono.Cecil.Cil.OpCode;

namespace CosmoteerModInjector.Patches;

public class GetAllPolymorphicSubclasses : IPatch
{
    public void Patch(Harmony harmony)
    {
        // have to use a workaround to get the target type
        // ReSharper disable once StringLiteralTypo
        const string patchingType = "Halfling.Serialization.Binary.BinarySerializer";
        const string patchingMethod = "GetAllPolymorphicSubclasses";

        Type? childType = Assemblies.EngineAssembly.GetType(patchingType)!;
        Type type = childType.BaseType!;
        if (type == null)
            throw new NullReferenceException($"{patchingType} found type was null");
        MethodInfo? method = type.GetMethod(patchingMethod, BindingFlags.Static | BindingFlags.NonPublic, new[]{typeof(Type), typeof(bool), typeof(bool)});

        // how is this not required??????
        // looking at GetAllPolymorphicSubclasses it should be required
        //harmony.Patch(method, prefix: new HarmonyMethod(Prefix), transpiler: new HarmonyMethod(Transpiler));
    }

    private static void Prefix(Type baseType, bool explicitOnly, bool oneNamePerClass)
    {
        Logger.Log(baseType.Name);
        MessageBox.Show($"prefix {baseType.Name}");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeInstruction?[] code = instructions.ToArray();

        // label target is first ldsfld op in method
        Label branchTargetLabel = generator.DefineLabel();
        bool found = false;
        foreach (CodeInstruction? instruction in code)
        {
            if (instruction!.opcode == OpCodes.Ldsfld)
            {
                instruction.labels.Add(branchTargetLabel);
                found = true;
                break;
            }
        }

        if (!found)
            throw new Exception("label not added - could not find target");

        int foundIndex = -1;
        for (int index = 0; index < code.Length; index++)
        {
            CodeInstruction instruction = code[index]!;
            if (instruction.opcode == OpCodes.Brfalse_S)
            {
                foundIndex = index;
                break;
            }
        }

        if (foundIndex == -1)
            throw new Exception("target branch instruction not found");

        code[foundIndex - 3] = null!; // ldloc.s
        code[foundIndex - 2] = null!; // ldloc.1
        code[foundIndex - 1] = null!; // call ==
        code[foundIndex] = new CodeInstruction(OpCodes.Br_S, branchTargetLabel);

        foreach (CodeInstruction? instruction in code)
        {
            if (instruction != null)
                yield return instruction;
        }
    }
}