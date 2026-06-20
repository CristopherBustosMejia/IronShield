# Testing Guide

Esta guía describe cómo probar manualmente la CLI de IronShield.

## Prerequisitos

- .NET 10 SDK
- Git
- Linux, macOS o Windows

## Build

```bash
dotnet build
```

## Ejecutar tests automatizados

```bash
# Todos los tests
dotnet test

# Solo tests de la CLI
dotnet test tests/IronShield.Cli.Tests

# Solo tests de Storage
dotnet test tests/IronShield.Storage.Tests

# Solo tests de Cryptography
dotnet test tests/IronShield.Cryptography.Tests
```

## Smoke test manual

Hay un script que ejecuta todos los escenarios básicos:

```bash
bash tests/CLI/test-scenarios.sh
```

## CLI: Escenarios de prueba

### 1. Ayuda

```bash
dotnet run --project src/IronShield.Cli -- --help
dotnet run --project src/IronShield.Cli -- protect --help
dotnet run --project src/IronShield.Cli -- unprotect --help
```

### 2. Proteger un archivo

```bash
echo "Mi contenido secreto" > /tmp/test.txt
dotnet run --project src/IronShield.Cli -- protect /tmp/test.txt \
    --password "MiClaveSegura123" \
    --output /tmp/test.txt.iron
```

O sin flags (pedirá password interactivamente):

```bash
dotnet run --project src/IronShield.Cli -- protect /tmp/test.txt
```

### 3. Restaurar un archivo

```bash
dotnet run --project src/IronShield.Cli -- unprotect /tmp/test.txt.iron \
    --password "MiClaveSegura123" \
    --output /tmp/restored.txt
```

Verificar que el contenido coincide:

```bash
diff /tmp/test.txt /tmp/restored.txt && echo "OK"
```

### 4. Proteger un directorio

```bash
mkdir -p /tmp/misdocs
echo "doc1" > /tmp/misdocs/doc1.txt
echo "doc2" > /tmp/misdocs/doc2.txt

dotnet run --project src/IronShield.Cli -- protect /tmp/misdocs \
    --password "ClaveDir" \
    --output /tmp/misdocs.iron
```

### 5. Restaurar un directorio

```bash
dotnet run --project src/IronShield.Cli -- unprotect /tmp/misdocs.iron \
    --password "ClaveDir" \
    --output /tmp/restored-dir.zip

unzip /tmp/restored-dir.zip -d /tmp/extracted
diff /tmp/misdocs/doc1.txt /tmp/extracted/doc1.txt && echo "OK"
diff /tmp/misdocs/doc2.txt /tmp/extracted/doc2.txt && echo "OK"
```

### 6. Errores esperados

**Password incorrecta:**

```bash
dotnet run --project src/IronShield.Cli -- unprotect /tmp/test.txt.iron \
    --password "WrongPassword"
# Debe mostrar: ERROR: Incorrect password. Decryption failed.
```

**Archivo no encontrado:**

```bash
dotnet run --project src/IronShield.Cli -- unprotect /tmp/no-existe.iron \
    --password "x"
# Debe mostrar: ERROR: File not found: /tmp/no-existe.iron
```

**Ruta no encontrada (protect):**

```bash
dotnet run --project src/IronShield.Cli -- protect /ruta/inexistente \
    --password "x"
# Debe mostrar: ERROR: Path not found: /ruta/inexistente
```

## Casos borde a probar

### Archivos de distinto tamaño
- Archivo vacío (0 bytes)
- Archivo pequeño (1 KB)
- Archivo mediano (1 MB)
- Archivo grande (100 MB+)

### Nombres de archivo con caracteres especiales
- Espacios (`mi archivo.txt`)
- Acentos (`informe_2024_ñ.txt`)
- Paréntesis (`datos (final).csv`)
- Puntos múltiples (`backup.2024.01.01.tar.gz.iron`)

### Directorios con subdirectorios
```bash
mkdir -p /tmp/nested/a/b/c
echo "deep" > /tmp/nested/a/b/c/deep.txt
dotnet run --project src/IronShield.Cli -- protect /tmp/nested -p "x" -o /tmp/nested.iron --overwrite
dotnet run --project src/IronShield.Cli -- unprotect /tmp/nested.iron -p "x" -o /tmp/out.zip --overwrite
unzip -l /tmp/out.zip | grep deep.txt
```

### Flags combinados
- `--overwrite` con archivo existente
- `-o` con y sin directorio intermedio
- `-p` vacía (debe rechazar)
- Sin password (debe pedir interactivamente)

## Notas para testers

- La CLI no requiere permisos especiales más allá de lectura/escritura en los paths usados.
- Los archivos `.iron` generados no son legibles sin la password correcta.
- El formato `.iron` actual es la versión inicial (v1). Archivos protegidos ahora deberían seguir siendo descifrables en versiones futuras.
- La opción `--creator` está definida en el parser pero aún no es funcional (reservada para uso futuro).
