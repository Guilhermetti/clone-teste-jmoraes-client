using JMoraesDesktopClient.Helpers;
using JMoraesDesktopClient.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace JMoraesDesktopClient.Forms
{
    public class ProductForm : Form
    {
        private ComboBox cmbCategories;
        private DataGridView dgvProducts;
        private TextBox txtDetails;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnPrevPage;
        private Button btnNextPage;
        private Button btnBack;
        private Label lblPageNumber;

        private List<Category> categories = new();
        private List<Product> products = new();

        private int currentPage = 1;
        private const int pageSize = 10;

        private string? currentSortColumn = null;
        private bool sortAscending = true;

        public ProductForm()
        {
            Text = "Produtos";
            Size = new Size(700, 480);
            StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            LoadCategories();
        }

        private void InitializeComponent()
        {
            cmbCategories = new ComboBox
            {
                Location = new Point(20, 20),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbCategories.SelectedIndexChanged += CmbCategories_SelectedIndexChanged;

            dgvProducts = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(300, 300),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                Font = new Font("Segoe UI", 8),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 8, FontStyle.Bold) },
                EnableHeadersVisualStyles = false
            };
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;
            dgvProducts.ColumnHeaderMouseClick += DgvProducts_ColumnHeaderMouseClick;

            txtDetails = new TextBox
            {
                Location = new Point(340, 60),
                Size = new Size(320, 290),
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Segoe UI", 8)
            };

            btnAdd = new Button
            {
                Text = "Adicionar",
                Location = new Point(340, 390)
            };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button
            {
                Text = "Editar",
                Location = new Point(420, 390)
            };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button
            {
                Text = "Excluir",
                Location = new Point(500, 390)
            };
            btnDelete.Click += BtnDelete_Click;

            btnBack = new Button
            {
                Text = "Voltar",
                Location = new Point(580, 390)
            };
            btnBack.Click += (s, e) => this.Close();

            btnPrevPage = new Button
            {
                Text = "< Anterior",
                Location = new Point(20, 390)
            };
            btnPrevPage.Click += (s, e) => { if (currentPage > 1) { currentPage--; LoadProducts(); } };

            btnNextPage = new Button
            {
                Text = "Próxima >",
                Location = new Point(120, 390)
            };
            btnNextPage.Click += (s, e) => { currentPage++; LoadProducts(); };

            lblPageNumber = new Label
            {
                Location = new Point(20, 365),
                Size = new Size(100, 23),
                Text = $"Página {currentPage}"
            };

            Controls.AddRange(new Control[] {
                cmbCategories, dgvProducts, txtDetails,
                btnAdd, btnEdit, btnDelete, btnPrevPage, btnNextPage,
                btnBack, lblPageNumber
            });

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
        }

        private async void LoadCategories()
        {
            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.GetAsync("https://localhost:5001/api/category");
            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                cmbCategories.Items.Clear();
                cmbCategories.Text = "Nenhuma categoria";
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<Category>>(json) ?? new();

                cmbCategories.SelectedIndexChanged -= CmbCategories_SelectedIndexChanged;
                cmbCategories.Items.Clear();
                foreach (var cat in categories.OrderBy(c => c.Name))
                    cmbCategories.Items.Add(cat);

                if (cmbCategories.Items.Count > 0)
                {
                    cmbCategories.SelectedIndex = 0;
                    LoadProducts();
                }

                cmbCategories.SelectedIndexChanged += CmbCategories_SelectedIndexChanged;
            }
        }

        private async void LoadProducts()
        {
            if (cmbCategories.SelectedItem is not Category selectedCategory) return;

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.GetAsync($"https://localhost:5001/api/product/paged?pageNumber={currentPage}&pageSize={pageSize}&categoryId={selectedCategory.Id}");
            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var paginated = JsonConvert.DeserializeObject<PaginatedResponse<Product>>(json);
                products = paginated?.Items ?? new();

                IEnumerable<Product> sorted = products;
                if (!string.IsNullOrEmpty(currentSortColumn))
                {
                    var prop = typeof(Product).GetProperty(currentSortColumn);
                    if (prop != null)
                    {
                        sorted = sortAscending
                            ? products.OrderBy(p => prop.GetValue(p, null))
                            : products.OrderByDescending(p => prop.GetValue(p, null));
                    }
                }

                var productList = sorted.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    Price = p.Price
                }).ToList();

                dgvProducts.DataSource = productList;
                dgvProducts.Columns["Price"].HeaderText = "Preço";
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
                lblPageNumber.Text = $"Página {currentPage}";
                txtDetails.Clear();
            }
        }

        private void DgvProducts_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is not null)
            {
                var row = dgvProducts.CurrentRow;
                var id = (int)row.Cells["Id"].Value;
                var product = products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    txtDetails.Text = $"Nome: {product.Name}{Environment.NewLine}Descrição: {product.Description}{Environment.NewLine}Preço: R${product.Price:N2}";
                }
            }
        }

        private void DgvProducts_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            var columnName = dgvProducts.Columns[e.ColumnIndex].DataPropertyName;
            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }
            LoadProducts();
        }

        private void CmbCategories_SelectedIndexChanged(object? sender, EventArgs e)
        {
            currentPage = 1;
            LoadProducts();
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (cmbCategories.SelectedItem is not Category selectedCategory) return;

            string? name;
            do
            {
                name = Prompt.ShowDialog("Nome do produto:", "Adicionar Produto");
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name))
                    MessageBox.Show("O nome é obrigatório.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (string.IsNullOrWhiteSpace(name));

            string? priceStr;
            decimal price;
            do
            {
                priceStr = Prompt.ShowDialog("Preço:", "Adicionar Produto");
                if (priceStr == null) return;
                if (!decimal.TryParse(priceStr, out price))
                    MessageBox.Show("Preço inválido. Digite um número válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (!decimal.TryParse(priceStr, out price));

            var description = Prompt.ShowDialog("Descrição (opcional):", "Adicionar Produto") ?? "";

            var newProduct = new { name, price, description, categoryId = selectedCategory.Id };
            var content = new StringContent(JsonConvert.SerializeObject(newProduct), Encoding.UTF8, "application/json");

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.PostAsync("https://localhost:5001/api/product", content);
            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                LoadProducts();
            }
            else
            {
                try
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var errors = JsonConvert.DeserializeObject<List<ApiError>>(errorJson);
                    if (errors != null && errors.Any())
                    {
                        var errorMessage = string.Join(Environment.NewLine, errors.Select(e => e.Message));
                        MessageBox.Show(errorMessage, "Erro de Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Erro ao adicionar produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao adicionar produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem == null) return;
            var id = (int)dgvProducts.CurrentRow.Cells["Id"].Value;
            var selectedProduct = products.FirstOrDefault(p => p.Id == id);
            if (selectedProduct == null) return;

            string? name;
            do
            {
                name = Prompt.ShowDialog("Editar nome:", "Editar Produto", selectedProduct.Name);
                if (name == null) return;
                if (string.IsNullOrWhiteSpace(name))
                    MessageBox.Show("O nome é obrigatório.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (string.IsNullOrWhiteSpace(name));

            string? priceStr;
            decimal price;
            do
            {
                priceStr = Prompt.ShowDialog("Editar preço:", "Editar Produto", selectedProduct.Price.ToString());
                if (priceStr == null) return;
                if (!decimal.TryParse(priceStr, out price))
                    MessageBox.Show("Preço inválido. Digite um número válido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } while (!decimal.TryParse(priceStr, out price));

            var updatedProduct = new
            {
                id = selectedProduct.Id,
                name,
                price,
                description = selectedProduct.Description,
                categoryId = selectedProduct.CategoryId
            };

            var content = new StringContent(JsonConvert.SerializeObject(updatedProduct), Encoding.UTF8, "application/json");

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.PutAsync("https://localhost:5001/api/product", content);

            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                LoadProducts();
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
                        MessageBox.Show("Erro ao editar produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao editar produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem == null) return;
            var id = (int)dgvProducts.CurrentRow.Cells["Id"].Value;
            var selectedProduct = products.FirstOrDefault(p => p.Id == id);
            if (selectedProduct == null) return;

            var confirm = MessageBox.Show(
                $"Deseja excluir o produto '{selectedProduct.Name}'?",
                "Confirmação de Exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (confirm != DialogResult.Yes) return;

            using var client = ApiSession.CreateAuthorizedClient(this);
            var response = await client.DeleteAsync($"https://localhost:5001/api/product/{selectedProduct.Id}");
            if (await ApiSession.CheckUnauthorized(response, this)) return;

            if (response.IsSuccessStatusCode)
            {
                if (products.Count == 1 && currentPage > 1)
                    currentPage--;

                LoadProducts();
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
                        MessageBox.Show("Erro ao excluir produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Erro ao excluir produto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
