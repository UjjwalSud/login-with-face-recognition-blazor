using Fido2NetLib;
using LoginWithFaceRecognitionBlazor.Code;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace LoginWithFaceRecognitionBlazor.Pages
{
    public partial class Register
    {
        private RegisterModel registerModel = new RegisterModel();

        [Inject]
        private Code.Fido2Service Fido2Service { get; set; }

        [Inject]
        private IJSRuntime JS { get; set; }

        private async Task HandleValidSubmit()
        {
            // generating a GUID and encoding it to Base64
            var userId = Guid.NewGuid().ToString();
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId);
            registerModel.UserId = Convert.ToBase64String(userIdBytes);

            var options = await Fido2Service.MakeCredentialAsync(registerModel.UserName, registerModel.UserId);

            // Serialize the options object to JSON
            var jsonOptions = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });

            // Print to console
            Console.WriteLine(jsonOptions);

            // Pass the options to JavaScript to initiate WebAuthn API
            await JS.InvokeVoidAsync("initiateWebAuthn", jsonOptions, DotNetObjectReference.Create(this));
        }

        [JSInvokableAttribute("ProcessRegistration")]
        public async Task ProcessRegistration(AuthenticatorAttestationRawResponse response)
        {           

            var result = await Fido2Service.VerifyCredentialAsync(response);

            if (result)
            {
                //return Ok(new { success = true });
            }
            //return BadRequest(new { success = false });
        }

        public class RegisterModel
        {
            public string UserName { get; set; }
            public string UserId { get; set; }
        }
        public class CredentialResponseModel
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string RawId { get; set; }
            public ResponseModel Response { get; set; }

            public class ResponseModel
            {
                public string AttestationObject { get; set; }
                public string ClientDataJSON { get; set; }
            }
        }
    }
}
