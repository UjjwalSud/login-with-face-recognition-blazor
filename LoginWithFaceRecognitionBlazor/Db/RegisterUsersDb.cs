using Fido2NetLib;
using Fido2NetLib.Objects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;

namespace LoginWithFaceRecognitionBlazor.Db
{
    public class RegisterUsersDb
    {
        /// <summary>
        /// Gets or sets the primary key for this user.
        /// </summary>
        public virtual int Id { get; set; }

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        public virtual string? UserName { get; set; }

        public virtual byte[]? UserId { get; set; }

        /// <summary>
        /// Gets or sets the public key for this user.
        /// </summary>
        public virtual byte[]? PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the user handle for this user.
        /// </summary>
        public virtual byte[]? UserHandle { get; set; }

        public virtual uint SignatureCounter { get; set; }

        public virtual string? CredType { get; set; }

        /// <summary>
        /// Gets or sets the registration date for this user.
        /// </summary>
        public virtual DateTimeOffset RegDate { get; set; }

        /// <summary>
        /// Gets or sets the Authenticator Attestation GUID (AAGUID) for this user.
        /// </summary>
        /// <remarks>
        /// An AAGUID is a 128-bit identifier indicating the type of the authenticator.
        /// </remarks>
        public virtual Guid AaGuid { get; set; }

        [NotMapped]
        public PublicKeyCredentialDescriptor? Descriptor
        {
            get { return string.IsNullOrWhiteSpace(DescriptorJson) ? null : JsonSerializer.Deserialize<PublicKeyCredentialDescriptor>(DescriptorJson); }
            set { DescriptorJson = JsonSerializer.Serialize(value); }
        }

        public virtual string? DescriptorJson { get; set; }
    }

    public class RegisterUserRepository
    {
        public static readonly List<RegisterUsersDb> Instance = [];

        public static bool AddNewUser(RegisterUsersDb registerUsersDb)
        {
            if (Instance.Any(x => x.UserName == registerUsersDb.UserName))
            {
                return false;
            }
            Instance.Add(registerUsersDb);
            return true;
        }

        public static ICollection<Fido2User> GetUsersByCredentialIdAsync(byte[] credentialId)
        {
            var credentialIdString = Base64Url.Encode(credentialId);
            Console.WriteLine(credentialIdString);
            var cred = Instance
           .Where(c => c.DescriptorJson != null && c.DescriptorJson.Contains(credentialIdString)).FirstOrDefault();

            if (cred == null || cred.UserId == null)
            {
                return new List<Fido2User>();
            }

            return Instance
               .Where(u => u.UserName != null && GetUserNameInBytes(u.UserName)
               .SequenceEqual(cred.UserId))
               .Select(u => new Fido2User
               {
                   DisplayName = u.UserName,
                   Name = u.UserName,
                   Id = GetUserNameInBytes(u.UserName) // byte representation of userID is required
               }).ToList();

        }

        public static List<RegisterUsersDb> GetUserByUserName(string userName)
        {
            return Instance.Where(x => x.UserName == userName).ToList();
        }

        public static byte[] GetUserNameInBytes(string? userName)
        {
            if (userName != null)
            {
                return Encoding.UTF8.GetBytes(userName);
            }

            throw new ArgumentNullException(nameof(userName));
        }
    }
}
