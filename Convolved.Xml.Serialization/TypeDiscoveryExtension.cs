using System;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Serialization.Advanced;

namespace Convolved.Xml.Serialization
{
    public class TypeDiscoveryExtension : SchemaImporterExtension
    {
        private Dictionary<string, Type> discoveredTypes;

        public override string ImportSchemaType(string name, string ns, XmlSchemaObject context,
            XmlSchemas schemas, XmlSchemaImporter importer, CodeCompileUnit compileUnit,
            CodeNamespace mainNamespace, CodeGenerationOptions options,
            CodeDomProvider codeProvider)
        {
            if (discoveredTypes == null)
                DiscoverTypes();
            string key = ns + name;
            Type type;
            if (!discoveredTypes.TryGetValue(key, out type))
                return base.ImportSchemaType(name, ns, context, schemas, importer, compileUnit,
                    mainNamespace, options, codeProvider);
            compileUnit.ReferencedAssemblies.Add(type.Assembly.FullName);
            XmlSchemaElement schemaElement = context as XmlSchemaElement;
            if ((schemaElement != null) && schemaElement.IsNillable &&
                (schemaElement.ElementSchemaType is XmlSchemaSimpleType))
                return String.Format("System.Nullable<{0}>", type.FullName);
            return type.FullName;
        }

        protected virtual void DiscoverTypes()
        {
            discoveredTypes = new Dictionary<string, Type>();
            string discoveryDirectoryName =
                Environment.GetEnvironmentVariable("ConvolvedXmlDiscoveryPath");
            if (string.IsNullOrEmpty(discoveryDirectoryName))
                discoveryDirectoryName = Environment.CurrentDirectory;
            if (!Directory.Exists(discoveryDirectoryName))
                return;
            string[] fileNames = Directory.GetFiles(discoveryDirectoryName, "*.dll",
                SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(fileName);
                    foreach (Type type in assembly.GetTypes())
                    {
                        XmlTypeAttribute xta = (XmlTypeAttribute)Attribute.GetCustomAttribute(type,
                            typeof(XmlTypeAttribute), false);
                        string ns = (xta != null) ? xta.Namespace : null;
                        if (string.IsNullOrEmpty(ns))
                            ns = "http://tempuri.org/";
                        string name = (xta != null) ? xta.TypeName : null;
                        if (string.IsNullOrEmpty(name))
                            name = type.Name;
                        string key = ns + name;
                        discoveredTypes[key] = type;
                    }
                }
                catch (BadImageFormatException)
                {
                    // Not a readable .NET assembly; ignore it.
                }
            }
        }
    }
}