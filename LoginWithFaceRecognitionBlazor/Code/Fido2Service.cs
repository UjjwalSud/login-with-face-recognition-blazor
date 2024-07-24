using Blazored.LocalStorage;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace LoginWithFaceRecognitionBlazor.Code
{
    public class Fido2Service
    {
        private readonly Fido2 _fido2;
        ILocalStorageService _localStorageService;
        public Fido2Service(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
            _fido2 = new Fido2(new Fido2Configuration
            {
                ServerDomain = "localhost",
                ServerName = "Your Server Name",
                Origins = ["https://localhost:7086"],
            });
        }

        public async Task<CredentialCreateOptions> MakeCredentialAsync(string userName, string userId)
        {
            var items = Db.RegisterUserRepository.GetUserByUserName(userName);
            var existingKeys = new List<PublicKeyCredentialDescriptor>();
            foreach (var publicKeyCredentialDescriptor in items)
            {
                if (publicKeyCredentialDescriptor.Descriptor != null)
                    existingKeys.Add(publicKeyCredentialDescriptor.Descriptor);
            }
            var user = new Fido2User
            {
                Name = userName,
                Id = Convert.FromBase64String(userId),
                DisplayName = userName
            };

            var response = _fido2.RequestNewCredential(user, existingKeys, new AuthenticatorSelection
            {
                UserVerification = UserVerificationRequirement.Preferred
            }, AttestationConveyancePreference.None);

            await _localStorageService.SetItemAsync<CredentialCreateOptions>("CredentialCreateOptions", response);

            return response;

        }

        private static async Task<bool> CredentialIdUniqueToUserAsync(IsCredentialIdUniqueToUserParams args, CancellationToken cancellationToken)
        {
            var users = Db.RegisterUserRepository.GetUsersByCredentialIdAsync(args.CredentialId);
             return users.Count <= 0;            
        }

        public async Task<bool> VerifyCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse)
        {
            try
            {
                var options = await _localStorageService.GetItemAsync<CredentialCreateOptions>("CredentialCreateOptions");            



                // Verify the attestation response
                var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, CredentialIdUniqueToUserAsync);

                if (success.Result != null)
                {
                    //// Store the credential               
                    Db.RegisterUserRepository.AddNewUser(new Db.RegisterUsersDb
                    {
                        UserId = options.User.Id,
                        UserName = options.User.Name,
                        Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                        PublicKey = success.Result.PublicKey,
                        UserHandle = success.Result.User.Id,
                        SignatureCounter = success.Result.Counter,
                        CredType = success.Result.CredType,
                        RegDate = DateTimeOffset.UtcNow,
                        //AaGuid = success.Result.AaGuid // version 4
                        AaGuid = success.Result.Aaguid

                    });
                }


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying credential: {ex.Message}");
                return false;
            }
        }


    }
}
