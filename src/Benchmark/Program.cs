﻿using System;
using System.Collections.Generic;
using Benchmark.Flattening;

namespace Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var mappers = new Dictionary<string, IObjectToObjectMapper[]>
                {
                    { "Flattening", new IObjectToObjectMapper[] { new FlatteningMapper(), new ManualMapper(), new EquilvalentManualMapper(),  } },
                    { "Ctors", new IObjectToObjectMapper[] { new CtorMapper(), new ManualCtorMapper(),  } },
                    { "Complex", new IObjectToObjectMapper[] { new ComplexTypeMapper(), new ManualComplexTypeMapper() } },
                    { "Deep", new IObjectToObjectMapper[] { new DeepTypeMapper(), new ManualDeepTypeMapper() } }
                };
            //var mappers = new Dictionary<string, IObjectToObjectMapper[]>
            //{
            //    {"Flattening", new IObjectToObjectMapper[] {new ComplexTypeMapper()}},
            //};


            foreach (var pair in mappers)
            {
                foreach (var mapper in pair.Value)
                {
                    new BenchEngine(mapper, pair.Key).Start();
                }
            }
        }
    }
}