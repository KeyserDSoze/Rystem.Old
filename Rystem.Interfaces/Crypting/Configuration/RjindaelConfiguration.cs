using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rystem.Crypting
{
    public class RjindaelConfiguration : CryptoConfiguration, IRystemConfiguration
    {
        private const string DefaultPasswordHash = "A9@#d56_";
        private const string DefaultSaltKey = "7§hg!8@ò";
        private const string DefaultVIKey = "01tyç°@#78gh_jzx";

        public string PasswordHash => this.passowrdHash ?? DefaultPasswordHash;
        public string SaltKey => this.saltKey ?? DefaultSaltKey;
        public string VIKey => this.vIKey ?? DefaultVIKey;
        private CipherMode CipherMode => this.cipherMode ?? CipherMode.CBC;
        private PaddingMode PaddingMode => this.paddingMode ?? PaddingMode.ISO10126;
        private byte[] SaltKeyAsBytes => Encoding.ASCII.GetBytes(this.SaltKey);
        private byte[] VIKeyAsBytes => Encoding.ASCII.GetBytes(this.VIKey);
        private byte[] KeyAsBytes => new Rfc2898DeriveBytes(this.PasswordHash, this.SaltKeyAsBytes).GetBytes(256 / 8);
        public RijndaelManaged SymmetricKey => new RijndaelManaged() { Mode = CipherMode, Padding = PaddingMode };
        public ICryptoTransform Decryptor => this.SymmetricKey.CreateDecryptor(KeyAsBytes, VIKeyAsBytes);
        public ICryptoTransform Encryptor => this.SymmetricKey.CreateEncryptor(KeyAsBytes, VIKeyAsBytes);
        private readonly string passowrdHash;
        private readonly string saltKey;
        private readonly string vIKey;
        private readonly CipherMode? cipherMode;
        private readonly PaddingMode? paddingMode;
        public override CryptoType Type => CryptoType.Rijndael;
        public RjindaelConfiguration() { }
        public RjindaelConfiguration(string passwordHash, string saltKey, string vIKey)
        {
            this.passowrdHash = passwordHash;
            this.saltKey = saltKey;
            this.vIKey = vIKey;
        }
        public RjindaelConfiguration(CipherMode cipherMode, PaddingMode paddingMode)
        {
            this.cipherMode = cipherMode;
            this.paddingMode = paddingMode;
        }
        public RjindaelConfiguration(string passwordHash, string saltKey, string vIKey, CipherMode cipherMode, PaddingMode paddingMode) : this(passwordHash, saltKey, vIKey)
        {
            this.cipherMode = cipherMode;
            this.paddingMode = paddingMode;
        }

    }
}
