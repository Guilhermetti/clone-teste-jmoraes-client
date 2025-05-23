﻿namespace JMoraesDesktopClient.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Product>? Products { get; set; }

        public override string ToString() => Name;
    }
}
