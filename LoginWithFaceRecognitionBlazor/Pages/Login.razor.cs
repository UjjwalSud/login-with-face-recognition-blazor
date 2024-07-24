namespace LoginWithFaceRecognitionBlazor.Pages
{
    public partial class Login
    {
        private User user = new User();

        private void HandleValidSubmit()
        {
            // Here, you would normally send the data to your API or service
            Console.WriteLine($"User Logged In: {user.Username}");
        }

        public class User
        {
            //[Required]
            public string Username { get; set; }
        }
    }
}
