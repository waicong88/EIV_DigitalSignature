using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class JsonDigitalSignature
{
    private readonly string privateKeyPath;
    private readonly string publicKeyPath;

    public JsonDigitalSignature(string privateKeyPath = "private_key.pem", string publicKeyPath = "public_key.pem")
    {
        this.privateKeyPath = privateKeyPath;
        this.publicKeyPath = publicKeyPath;
    }

    public void GenerateKeys()
    {
        using (var rsa = RSA.Create(2048))
        {
            // Export and save the private key
            var privateKey = rsa.ExportRSAPrivateKey();
            File.WriteAllBytes(privateKeyPath, privateKey);

            // Export and save the public key
            var publicKey = rsa.ExportRSAPublicKey();
            File.WriteAllBytes(publicKeyPath, publicKey);
        }

        Console.WriteLine("Keys generated and saved successfully.");
    }

    public void SignJson(string jsonFilePath, string signedJsonFilePath)
    {
        string jsonData = File.ReadAllText(jsonFilePath);
        Console.WriteLine("Original JSON data: " + jsonData);

        // Load the private key
        var privateKey = File.ReadAllBytes(privateKeyPath);

        // Sign the JSON string
        byte[] signature;
        using (var rsa = RSA.Create())
        {
            rsa.ImportRSAPrivateKey(privateKey, out _);
            var data = Encoding.UTF8.GetBytes(jsonData);
            signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }

        // Encode the signature in base64
        string signatureBase64 = Convert.ToBase64String(signature);
        Console.WriteLine("Generated Signature: " + signatureBase64);

        // Save the signed JSON with signature
        var signedJson = new
        {
            data = JsonConvert.DeserializeObject(jsonData),
            signature = signatureBase64
        };

        File.WriteAllText(signedJsonFilePath, JsonConvert.SerializeObject(signedJson, Newtonsoft.Json.Formatting.Indented));

        Console.WriteLine("Document signed successfully.");
    }

}
