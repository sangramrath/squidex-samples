﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;

#pragma warning disable IDE0017 // Simplify object initialization

namespace CodeGeneration;

public static class Program
{
    public static async Task Main()
    {
        var document = await OpenApiDocument.FromUrlAsync("https://localhost:5001/api/swagger/v1/swagger.json");

        SchemaCleaner.Clean(document);

        var generatorSettings = new CSharpClientGeneratorSettings();
        generatorSettings.ExceptionClass = "SquidexManagementException";
        generatorSettings.CSharpGeneratorSettings.ValueGenerator = new ValueGenerator(generatorSettings.CSharpGeneratorSettings);
        generatorSettings.CSharpGeneratorSettings.Namespace = "Squidex.ClientLibrary.Management";
        generatorSettings.CSharpGeneratorSettings.RequiredPropertiesMustBeDefined = false;
        generatorSettings.CSharpGeneratorSettings.ExcludedTypeNames = new[] { "JsonInheritanceConverter" };
        generatorSettings.CSharpGeneratorSettings.ArrayType = "System.Collections.Generic.List";
        generatorSettings.CSharpGeneratorSettings.ArrayInstanceType = "System.Collections.Generic.List";
        generatorSettings.CSharpGeneratorSettings.ArrayBaseType = "System.Collections.Generic.List";
        generatorSettings.CSharpGeneratorSettings.DictionaryBaseType = "System.Collections.Generic.Dictionary";
        generatorSettings.CSharpGeneratorSettings.DictionaryInstanceType = "System.Collections.Generic.Dictionary";
        generatorSettings.CSharpGeneratorSettings.DictionaryType = "System.Collections.Generic.Dictionary";
        generatorSettings.CSharpGeneratorSettings.TemplateDirectory = Directory.GetCurrentDirectory();
        generatorSettings.OperationNameGenerator = new TagNameGenerator();
        generatorSettings.GenerateOptionalParameters = true;
        generatorSettings.GenerateClientInterfaces = true;
        generatorSettings.UseBaseUrl = false;

        var sourceCode =
            new CSharpClientGenerator(document, generatorSettings)
                .GenerateFile();

        // Use a static version to keep the changes low.
        sourceCode = sourceCode.Replace("13.18.2.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v10.0.0.0))", "13.17.0.0 (NJsonSchema v10.8.0.0 (Newtonsoft.Json v9.0.0.0))");

        File.WriteAllText(@"..\..\..\..\Squidex.ClientLibrary\Management\Generated.cs", sourceCode);
    }

    public sealed class ValueGenerator : CSharpValueGenerator
    {
        public ValueGenerator(CSharpGeneratorSettings settings)
            : base(settings)
        {
        }

        public override string GetDefaultValue(JsonSchema schema, bool allowsNull, string targetType, string typeNameHint, bool useSchemaDefault, TypeResolverBase typeResolver)
        {
            if (!string.IsNullOrWhiteSpace(schema.ActualDiscriminator))
            {
                return string.Empty;
            }

            return base.GetDefaultValue(schema, allowsNull, targetType, typeNameHint, useSchemaDefault, typeResolver);
        }
    }

    public sealed class TagNameGenerator : MultipleClientsFromOperationIdOperationNameGenerator
    {
        public override string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
        {
            if (operation.Tags?.Count == 1)
            {
                return operation.Tags[0];
            }

            return base.GetClientName(document, path, httpMethod, operation);
        }
    }
}
