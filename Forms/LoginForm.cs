using JMoraesDesktopClient.Helpers;
using Newtonsoft.Json;
using System.Text;

namespace JMoraesDesktopClient.Forms
{
    public class LoginForm : Form
    {
        private Label lblTitle;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblError;

        public LoginForm()
        {
            this.Text = "Login";
            this.Size = new Size(350, 280);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            this.AcceptButton = btnLogin;
        }

        private void InitializeComponent()
        {
            lblTitle = new Label
            {
                Text = "Bem-vindo",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(110, 20)
            };

            txtUsername = new TextBox
            {
                PlaceholderText = "Usuário",
                Location = new Point(50, 70),
                Width = 230
            };

            txtPassword = new TextBox
            {
                PlaceholderText = "Senha",
                Location = new Point(50, 110),
                Width = 230,
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "Entrar",
                Location = new Point(50, 160),
                Width = 230,
                Height = 30
            };
            btnLogin.Click += btnLogin_Click;

            lblError = new Label
            {
                Text = "",
                ForeColor = Color.Red,
                Location = new Point(50, 200),
                AutoSize = true,
                Visible = false
            };

            Controls.Add(lblTitle);
            Controls.Add(txtUsername);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(lblError);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.FormClosed += (s, e) => Application.Exit();
        }

        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Visible = false;

            var username = txtUsername.Text;
            var password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblError.Text = "Preencha todos os campos.";
                lblError.Visible = true;
                return;
            }

            var loginRequest = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync("https://localhost:5001/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LoginResult>(responseJson);

                ApiSession.Token = result?.Token;
                new MainForm().Show();
                this.Hide();
            }
            else
            {
                lblError.Text = "Usuário ou senha inválidos.";
                lblError.Visible = true;
            }
        }
    }
}
