﻿using System;
using System.Linq.Expressions;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ConvensionTest : AutoMapperSpecBase
    {
        public class Client
        {
            public int ID { get; set; }
            public string Value { get; set; }
            public string Transval { get; set; }
        }

        public class ClientDto
        {
            public int ID { get; set; }
            public string ValueTransfer { get; set; }
            public string val { get; set; }
        }

        [Fact]
        public void Fact()
        {

            //Mapper.AddConvension().Postfix("Dto");

            Mapper.Initialize(cfg =>
            {
                var profile = cfg.CreateProfile("New Profile");
                profile.ProfileConfiguration.MemberConfigurations[0].AddName<PrePostfixName>(
                        _ => _.AddStrings(p => p.DestinationPostfixes, "Transfer")
                            .AddStrings(p => p.Postfixes, "Transfer")
                            .AddStrings(p => p.DestinationPrefixes, "Trans")
                            .AddStrings(p => p.Prefixes, "Trans"))
                            .SetMemberInfo<FieldPropertyMemberInfo>();
                profile.ProfileConfiguration.AddConditionalObjectMapper("New Profile").Where((s, d) => s.Name.Contains(d.Name) || d.Name.Contains(s.Name));
            });

            var a2 = Mapper.Map<ClientDto>(new Client() { Value= "Test", Transval = "test"});
            a2.ValueTransfer.ShouldEqual("Test");
            a2.val.ShouldEqual("test");

            var a = Mapper.Map<Client>(new ClientDto() { ValueTransfer = "TestTransfer", val = "testTransfer"});
            a.Value.ShouldEqual("TestTransfer");
            a.Transval.ShouldEqual("testTransfer");

            var clients = Mapper.Map<Client[]>(new[] { new ClientDto() });
            Expression<Func<Client, bool>> expr = c => c.ID < 5;
            var clientExp = Mapper.Map<Expression<Func<ClientDto,bool>>>(expr);
        }
    }
}