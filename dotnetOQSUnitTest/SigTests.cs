﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQuantumSafe;
using System.Linq;

namespace dotnetOQSUnitTest
{
    [TestClass]
    public class SigTests
    {
        private static void log(string message)
        {
            Console.WriteLine(message);
        }

        private static string BytesToHex(byte[] b)
        {
            return BitConverter.ToString(b).Replace("-", "");
        }

        private static void TestSig(string sigAlg)
        {
            byte[] public_key;
            byte[] secret_key;
            byte[] signature;
            byte[] message = new byte[100];
            Random random = new Random();
            random.NextBytes(message);
            log("message: " + BytesToHex(message));

            // successful case
            Sig sig = new Sig(sigAlg);
            if (sigAlg != "DEFAULT")
            {
                Assert.AreEqual(sig.AlgorithmName, sigAlg);
            }
            Assert.IsTrue(sig.IsUsable, "IsUsable after constructor");

            sig.keypair(out public_key, out secret_key);
            log("public_key: " + BytesToHex(public_key));
            log("secret_key: " + BytesToHex(secret_key));
            Assert.IsTrue((UInt64)public_key.Length <= sig.PublicKeyLength, "public key length");
            Assert.IsTrue((UInt64)secret_key.Length <= sig.SecretKeyLength, "secret key length");

            sig.sign(out signature, message, secret_key);
            log("signature: " + BytesToHex(signature));
            Assert.IsTrue((UInt64)signature.Length <= sig.SignatureLength, "signature length");
            Assert.IsTrue(sig.verify(message, signature, public_key), "signature verification");

            // failure cases

            // wrong message
            byte[] wrong_message = new byte[100];
            random.NextBytes(wrong_message);
            log("wrong_message: " + BytesToHex(wrong_message));
            Assert.IsFalse(sig.verify(wrong_message, signature, public_key), "wrong message, verification should have failed");

            // wrong signature
            byte[] wrong_signature = new byte[signature.Length];
            random.NextBytes(wrong_signature);
            log("wrong_signature: " + BytesToHex(wrong_signature));
            Assert.IsFalse(sig.verify(message, wrong_signature, public_key), "wrong signature, verification should have failed");

            // wrong public key
            byte[] wrong_public_key = new byte[public_key.Length];
            random.NextBytes(wrong_public_key);
            log("wrong_public_key: " + BytesToHex(wrong_public_key));
            Assert.IsFalse(sig.verify(message, signature, wrong_public_key), "wrong public key, verification should have failed");

            // clean-up
            sig.Dispose();
            Assert.IsFalse(sig.IsUsable, "IsUsable after cleanup");
        }

        [TestMethod]
        public void TestSigOQSDefault()
        {
            TestSig("DEFAULT");
        }

        [TestMethod]
        public void TestSigPicnicL1FS()
        {
            TestSig("picnic_L1_FS");
        }

        [TestMethod]
        public void TestSigPicnicL1UR()
        {
            TestSig("picnic_L1_UR");
        }

        [TestMethod]
        public void TestSigPicnicL3FS()
        {
            TestSig("picnic_L3_FS");
        }

        [TestMethod]
        public void TestSigPicnicL3UR()
        {
            TestSig("picnic_L3_UR");
        }

        [TestMethod]
        public void TestSigPicnicL5FS()
        {
            TestSig("picnic_L5_FS");
        }

        [TestMethod]
        public void TestSigPicnicL5UR()
        {
            TestSig("picnic_L5_UR");
        }

        [TestMethod]
        public void TestSigNotSupported()
        {
            Assert.ThrowsException<MechanismNotSupportedException>(() => new KEM("bogus"));
        }

        [TestMethod]
        public void TestSigNotEnabled()
        {
            // find a supported-but-not-enabled mechanism
            foreach (string supported in Sig.SupportedMechanisms)
            {
                if (!Sig.EnabledMechanisms.Contains(supported))
                {
                    // found one
                    Assert.ThrowsException<MechanismNotEnabledException>(() => new Sig(supported));
                }
            }
        }
    }
}
