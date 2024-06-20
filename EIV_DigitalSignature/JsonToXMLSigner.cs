using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class JsonXmlSigner
{
    private readonly string privateKeyPath;
    private readonly string publicKeyPath;

    public JsonXmlSigner(string privateKeyPath = "private_key.pem", string publicKeyPath = "public_key.pem")
    {
        this.privateKeyPath = privateKeyPath;
        this.publicKeyPath = publicKeyPath;
    }

    public RSA LoadPrivateKey()
    {
        var privateKey = File.ReadAllBytes(privateKeyPath);
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        return rsa;
    }

    public string JsonToXml(string jsonString)
    {
        var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        var xmlDoc = new XDocument(new XElement("root", ConvertJsonToXml(json)));
        return xmlDoc.ToString(SaveOptions.DisableFormatting);
    }

    private IEnumerable<XElement> ConvertJsonToXml(Dictionary<string, object> jsonDict)
    {
        var elements = new List<XElement>();
        foreach (var kvp in jsonDict)
        {
            if (kvp.Value is JObject nestedObj)
            {
                elements.Add(new XElement(kvp.Key, ConvertJsonToXml(nestedObj.ToObject<Dictionary<string, object>>())));
            }
            else if (kvp.Value is JArray nestedArray)
            {
                elements.Add(new XElement(kvp.Key, nestedArray.Select(item => new XElement("item", item))));
            }
            else
            {
                elements.Add(new XElement(kvp.Key, kvp.Value));
            }
        }
        return elements;
    }

    public void SignXml(string jsonFilePath, string outputPath)
    {
        string jsonString = File.ReadAllText(jsonFilePath);
        string xmlString = JsonToXml(jsonString);

        var xmlDoc = new XmlDocument();
        xmlDoc.PreserveWhitespace = true;
        xmlDoc.LoadXml(xmlString);

        var signedXml = new SignedXml(xmlDoc);
        signedXml.SigningKey = LoadPrivateKey();

        var reference = new Reference();
        reference.Uri = "";
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        signedXml.AddReference(reference);

        signedXml.ComputeSignature();
        var xmlDigitalSignature = signedXml.GetXml();
        xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

        using (var xmlWriter = XmlWriter.Create(outputPath, new XmlWriterSettings { Indent = true }))
        {
            xmlDoc.WriteTo(xmlWriter);
        }

        Console.WriteLine("Document signed successfully.");
    }
}
