# 🧠 Rio AI Workspace

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Ollama](https://img.shields.io/badge/AI-Ollama-black?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Active-success?style=for-the-badge)

**Rio AI Workspace** is a modern, privacy-focused AI Chat interface powered by **Local LLM (Ollama)** and **ASP.NET Core**. It provides a ChatGPT-like experience running entirely offline on your machine, ensuring 100% data privacy.

## ✨ Key Features

- 🤖 **Local Intelligence:** Powered by **Llama 3.2** (via Ollama), no internet required for inference.
- 🔒 **Privacy First:** Your data never leaves your device. No API keys sent to third-party servers.
- 💻 **Modern UI/UX:**
  - Full-screen responsive layout.
  - **Dark Mode / Light Mode** support (with auto-save).
  - **Markdown Rendering** (Bold, Lists, Tables).
  - **Syntax Highlighting** for code blocks (VS Code style).
- 🛠️ **Developer Friendly tools:**
  - One-click **Copy Code** button.
  - Copy entire message button.
- 🔐 **Secure Access:** Built-in Login system with **JWT Authentication**.
- 💾 **Smart Session:** Chat history is preserved during the active session.

## 🛠️ Tech Stack

- **Backend:** C# .NET 8 (ASP.NET Core Razor Pages)
- **AI Integration:** [OllamaSharp](https://github.com/awaescher/OllamaSharp)
- **Frontend:** HTML5, CSS3 (Inter Font), JavaScript
- **Libraries:**
  - `Marked.js` (Markdown Parser)
  - `Highlight.js` (Code Syntax Highlighter)

## 🚀 Getting Started

### Prerequisites
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download).
2. Install [Ollama](https://ollama.com/) and pull the model:
   ```bash
   ollama pull llama3.2
