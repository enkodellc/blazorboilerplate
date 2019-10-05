using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

// this creates a self signed CA, generates a new certificate based on it and saves that cert to the local file system.
namespace TempCertGenerator
{
    public static class Program
    {
        static string certFileName = "AuthSample.pfx"; // the name of the new certificate file
        static string certPassword = "Admin123"; // The password for the new certificate
        static string certSubjectName = "TEST"; //Subject Name

        public static void Main(string[] args)
        {
            
            AsymmetricKeyParameter myCAprivateKey = null;
            Console.WriteLine("Creating CA");
            X509Certificate2 certificateAuthorityCertificate = CreateCertificateAuthorityCertificate("CN=" + certSubjectName + "CA", ref myCAprivateKey);

            SaveCertToLocalFile(certificateAuthorityCertificate, "tempRootCACert.pfx", args[0]);

            Console.WriteLine("Creating certificate based on CA");
            X509Certificate2 certificate = CreateSelfSignedCertificateBasedOnCertificateAuthorityPrivateKey("CN=" + certSubjectName, "CN=" + certSubjectName + "CA", myCAprivateKey);
            Console.WriteLine("Saving New Certificate to File: " + certFileName);

            SaveCertToLocalFile(certificate, certFileName, args[0]);
        }

        private static void SaveCertToLocalFile(X509Certificate2 cert, string fileName, string filePath)
        {
            var certBuffer = cert.Export(X509ContentType.Pfx, certPassword);

            try
            {
                File.WriteAllBytes(filePath + "/" + fileName, certBuffer);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }
        }

        private static X509Certificate2 CreateCertificateAuthorityCertificate(string subjectName, ref AsymmetricKeyParameter CaPrivateKey)
        {
            const int keyStrength = 2048;

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();

            // Serial Number
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);
            
            // Issuer and Subject Name
            X509Name subjectDN = new X509Name(subjectName);
            X509Name issuerDN = subjectDN;
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddMonths(2);

            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            KeyGenerationParameters keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // Generating the Certificate
            AsymmetricCipherKeyPair issuerKeyPair = subjectKeyPair;
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", issuerKeyPair.Private, random);
            // selfsign certificate
            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
            X509Certificate2 x509 = new X509Certificate2(certificate.GetEncoded());

            CaPrivateKey = issuerKeyPair.Private;

            return x509;
        }
        private static X509Certificate2 CreateSelfSignedCertificateBasedOnCertificateAuthorityPrivateKey(string subjectName, string issuerName, AsymmetricKeyParameter issuerPrivKey)
        {
            const int keyStrength = 2048;

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", issuerPrivKey, random);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, true, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

            // Serial Number
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            X509Name subjectDN = new X509Name("CN=" + subjectName);
            X509Name issuerDN = new X509Name("CN=" + issuerName);
            certificateGenerator.SetIssuerDN(issuerDN);
            certificateGenerator.SetSubjectDN(subjectDN);

            // Valid For
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddMonths(2);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            // Subject Public Key
            AsymmetricCipherKeyPair subjectKeyPair;
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);

            // selfsign certificate
            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

            // merge into X509Certificate2
            X509Certificate2 x509i = new X509Certificate2(DotNetUtilities.ToX509Certificate(certificate));
            var rsa = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)subjectKeyPair.Private);
            X509Certificate2 x509 = x509i.CopyWithPrivateKey(rsa);
            x509.FriendlyName = subjectName;

            return x509;
        }

        private static AsymmetricAlgorithm ToDotNetKey(RsaPrivateCrtKeyParameters privateKey)
        {
            var cspParams = new CspParameters()
            {
                KeyContainerName = Guid.NewGuid().ToString(),
                KeyNumber = (int)KeyNumber.Exchange,
                Flags = CspProviderFlags.UseMachineKeyStore
            };

            var rsaProvider = new RSACryptoServiceProvider(cspParams);
            var parameters = new RSAParameters()
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
            };

            rsaProvider.ImportParameters(parameters);
            return rsaProvider;
        }
    }
}