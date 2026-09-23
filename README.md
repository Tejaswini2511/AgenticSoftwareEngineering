# Agentic Software Engineering & URL Shortener System

A consolidated, high-performance .NET 8.0 solution combining a secure URL Shortener service with an Agentic Workflow Orchestrator. Built with robust input sanitization, automated unit tests, and seamless OpenAPI/Swagger UI compatibility.

---

## Architecture Overview

The solution uses a simplified **2-project architecture** to optimize build performance, eliminate reference conflicts, and isolate testing logic:

* **`AgenticSoftwareEngineering`**: Main Web API housing models, domain services, orchestration logic, and security middleware.
* **`AgenticSoftwareEngineering.Tests`**: Automated unit test suite utilizing xUnit.

---

## Security Implementation

* **Input Sanitization**: Regular expression pattern matching (`^[a-zA-Z0-9_-]+$`) validates all short codes and custom aliases to block XSS and path traversal attacks.
* **URL Scheme Validation**: Strictly enforces `http://` and `https://` schemes to prevent Server-Side Request Forgery (SSRF) and open redirect vulnerabilities.
* **Security Headers**: Injecting `X-Content-Type-Options: nosniff` and `X-Frame-Options: DENY` on API HTTP responses.
* **CORS Policy**: Configured middleware allowing safe cross-origin requests for API consumers and interactive documentation.
* **Context-Aware Redirects**: Inspects `Referer` and `Accept` headers to return JSON payloads for Swagger UI requests (preventing browser `Failed to fetch` errors) while executing standard `302 Found` redirects for native browser navigation.

---

## API Documentation

### 1. URL Shortener Service

#### `POST /api/shorten`
Generates a unique short code mapping for a target long URL.
* **Request Body**:
  ```json
  {
    "longUrl": "[https://www.google.com](https://www.google.com)",
    "customAlias": "my-custom-code" 
  }