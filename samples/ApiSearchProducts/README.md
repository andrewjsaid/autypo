# 🧪 Autypo ASP.NET Core Product Search Demo

This sample demonstrates how to use the **[Autypo](https://github.com/andrewjsaid/autypo)** library to power **fast, intelligent, in-memory search** in a modern ASP.NET Core Web API.

---

## 🎯 What It Does

Simulates an e-commerce-style product search with:

- 🧠 **Fuzzy matching** on product names and descriptions
- 🏷️ **Exact, filtered matches** on product codes (e.g. "A1001")
- 🪄 **Unordered, partial term support** ("usb fingerprint recognition")
- 🚀 **In-memory speed** with background indexing

Try the live endpoints using the included `ApiSearchProducts.http` file.

---

## 📦 Sample API Endpoints

```http
GET /products/search?query=micro
GET /products/search?query=A1080
GET /products/search?query=usb fingerprint recognition
GET /products/search?query=1080
````

Returns matching products from a simulated 100-product catalog.

---

## 🧠 How It Works

Products are indexed with **two strategies**:

### 1. **By `Code`**

* High priority
* Exact match only
* Filtered to 5-character codes starting with `"A"`
* No fuzzy matching

### 2. **By `Name` + `Description`**

* Lower priority
* Fuzzy & partial matching
* Accepts queries like `"stream usb"` or `"fingerprint adapter"`

---

## 📂 File Structure

```
/ApiSearchProducts
├── ApiSearchProducts.csproj  # Project file
├── Program.cs                # Main app config + endpoint
├── Database.cs               # Simulated database source
├── ApiSearchProducts.http    # Sample REST client queries
└── README.md                 # (you are here)
```

---

## 🚀 Production-Like Features Demonstrated

✅ Background data loading with `ProductsLoader`
✅ DI-integrated database abstraction (`IDatabase`)
✅ Per-index priority and token strategies
✅ Custom match filters & fuzziness levels
✅ Ready for scalable deployment as a microservice

---

## 🧪 Sample Output

```
GET /products/search?query=fngerprint usb recogn

[
  {
    "code": "A1067",
    "name": "Fingerprint USB",
    "description": "Secure USB flash drive with fingerprint encryption"
  },
  ...
]
```
