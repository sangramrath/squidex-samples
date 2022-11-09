﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using CommandDotNet;
using ConsoleTables;
using FluentValidation;
using Squidex.CLI.Commands.Implementation;
using Squidex.CLI.Configuration;

namespace Squidex.CLI.Commands;

public partial class App
{
    [Command("config", Description = "Manage configurations.")]
    [Subcommand]
    public sealed class Config
    {
        private readonly IConfigurationService configuration;
        private readonly ILogger log;

        public Config(IConfigurationService configuration, ILogger log)
        {
            this.configuration = configuration;

            this.log = log;
        }

        [Command("list", Description = "Shows the current configuration.")]
        public void List(ListArguments arguments)
        {
            var config = configuration.GetConfiguration();

            if (arguments.Table)
            {
                var table = new ConsoleTable("Name", "App", "ClientId", "ClientSecret", "Url");

                foreach (var (key, app) in config.Apps)
                {
                    table.AddRow(key, app.Name, app.ClientId, app.ClientSecret.Truncate(10), app.ServiceUrl);
                }

                table.Write();

                log.WriteLine();
                log.WriteLine("Current App: {0}", config.CurrentApp);
            }
            else
            {
                log.WriteLine(config.JsonPrettyString());
            }
        }

        [Command("add", Description = "Add or update an app.")]
        public void Add(AddArguments arguments)
        {
            configuration.Upsert(arguments.ToEntryName(), arguments.ToModel());

            if (arguments.Use)
            {
                configuration.UseApp(arguments.Name);

                log.WriteLine("> App added and selected.");
            }
            else
            {
                log.WriteLine("> App added.");
            }
        }

        [Command("use", Description = "Use an app.")]
        public void Use(UseArguments arguments)
        {
            configuration.UseApp(arguments.Name);

            log.WriteLine("> App selected.");
        }

        [Command("remove", Description = "Remove an app.")]
        public void Remove(RemoveArguments arguments)
        {
            configuration.Remove(arguments.Name);

            log.WriteLine("> App removed.");
        }

        [Command("reset", Description = "Reset the config.")]
        public void Reset()
        {
            configuration.Reset();

            log.WriteLine("> Config reset.");
        }

        public sealed class ListArguments : IArgumentModel
        {
            [Option('t', "table", Description = "Output as table.")]
            public bool Table { get; set; }

            public sealed class Validator : AbstractValidator<ListArguments>
            {
            }
        }

        public sealed class RemoveArguments : IArgumentModel
        {
            [Operand("name", Description = "The name of the app.")]
            public string Name { get; set; }

            public sealed class Validator : AbstractValidator<RemoveArguments>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).NotEmpty();
                }
            }
        }

        public sealed class UseArguments : IArgumentModel
        {
            [Operand("name", Description = "The name of the app.")]
            public string Name { get; set; }

            public sealed class Validator : AbstractValidator<UseArguments>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).NotEmpty();
                }
            }
        }

        public sealed class AddArguments : IArgumentModel
        {
            [Operand("name", Description = "The name of the app.")]
            public string Name { get; set; }

            [Operand("client-id", Description = "The client id.")]
            public string ClientId { get; set; }

            [Operand("client-secret", Description = "The client secret.")]
            public string ClientSecret { get; set; }

            [Option('u', "url", Description = "The optional url to your squidex installation. Default: https://cloud.squidex.io.")]
            public string ServiceUrl { get; set; }

            [Option('l', "label", Description = "Optional label for this app.")]
            public string Label { get; set; }

            [Option('c', "create", Description = "Create the app if it does not exist (needs admin client).")]
            public bool Create { get; set; }

            [Option('i', "ignore-self-signed", Description = "Ignores self signed certificates.")]
            public bool IgnoreSelfSigned { get; set; }

            [Option("use", Description = "Use the config.")]
            public bool Use { get; set; }

            public string ToEntryName()
            {
                return !string.IsNullOrWhiteSpace(Label) ? Label : Name;
            }

            public ConfiguredApp ToModel()
            {
                return new ConfiguredApp
                {
                    Name = Name,
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    IgnoreSelfSigned = IgnoreSelfSigned,
                    ServiceUrl = ServiceUrl
                };
            }

            public sealed class Validator : AbstractValidator<AddArguments>
            {
                public Validator()
                {
                    RuleFor(x => x.Name).NotEmpty();
                    RuleFor(x => x.ClientId).NotEmpty();
                    RuleFor(x => x.ClientSecret).NotEmpty();
                }
            }
        }
    }
}
