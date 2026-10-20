using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductCatalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImgUri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "ImgUri", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "6.1\" Super Retina XDR display, A18 chip, 48 MP Fusion camera and Camera Control button.", "https://example.com/img/1.jpg", "Apple iPhone 16 128GB Black", 22990.00m },
                    { 2, "6.9\" Dynamic AMOLED 2X, Snapdragon 8 Elite, 200 MP main camera and built-in S Pen.", "https://example.com/img/2.jpg", "Samsung Galaxy S25 Ultra 256GB Titanium Gray", 32990.00m },
                    { 3, "Tensor G4, 6.3\" LTPO OLED display, triple camera with 5x optical zoom.", "https://example.com/img/3.jpg", "Google Pixel 9 Pro 128GB Obsidian", 25990.00m },
                    { 4, null, "https://example.com/img/4.jpg", "Xiaomi Redmi Note 14 Pro 256GB Midnight Black", 8490.00m },
                    { 5, "Fanless design, 13.6\" Liquid Retina display and up to 18 hours of battery life.", "https://example.com/img/5.jpg", "Apple MacBook Air 13\" M3 16GB/512GB Midnight", 34990.00m },
                    { 6, "Business laptop with 14\" WUXGA IPS display, MIL-STD-810H tested chassis and Windows 11 Pro.", "https://example.com/img/6.jpg", "Lenovo ThinkPad T14 Gen 5 Ryzen 7 PRO 32GB/1TB", 38490.00m },
                    { 7, "16\" 240 Hz OLED gaming laptop with Intel Core Ultra 9 and NVIDIA GeForce RTX 4070.", "https://example.com/img/7.jpg", "ASUS ROG Zephyrus G16 RTX 4070 32GB/1TB", 59990.00m },
                    { 8, null, "https://example.com/img/8.jpg", "Dell XPS 13 9350 Core Ultra 7 16GB/512GB", 36990.00m },
                    { 9, "Wireless over-ear headphones with industry-leading noise cancellation and 30 hours of playback.", "https://example.com/img/9.jpg", "Sony WH-1000XM5 Black", 8990.00m },
                    { 10, "Active noise cancellation, adaptive audio and MagSafe charging case with USB-C.", "https://example.com/img/10.jpg", "Apple AirPods Pro 2 USB-C", 6490.00m },
                    { 11, null, "https://example.com/img/11.jpg", "JBL Tune 520BT Modrá", 1190.00m },
                    { 12, "True wireless earbuds with immersive audio and CustomTune noise cancellation.", "https://example.com/img/12.jpg", "Bose QuietComfort Ultra Earbuds Černá", 7490.00m },
                    { 13, "4K Neo QLED TV with Mini LED backlight, 144 Hz refresh rate and Tizen OS.", "https://example.com/img/13.jpg", "Samsung 55\" Neo QLED QE55QN90D", 29990.00m },
                    { 14, "65\" 4K OLED evo TV with α9 Gen7 AI processor, Dolby Vision and webOS.", "https://example.com/img/14.jpg", "LG OLED65C4 65\" Šedá", 44990.00m },
                    { 15, "Compact PS5 console without disc drive, 1 TB SSD and DualSense controller.", "https://example.com/img/15.jpg", "Sony PlayStation 5 Slim Digital Edition", 11990.00m },
                    { 16, null, "https://example.com/img/16.jpg", "Nintendo Switch OLED Bílá", 8490.00m },
                    { 17, "4K gaming at up to 120 fps, 1 TB custom NVMe SSD and Quick Resume.", "https://example.com/img/17.jpg", "Microsoft Xbox Series X 1TB", 13490.00m },
                    { 18, "Ergonomic wireless mouse with 8K DPI sensor, quiet clicks and MagSpeed scroll wheel.", "https://example.com/img/18.jpg", "Logitech MX Master 3S Grafitová", 2490.00m },
                    { 19, "Wireless illuminated keyboard with Czech and Slovak layout and Smart Actions.", "https://example.com/img/19.jpg", "Klávesnice Logitech MX Keys S CZ/SK", 2990.00m },
                    { 20, "27\" IPS Black monitor with 2560×1440 resolution, 120 Hz and USB-C hub.", "https://example.com/img/20.jpg", "Dell UltraSharp U2724D 27\" QHD", 9990.00m },
                    { 21, null, "https://example.com/img/21.jpg", "Kingston FURY Beast 32GB DDR5 6000MHz CL36 Kit", 2790.00m },
                    { 22, "PCIe 4.0 NVMe SSD with sequential reads up to 7450 MB/s.", "https://example.com/img/22.jpg", "Samsung 990 PRO 2TB NVMe M.2 SSD", 4290.00m },
                    { 23, "11\" Liquid Retina display, M2 chip and support for Apple Pencil Pro.", "https://example.com/img/23.jpg", "Apple iPad Air 11\" M2 Wi-Fi 128GB Hvězdně šedý", 16990.00m },
                    { 24, "Robotic vacuum with mopping, 6000 Pa suction and self-emptying station.", "https://example.com/img/24.jpg", "Xiaomi Robot Vacuum S20+ Bílý", 9990.00m },
                    { 25, "Free promotional voucher, included with selected orders.", "https://example.com/img/25.jpg", "Dárkový poukaz AlzaPlus+ zdarma", 0.00m },
                    { 26, null, "https://example.com/img/26.jpg", "Kabel USB-C na USB-C 1 m Černý", 149.00m },
                    { 27, "AI workstation with four NVIDIA A100 80GB GPUs for data science and deep learning workloads.", "https://example.com/img/27.jpg", "NVIDIA DGX Station A100 320GB Workstation", 4899990.00m },
                    { 28, "High-end gaming PC built and tested by Alza with three-year warranty.", "https://example.com/img/28.jpg", "Herní sestava Alza GameBox Ryzen 9 9950X3D, RTX 5090 32GB, 64GB DDR5 6000MHz, 4TB NVMe SSD, vodní chlazení 360 mm, zdroj 1200W 80+ Platinum, Wi-Fi 7, Bluetooth 5.4, Windows 11 Pro, černá skříň", 129990.00m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
