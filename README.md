# IronShield

IronShield es una herramienta CLI multiplataforma para proteger archivos sensibles mediante criptografía moderna (AES-256-GCM + Argon2id + SHA-256) y un formato de archivo propio extensible (`.iron`).

## Estado

MVP funcional. 116 tests pasando. Listo para pruebas externas.

## Uso rápido

```bash
# Proteger un archivo
dotnet run --project src/IronShield.Cli -- protect documento.pdf -p "miclave" -o documento.pdf.iron

# Proteger un directorio (se comprime a ZIP internamente)
dotnet run --project src/IronShield.Cli -- protect ./carpeta -p "miclave" -o carpeta.iron

# Restaurar
dotnet run --project src/IronShield.Cli -- unprotect carpeta.iron -p "miclave" -o carpeta.zip
```

## Comandos CLI

| Comando | Descripción |
|---|---|
| `protect <path>` | Cifra un archivo o directorio |
| `unprotect <path>` | Descifra un archivo `.iron` |

### Opciones compartidas

| Flag | Descripción |
|---|---|
| `-o, --output` | Ruta de salida |
| `-p, --password` | Contraseña (omitir para modo interactivo) |
| `--overwrite` | Sobrescribir archivo existente |

## Documentación

La documentación completa está en [`docs/`](docs/):

- [`docs/arquitecture.md`](docs/arquitecture.md)
- [`docs/roadmap.md`](docs/roadmap.md)
- [`docs/project-context.md`](docs/project-context.md)
- [`docs/file-format/`](docs/file-format/)
- [`docs/cryptography/`](docs/cryptography/)
- [`docs/development/testing-guide.md`](docs/development/testing-guide.md)

## Requisitos

- .NET 10 SDK
- Linux, macOS o Windows

## Tests automatizados

```bash
dotnet test
```

## Smoke test manual

```bash
bash tests/CLI/test-scenarios.sh
```
