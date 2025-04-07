using JMoraesDesktopClient.Helpers;
using JMoraesDesktopClient.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace JMoraesDesktopClient.Forms
{
    public class CategoryForm : Form
    {
        private ListBox lstCategories;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnBack;
        private TextBox txtProducts;
        private List<Category> categories;
        private TextBox txtSearch;
        private List<Category> allCategories = new();

        public CategoryForm()
        {
            Text = "Categorias";
            Size = new Size(350, 460);
            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            lstCategories = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(200, 280)
            };
            lstCategories.SelectedIndexChanged += LstCategories_SelectedIndexChanged;

            btnAdd = new Button
            {
                Text = "Adicionar",
                Location = new Point(240, 20)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Editar",
                Location = new Point(240, 60)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Excluir",
                Location = new Point(240, 100)
            };
            btnDelete.Click += BtnDelete_Click;

            btnBack = new Button
            {
                Text = "Voltar",
                Location = new Point(240, 140)
            };
            btnBack.Click += (s, e) => this.Close();

            txtProducts = new TextBox
            {
                Location = new Point(20, 310),
                Size = new Size(295, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Buscar categoria...",
                Location = new Point(20, 0),
                Size = new Size(200, 20)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            Controls.Add(lstCategories);
            Controls.Add(btnAdd);
            Controls.Add(btnEdit);
            Controls.Add(btnDelete);
            Controls.Add(txtProducts);
            Controls.Add(txtSearch);
            Controls.Add(btnBack);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
        }

        private async void LoadCategories()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiSession.Token);

            try
            {
                var response = await client.GetAsync("https://localhost:5001/api/category");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    lstCategories.Items.Clear();
                    txtProducts.Text = "Nenhuma categoria cadastrada.";
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    allCategories = (JsonConvert.DeserializeObject<List<Category>>(json) ?? new())
                        .OrderBy(c => c.Name)
                        .ToList();

                    UpdateCategoryList();
                }
                else if (await ApiSession.CheckUnauthorized(response, this)) return;
                else
                {
                    MessageBox.Show("Erro inesperado ao carregar categorias.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar com a API: {ex.Message}");
            }
        }

        private async void LstCategories_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstCategories.SelectedIndex < 0) return;

            var category = categories[lstCategories.SelectedIndex];
            if (category?.Products != null && category.Products.Any())
            {
                txtProducts.Text = string.Join(Environment.NewLine,
                    category.Products.Select(p => $"{p.Name}"));
            }
            else
            {
                txtProducts.Text = "Sem produtos cadastrados.";
            }
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            string? name;
            do
            {
                name = Prompt.ShowDialog("Nome da categoria:", "Adicionar Categoria");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name))
                    MessageBox.Show("O nome é obrigatório.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (string.IsNullOrWhiteSpace(name));

            var category = new { name };
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.PostAsync("https://localhost:5001/api/category", content);

            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                LoadCategories();
            }
            else
            {
                try
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var errors = JsonConvert.DeserializeObject<List<ApiError>>(json);
                    if (errors != null && errors.Any())
                    {
                        var errorMessage = string.Join(Environment.NewLine, errors.Select(e => e.Message));
                        MessageBox.Show(errorMessage, "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Erro ao adicionar categoria.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao adicionar categoria.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (lstCategories.SelectedIndex < 0) return;
            var category = categories[lstCategories.SelectedIndex];

            string? newName;
            do
            {
                newName = Prompt.ShowDialog("Editar nome da categoria:", "Editar", category.Name);
                if (newName == null) return;
                if (string.IsNullOrWhiteSpace(newName))
                    MessageBox.Show("O nome é obrigatório.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (string.IsNullOrWhiteSpace(newName));

            var updatedCategory = new { id = category.Id, name = newName };
            var content = new StringContent(JsonConvert.SerializeObject(updatedCategory), Encoding.UTF8, "application/json");

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.PutAsync("https://localhost:5001/api/category", content);

            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                LoadCategories();
            }
            else
            {
                try
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var errors = JsonConvert.DeserializeObject<List<ApiError>>(json);
                    if (errors != null && errors.Any())
                    {
                        var errorMessage = string.Join(Environment.NewLine, errors.Select(e => e.Message));
                        MessageBox.Show(errorMessage, "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Erro ao editar categoria.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao editar categoria.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstCategories.SelectedIndex < 0) return;
            var category = categories[lstCategories.SelectedIndex];

            var confirm = MessageBox.Show(
                @$"Tem certeza que deseja excluir a categoria '{category.Name}'? Todos os produtos vinculados a essa categoria também serão permanentemente excluídos.",
                "Confirmação de Exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (confirm != DialogResult.Yes) return;

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.DeleteAsync($"https://localhost:5001/api/category/{category.Id}");

            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                LoadCategories();
            }
            else
            {
                try
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var errors = JsonConvert.DeserializeObject<List<ApiError>>(json);
                    if (errors != null && errors.Any())
                    {
                        MessageBox.Show(errors.First().Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Erro ao excluir categoria.");
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao excluir categoria.");
                }
            }
        }

        private void UpdateCategoryList(string filter = "")
        {
            lstCategories.Items.Clear();

            var filtered = string.IsNullOrWhiteSpace(filter)
                ? allCategories
                : allCategories.Where(c => c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var cat in filtered)
            {
                var productCount = cat.Products?.Count ?? 0;
                lstCategories.Items.Add($"{cat.Name} ({productCount})");
            }

            categories = filtered;
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            UpdateCategoryList(txtSearch.Text);
        }
    }
}
