using JMoraesDesktopClient.Forms;

namespace JMoraesDesktopClient.Helpers
{
    public static class ApiSession
    {
        public static string Token { get; set; } = string.Empty;
        public static string Username { get; set; } = string.Empty;

        public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public static void Clear()
        {
            Token = string.Empty;
            Username = string.Empty;
        }

        public static HttpClient CreateAuthorizedClient(Form currentForm)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            client.DefaultRequestHeaders.ExpectContinue = false;
            return client;
        }

        public static async Task<bool> CheckUnauthorized(HttpResponseMessage response, Form currentForm)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                MessageBox.Show("Sessão expirada. Faça login novamente.");
                Clear();
                currentForm.Invoke(() =>
                {
                    new LoginForm().Show();
                    currentForm.Close();
                });
                return true;
            }
            return false;
        }
    }
}
