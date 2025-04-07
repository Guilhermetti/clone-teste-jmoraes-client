namespace JMoraesDesktopClient.Forms
{
    public class MainForm : Form
    {
        private Button btnCategorias;
        private Button btnProdutos;

        public MainForm()
        {
            Text = "Tela Principal";
            Size = new Size(275, 175);
            StartPosition = FormStartPosition.CenterScreen;

            btnCategorias = new Button { Text = "Categorias", Location = new Point(40, 30), Width = 180 };
            btnProdutos = new Button { Text = "Produtos", Location = new Point(40, 80), Width = 180 };

            btnCategorias.Click += (s, e) =>
            {
                this.Hide();
                var categoryForm = new CategoryForm();
                categoryForm.FormClosed += (sender, args) => this.Show();
                categoryForm.Show();
            };

            btnProdutos.Click += (s, e) =>
            {
                this.Hide();
                var productForm = new ProductForm();
                productForm.FormClosed += (sender, args) => this.Show();
                productForm.Show();
            };

            Controls.Add(btnCategorias);
            Controls.Add(btnProdutos);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.FormClosed += (s, e) => Application.Exit();
        }
    }
}
