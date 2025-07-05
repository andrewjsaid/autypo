// Just returns a hardcoded list of products.

public class Database : IDatabase
{
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(5)); // simulate slow database latency

        return GetHardcodedProducts();
    }

    private static IEnumerable<Product> GetHardcodedProducts()
    {
        // AI-generated list of products

        yield return new Product("A1001", "Wireless Mouse", "Ergonomic 2.4GHz wireless optical mouse");
        yield return new Product("A1002", "Mechanical Keyboard", "RGB backlit mechanical keyboard with blue switches");
        yield return new Product("A1003", "USB-C Charger", "65W USB-C fast charger for laptops and phones");
        yield return new Product("A1004", "HD Webcam", "1080p HD webcam with built-in microphone");
        yield return new Product("A1005", "Noise Cancelling Headphones", "Bluetooth over-ear noise cancelling headphones");
        yield return new Product("A1006", "4K Monitor", "27-inch 4K UHD IPS display with HDR support");
        yield return new Product("A1007", "Portable SSD", "1TB USB 3.1 portable solid-state drive");
        yield return new Product("A1008", "Smart LED Bulb", "Color-changing Wi-Fi smart LED light bulb");
        yield return new Product("A1009", "Fitness Tracker", "Water-resistant fitness tracker with heart rate monitor");
        yield return new Product("A1010", "Smartphone Stand", "Adjustable aluminum desk stand for smartphones");

        yield return new Product("A1011", "Laptop Stand", "Ergonomic laptop stand with ventilation");
        yield return new Product("A1012", "Bluetooth Speaker", "Portable waterproof Bluetooth speaker");
        yield return new Product("A1013", "Wireless Earbuds", "True wireless earbuds with charging case");
        yield return new Product("A1014", "Gaming Mouse Pad", "Large extended gaming mouse pad with stitched edges");
        yield return new Product("A1015", "Laptop Backpack", "Anti-theft backpack with USB charging port");
        yield return new Product("A1016", "External Hard Drive", "2TB external USB 3.0 hard drive");
        yield return new Product("A1017", "Graphics Tablet", "Digital drawing tablet with pressure-sensitive pen");
        yield return new Product("A1018", "Webcam Cover", "Privacy cover for laptop and desktop webcams");
        yield return new Product("A1019", "USB Hub", "4-port USB 3.0 hub with individual switches");
        yield return new Product("A1020", "Phone Tripod", "Flexible tripod with phone mount and remote");

        yield return new Product("A1021", "Wireless Charger", "15W fast wireless charging pad");
        yield return new Product("A1022", "Mechanical Pencil", "0.5mm mechanical pencil with metal grip");
        yield return new Product("A1023", "Desk Lamp", "LED desk lamp with adjustable brightness and color");
        yield return new Product("A1024", "Office Chair", "Ergonomic office chair with lumbar support");
        yield return new Product("A1025", "Coffee Mug Warmer", "USB-powered mug warmer for desk use");
        yield return new Product("A1026", "Monitor Arm", "Adjustable single monitor desk mount");
        yield return new Product("A1027", "Digital Voice Recorder", "Portable voice recorder with noise reduction");
        yield return new Product("A1028", "Bluetooth Adapter", "USB Bluetooth 5.0 adapter for PC");
        yield return new Product("A1029", "Smart Plug", "Wi-Fi smart plug compatible with Alexa and Google Home");
        yield return new Product("A1030", "Surge Protector", "6-outlet surge protector with USB ports");

        yield return new Product("A1031", "Smart Thermostat", "Wi-Fi smart thermostat with touchscreen");
        yield return new Product("A1032", "Action Camera", "4K waterproof action camera with accessories");
        yield return new Product("A1033", "Drawing Stylus", "Universal stylus for tablets and smartphones");
        yield return new Product("A1034", "Photo Printer", "Portable wireless photo printer for smartphones");
        yield return new Product("A1035", "Wi-Fi Extender", "Dual band Wi-Fi range extender with Ethernet port");
        yield return new Product("A1036", "Cable Organizer", "Desk cable organizer with clips and channels");
        yield return new Product("A1037", "Smartwatch", "Fitness smartwatch with GPS and sleep tracking");
        yield return new Product("A1038", "Digital Alarm Clock", "LED alarm clock with USB charging and dimmer");
        yield return new Product("A1039", "Laptop Cooling Pad", "5-fan cooling pad for laptops up to 17 inches");
        yield return new Product("A1040", "Streaming Microphone", "USB condenser microphone with stand and pop filter");

        yield return new Product("A1041", "Ring Light", "10-inch ring light with tripod for video and streaming");
        yield return new Product("A1042", "Power Bank", "10000mAh slim portable power bank");
        yield return new Product("A1043", "VR Headset", "Virtual reality headset compatible with smartphones");
        yield return new Product("A1044", "Smart Scale", "Bluetooth smart scale with body composition tracking");
        yield return new Product("A1045", "Keyboard Cleaner", "Mini vacuum cleaner for keyboard and electronics");
        yield return new Product("A1046", "Wireless Presenter", "Presentation clicker with laser pointer");
        yield return new Product("A1047", "Ethernet Cable", "Cat 6 flat Ethernet cable - 10ft");
        yield return new Product("A1048", "Smart Lock", "Keyless smart lock with app and fingerprint access");
        yield return new Product("A1049", "Laptop Docking Station", "USB-C docking station with HDMI and Ethernet");
        yield return new Product("A1050", "3D Pen", "3D printing pen for kids and adults");

        yield return new Product("A1051", "Phone Sanitizer", "UV-C light phone sanitizer box with wireless charging");
        yield return new Product("A1052", "Compact Projector", "Mini HD projector for home entertainment");
        yield return new Product("A1053", "Ergonomic Mouse", "Vertical ergonomic mouse with adjustable DPI");
        yield return new Product("A1054", "Laser Printer", "Black and white wireless laser printer");
        yield return new Product("A1055", "Tablet Stand", "Adjustable stand for tablets and e-readers");
        yield return new Product("A1056", "Smart Doorbell", "Video doorbell with motion detection and app alerts");
        yield return new Product("A1057", "Label Maker", "Bluetooth label printer with mobile app");
        yield return new Product("A1058", "USB Fan", "Mini USB desk fan with 3 speeds");
        yield return new Product("A1059", "Paper Shredder", "Cross-cut shredder for home office use");
        yield return new Product("A1060", "Wireless HDMI Transmitter", "Wireless HDMI kit for TV and projector");

        yield return new Product("A1061", "Laptop Sleeve", "Water-resistant laptop sleeve with padding");
        yield return new Product("A1062", "Portable Monitor", "15.6-inch Full HD USB-C portable monitor");
        yield return new Product("A1063", "Cordless Screwdriver", "Rechargeable electric screwdriver set");
        yield return new Product("A1064", "Whiteboard", "Magnetic dry erase board with markers");
        yield return new Product("A1065", "Smart Glasses", "Bluetooth smart audio glasses with polarized lenses");
        yield return new Product("A1066", "Photo Frame", "Digital photo frame with Wi-Fi and cloud storage");
        yield return new Product("A1067", "Fingerprint USB", "Secure USB flash drive with fingerprint encryption");
        yield return new Product("A1068", "Air Purifier", "HEPA air purifier for home office");
        yield return new Product("A1069", "Wi-Fi Camera", "Indoor smart security camera with 2-way audio");
        yield return new Product("A1070", "Cable Tidy Box", "Cable management box with lid");

        yield return new Product("A1071", "LED Strip Lights", "Color LED strip lights with remote control");
        yield return new Product("A1072", "Gaming Controller", "Wireless game controller for PC and Android");
        yield return new Product("A1073", "Digital Thermometer", "Infrared forehead thermometer for contactless use");
        yield return new Product("A1074", "Notebook Cooler", "Cooling stand with adjustable height and fans");
        yield return new Product("A1075", "Screen Cleaner Kit", "Complete kit for cleaning screens and keyboards");
        yield return new Product("A1076", "Desk Organizer", "Multi-compartment wood desk organizer");
        yield return new Product("A1077", "Backup Camera", "Wireless backup camera for car with monitor");
        yield return new Product("A1078", "Laptop Privacy Screen", "Anti-glare privacy screen for 15.6\" laptop");
        yield return new Product("A1079", "Battery Tester", "Universal battery tester for AA/AAA/C/D/9V");
        yield return new Product("A1080", "USB Microscope", "Digital microscope with 1000x magnification");

        yield return new Product("A1081", "Smart Pen", "Bluetooth smart pen with handwriting recognition");
        yield return new Product("A1082", "PC Toolkit", "Precision screwdriver kit for electronics");
        yield return new Product("A1083", "Mini Fridge", "Compact personal fridge for office or bedroom");
        yield return new Product("A1084", "Standing Desk Converter", "Height adjustable desk riser");
        yield return new Product("A1085", "Smart IR Remote", "Universal smart remote controller with app");
        yield return new Product("A1086", "Webcam Light", "Clip-on ring light for webcams and laptops");
        yield return new Product("A1087", "HDMI Switch", "3-in-1 HDMI switch with remote control");
        yield return new Product("A1088", "USB Clock Fan", "LED fan with programmable time display");
        yield return new Product("A1089", "Portable Scanner", "Compact handheld document scanner");
        yield return new Product("A1090", "Streaming Capture Card", "USB capture card for game streaming");

        yield return new Product("A1091", "Gaming Headset", "Surround sound headset with mic for PC/PS5/Xbox");
        yield return new Product("A1092", "Laptop Power Bank", "20000mAh power bank with AC output for laptops");
        yield return new Product("A1093", "Smart Diffuser", "Wi-Fi controlled essential oil diffuser");
        yield return new Product("A1094", "Multi-Charging Cable", "3-in-1 USB charging cable (USB-C, Lightning, Micro)");
        yield return new Product("A1095", "Wireless Light Switch", "Battery-powered smart light switch");
        yield return new Product("A1096", "Magnetic Cable", "Magnetic USB charging cable set");
        yield return new Product("A1097", "Laptop Cleaner", "Screen and keyboard cleaning kit with spray and brush");
        yield return new Product("A1098", "Mini Projector Screen", "Portable tripod projector screen (60-inch)");
        yield return new Product("A1099", "LED Matrix Display", "Programmable LED display for messages and signs");
        yield return new Product("A1100", "Smart IR Blaster", "Wi-Fi universal remote for AC, TV, and more");
    }
}
