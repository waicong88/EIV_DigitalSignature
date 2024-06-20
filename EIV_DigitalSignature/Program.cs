using System;

public class Program
{
    public static void Main()
    {
        var jsonSigner = new JsonDigitalSignature();

        // Generate Keys
        jsonSigner.GenerateKeys();

        // Sign JSON
        jsonSigner.SignJson("C:\\Users\\wclau\\source\\repos\\EIV_DigitalSignature\\EIV_DigitalSignature\\invoice.json", "signed_invoice.json"); // Change to your JSON file path

        //Sign XML
        var jsonXmlSigner = new JsonXmlSigner();
        jsonXmlSigner.SignXml("C:\\Users\\wclau\\source\\repos\\EIV_DigitalSignature\\EIV_DigitalSignature\\invoice.json", "signed_invoice.xml"); // Change to your XML file path
    }
}
