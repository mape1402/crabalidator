# Crabalidator

**Crabalidator** is a high-performance .NET validation library intended to replace FluentValidation-style application validators with DynaBee-generated runtime validators.

The project is in its foundation phase. The architecture and roadmap live in:

- [Architecture](docs/ARCHITECTURE.md)
- [Roadmap](docs/ROADMAP.md)

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

Run the basic sample:

```bash
dotnet run --project samples/Crabalidator.Samples.Basic
```
