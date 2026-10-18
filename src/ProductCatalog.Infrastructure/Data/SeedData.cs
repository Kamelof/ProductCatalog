using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Data;

public static class SeedData
{
    // Returns new instances on every call so that callers can't mutate shared state.
    public static IReadOnlyList<Product> Products =>
    [
        new(1, "Apple iPhone 16 128GB Black", Img(1), 22990.00m,
            "6.1\" Super Retina XDR display, A18 chip, 48 MP Fusion camera and Camera Control button."),
        new(2, "Samsung Galaxy S25 Ultra 256GB Titanium Gray", Img(2), 32990.00m,
            "6.9\" Dynamic AMOLED 2X, Snapdragon 8 Elite, 200 MP main camera and built-in S Pen."),
        new(3, "Google Pixel 9 Pro 128GB Obsidian", Img(3), 25990.00m,
            "Tensor G4, 6.3\" LTPO OLED display, triple camera with 5x optical zoom."),
        new(4, "Xiaomi Redmi Note 14 Pro 256GB Midnight Black", Img(4), 8490.00m),
        new(5, "Apple MacBook Air 13\" M3 16GB/512GB Midnight", Img(5), 34990.00m,
            "Fanless design, 13.6\" Liquid Retina display and up to 18 hours of battery life."),
        new(6, "Lenovo ThinkPad T14 Gen 5 Ryzen 7 PRO 32GB/1TB", Img(6), 38490.00m,
            "Business laptop with 14\" WUXGA IPS display, MIL-STD-810H tested chassis and Windows 11 Pro."),
        new(7, "ASUS ROG Zephyrus G16 RTX 4070 32GB/1TB", Img(7), 59990.00m,
            "16\" 240 Hz OLED gaming laptop with Intel Core Ultra 9 and NVIDIA GeForce RTX 4070."),
        new(8, "Dell XPS 13 9350 Core Ultra 7 16GB/512GB", Img(8), 36990.00m),
        new(9, "Sony WH-1000XM5 Black", Img(9), 8990.00m,
            "Wireless over-ear headphones with industry-leading noise cancellation and 30 hours of playback."),
        new(10, "Apple AirPods Pro 2 USB-C", Img(10), 6490.00m,
            "Active noise cancellation, adaptive audio and MagSafe charging case with USB-C."),
        new(11, "JBL Tune 520BT Modrá", Img(11), 1190.00m),
        new(12, "Bose QuietComfort Ultra Earbuds Černá", Img(12), 7490.00m,
            "True wireless earbuds with immersive audio and CustomTune noise cancellation."),
        new(13, "Samsung 55\" Neo QLED QE55QN90D", Img(13), 29990.00m,
            "4K Neo QLED TV with Mini LED backlight, 144 Hz refresh rate and Tizen OS."),
        new(14, "LG OLED65C4 65\" Šedá", Img(14), 44990.00m,
            "65\" 4K OLED evo TV with α9 Gen7 AI processor, Dolby Vision and webOS."),
        new(15, "Sony PlayStation 5 Slim Digital Edition", Img(15), 11990.00m,
            "Compact PS5 console without disc drive, 1 TB SSD and DualSense controller."),
        new(16, "Nintendo Switch OLED Bílá", Img(16), 8490.00m),
        new(17, "Microsoft Xbox Series X 1TB", Img(17), 13490.00m,
            "4K gaming at up to 120 fps, 1 TB custom NVMe SSD and Quick Resume."),
        new(18, "Logitech MX Master 3S Grafitová", Img(18), 2490.00m,
            "Ergonomic wireless mouse with 8K DPI sensor, quiet clicks and MagSpeed scroll wheel."),
        new(19, "Klávesnice Logitech MX Keys S CZ/SK", Img(19), 2990.00m,
            "Wireless illuminated keyboard with Czech and Slovak layout and Smart Actions."),
        new(20, "Dell UltraSharp U2724D 27\" QHD", Img(20), 9990.00m,
            "27\" IPS Black monitor with 2560×1440 resolution, 120 Hz and USB-C hub."),
        new(21, "Kingston FURY Beast 32GB DDR5 6000MHz CL36 Kit", Img(21), 2790.00m),
        new(22, "Samsung 990 PRO 2TB NVMe M.2 SSD", Img(22), 4290.00m,
            "PCIe 4.0 NVMe SSD with sequential reads up to 7450 MB/s."),
        new(23, "Apple iPad Air 11\" M2 Wi-Fi 128GB Hvězdně šedý", Img(23), 16990.00m,
            "11\" Liquid Retina display, M2 chip and support for Apple Pencil Pro."),
        new(24, "Xiaomi Robot Vacuum S20+ Bílý", Img(24), 9990.00m,
            "Robotic vacuum with mopping, 6000 Pa suction and self-emptying station."),
        new(25, "Dárkový poukaz AlzaPlus+ zdarma", Img(25), 0.00m,
            "Free promotional voucher, included with selected orders."),
        new(26, "Kabel USB-C na USB-C 1 m Černý", Img(26), 149.00m),
        new(27, "NVIDIA DGX Station A100 320GB Workstation", Img(27), 4899990.00m,
            "AI workstation with four NVIDIA A100 80GB GPUs for data science and deep learning workloads."),
        new(28,
            "Herní sestava Alza GameBox Ryzen 9 9950X3D, RTX 5090 32GB, 64GB DDR5 6000MHz, 4TB NVMe SSD, " +
            "vodní chlazení 360 mm, zdroj 1200W 80+ Platinum, Wi-Fi 7, Bluetooth 5.4, Windows 11 Pro, černá skříň",
            Img(28), 129990.00m,
            "High-end gaming PC built and tested by Alza with three-year warranty."),
    ];

    private static string Img(int id) => $"https://example.com/img/{id}.jpg";
}
