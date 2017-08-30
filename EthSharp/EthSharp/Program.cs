﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EthSharp.Compiler;
using EthSharp.ContractDevelopment;
using HashLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EthSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = @"
            using EthSharp.ContractDevelopment;

            public class SimpleStorage {
                private UInt256 storedData;

                public void set(uint256 x) {
                    storedData = x;
                }

                public uint256 get(){
                    return storedData;
                }
            }";
            //parse tree - create assembly then emit.

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var assembly = new EthSharpCompiler().Create(tree);
            Console.ReadKey();
        }

        private static void CompileToCSharp(SyntaxTree tree)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var ethSharp = MetadataReference.CreateFromFile(typeof(UInt256).Assembly.Location);

            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            options = options.WithAllowUnsafe(true);                                //Allow unsafe code;
            options = options.WithOptimizationLevel(OptimizationLevel.Release);     //Set optimization level
            options = options.WithPlatform(Platform.X64);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib, ethSharp },
                options: options);
            var emitResult = compilation.Emit("output.dll", "output.pdb");

            //If our compilation failed, we can discover exactly why.
            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic.ToString());
                }
            }
        }
    }
}
