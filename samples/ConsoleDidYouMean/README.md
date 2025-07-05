# 🧪 Autypo Command-Line Demo

This sample demonstrates how to use the **[Autypo](https://github.com/andrewjsaid/autypo)** library for fuzzy autocomplete and string matching in a simple **interactive CLI** scenario.

---

## 🎯 What It Does

This console app simulates a basic command interface where a user is prompted to choose between:

```

create, read, update, delete

````

If the user mistypes any of these commands (e.g., `"updtae"`, `"cretae"`, `"red"`), **Autypo** kicks in and suggests the closest valid command based on fuzzy matching.

---

## 🧠 Key Features Showcased

✅ Fast in-memory autocomplete  
✅ Fuzzy string matching with adjustable tolerance (`fuzziness = 4`)  
✅ Interactive CLI prompt  
✅ Suggestions for unrecognized inputs  
✅ Easy-to-follow usage of `AutypoFactory` and `AutypoComplete`

---

## 🛠️ Code Highlights

```csharp
AutypoComplete autypoComplete = await AutypoFactory.CreateCompleteAsync(config => config
    .WithDataSource(["create", "read", "update", "delete"])
    .WithIndex(c => c, index => index.WithFuzziness(4)));
```

> The engine is initialized once with a fixed command list and fuzzy matching enabled. On unrecognized input, Autypo provides suggestions in real-time.

---

## 📂 File Structure

```
/ConsoleDidYouMean
  ├── ConsoleDidYouMean.csproj  # Project file
  ├── Program.cs                # Main app config
  └── README.md                 # (you are here)
```

---

## 🤔 Why This Matters

User-facing systems—especially CLI tools, admin panels, or command UIs—often suffer from strict input matching. With **Autypo**, you can offer forgiving, intelligent suggestions without the complexity of full NLP engines.

This demo gives you a minimal but powerful blueprint to integrate Autypo into your own projects.

---

## 🧪 Sample Session

```bash
Which command do you want to execute? Options are create, read, update or delete: cretae
Did you mean: create

Which command do you want to execute? Options are create, read, update or delete: delate
Did you mean: delete

Which command do you want to execute? Options are create, read, update or delete: exit
```
