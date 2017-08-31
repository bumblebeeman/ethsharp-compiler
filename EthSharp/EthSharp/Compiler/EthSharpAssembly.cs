﻿using System;
using System.Collections.Generic;
using System.Linq;
using EthSharp.ContractDevelopment;

namespace EthSharp.Compiler
{
    public class EthSharpAssembly
    {
        public List<EthSharpAssemblyItem> Items { get; private set; }

        public EthSharpAssembly()
        {
            Items = new List<EthSharpAssemblyItem>();
        }

        public EvmByteCode Assemble()
        {
            EvmByteCode ret = new EvmByteCode();

            //tagNumber: locations jumping from - NOTE THAT THESE WILL CHANGE ONCE WE WRAP THE WHOLE CONTRACT
            Dictionary<int, List<int>> jumpFromLocations = new Dictionary<int, List<int>>();
            Dictionary<int, int> tagLocations = new Dictionary<int, int>();
            foreach (var item in Items)
            {
                switch (item.Type)
                {
                    case AssemblyItemType.Operation:
                        ret.ByteCode.Add((byte) item.Instruction);
                        break;
                    case AssemblyItemType.Push:
                    {
                        int length = item.Data.ByteLength;
                        var sizedPushInstruction = EvmInstruction.PUSH1 - 1 + length;
                        
                        ret.ByteCode.Add((byte)sizedPushInstruction);
                        ret.ByteCode.AddRange(item.Data.ToByteArrayBE().Skip(32-length).Take(length));
                        break;
                    }
                    case AssemblyItemType.PushTag:
                    {
                        ret.ByteCode.Add((byte)EvmInstruction.PUSH1);
                        ret.ByteCode.Add((byte) 0); //This will get replaced with the location to jump to!
                        if (jumpFromLocations.ContainsKey(item.Data.ToInt()))
                        {
                            jumpFromLocations[item.Data.ToInt()].Add(ret.ByteCode.Count);
                        }
                        else
                        {
                            jumpFromLocations[item.Data.ToInt()] = new List<int>{ret.ByteCode.Count};
                        }
                        break;
                    }
                    case AssemblyItemType.Tag:
                        tagLocations.Add(item.Data.ToInt(), ret.ByteCode.Count)
                        ret.ByteCode.Add();
                        //assertThrow(i.data() != 0, AssemblyException, "");
                        //assertThrow(i.splitForeignPushTag().first == size_t(-1), AssemblyException, "Foreign tag.");
                        //assertThrow(ret.bytecode.size() < 0xffffffffL, AssemblyException, "Tag too large.");
                        //m_tagPositionsInBytecode[size_t(i.data())] = ret.bytecode.size();
                        //ret.bytecode.push_back((byte)Instruction::JUMPDEST);
                        throw new NotImplementedException();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            
            //TODO: remove
            Console.WriteLine(ret.ByteCode.ToArray().ToHexString());

            // The way solidity handles tags is by putting them in afterwards - here
            throw new NotImplementedException();
        }


        //public EthSharpAssemblyItem NewData(byte[] value)
        //{
        //    //AssemblyItem newData(bytes const& _data) { h256 h(dev::keccak256(asString(_data))); m_data[h] = _data; return AssemblyItem(PushData, h);

        //    throw new NotImplementedException();
        //}

        public void Append(EthSharpAssemblyItem assemblyItem)
        {
            Items.Add(assemblyItem);
        }

        public void Append(UInt256 value)
        {
            Items.Add(new EthSharpAssemblyItem(value));
        }

        public void Append(EvmInstruction instruction)
        {
            Items.Add(new EthSharpAssemblyItem(instruction));
        }

        //public void Append(byte[] value)
        //{
        //    Items.Add(NewData(value));
        //}
    }
}
