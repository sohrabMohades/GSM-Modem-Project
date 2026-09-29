# Project context
- System: control and management of up to ~100 GSM modems concurrently
- Stack: C# [نسخه]، .NET [نسخه]، [نوع پروژه: Console/Service/WPF/...]
- Modem communication: [Serial/COM، USB، ...] using [System.IO.Ports/کتابخانه‌ی دیگر]
- Project is ~60% implemented; existing architecture must be respected

# Rules
- Use async/await for all I/O; never block with .Result or .Wait()
- Every I/O operation takes a CancellationToken and has a timeout
- Each modem is isolated: failure of one modem must not affect others
- State shared between modems must be explicitly thread-safe
- Do not change public APIs or existing class names unless I ask
- Keep changes minimal; do not refactor unrelated code
- Logging: [کتابخانه‌ی لاگ]; never log SIM PINs or phone numbers

# Testing
- Framework: [xUnit/NUnit/MSTest]
- Build: dotnet build
- Test: dotnet test

# Response style
- Explain briefly what you changed and why
- If something is ambiguous, ask before generating code