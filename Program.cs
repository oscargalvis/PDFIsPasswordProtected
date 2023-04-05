using System;
using System.IO;

namespace PDFIsPasswordProtected
{
    public class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine(DetectPDFPassword(@"/Users/mr.o/Toptal/doc1.pdf"));
            Console.WriteLine();
            Console.WriteLine(DetectPDFPassword(@"/Users/mr.o/Toptal/doc1-readonly.pdf"));
            Console.WriteLine();
            Console.WriteLine(DetectPDFPassword(@"/Users/mr.o/Toptal/doc1-protected.pdf"));
            Console.WriteLine();
        }

        public static bool DetectPDFPassword(string pdfPath)
        {
            // References:
            // https://www.cs.cmu.edu/~dst/Adobe/Gallery/anon21jul01-pdf-encryption.txt
            // https://medium.com/aia-sg-techblog/implementing-encryption-feature-in-pdf-lib-112091bce9af
            // https://opensource.adobe.com/dc-acrobat-sdk-docs/standards/pdfstandards/pdf/PDF32000_2008.pdf
            Console.WriteLine(pdfPath);
            byte[] pdfBytes = File.ReadAllBytes(pdfPath);
            var pdfContent = System.Text.Encoding.UTF8.GetString(pdfBytes);
            int lastEncryptIndex = pdfContent.LastIndexOf("/Encrypt ");
            if (lastEncryptIndex > -1){
                var encryptKeyDict = pdfContent.Substring(lastEncryptIndex+1);               
                encryptKeyDict = encryptKeyDict.Substring(0, encryptKeyDict.IndexOf(@"/"))
                                    .Replace("R", "obj").Replace("Encrypt", String.Empty).Trim();                
                // Separator might be 0A or 0D
                return analyseEncryptionDictionary(pdfContent, encryptKeyDict, "\n") ||
                    analyseEncryptionDictionary(pdfContent, encryptKeyDict, "\r");
            }
            return false;
        }

        public static bool analyseEncryptionDictionary(string pdfContent, string encryptKeyDict, string separator){
            var token = $"{separator}{encryptKeyDict}{separator}";            
            var tokenIndex = pdfContent.IndexOf(token);
            if (tokenIndex > -1){
                var dict = pdfContent.Substring(tokenIndex);
                dict = dict.Substring(0, dict.IndexOf($"{separator}endobj{separator}"));
                // User password exists but not encrypted password
                if (dict.IndexOf("/U") > -1 && dict.IndexOf("/UE") < 0) { return true; }
            }
            return false;
        }
    }
}
