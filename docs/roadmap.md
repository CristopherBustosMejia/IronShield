# Roadmap

## Phase 1 - Foundation

* [x] Crear estructura de la solución.
* [x] Definir modelos principales.
* [x] Definir formato `.iron`.
* [x] Implementar sistema de bloques.
* [x] Implementar IronContainerReader.
* [x] Implementar IronContainerWriter.
* [x] Implementar serialización binaria de bloques.
* [x] Implementar SHA-256.
* [x] Implementar Argon2id.
* [x] Implementar AES-256-GCM.

## Phase 2 - Core Services

* [x] Abstracción de fuentes de datos (`IDataSource`, `IIronBlockDataFactory`).
* [x] Proveedores de origen: archivo, directorio (ZIP embebido), comprimido (GZip).
* [x] Factoría de datos para construcción de bloques.
* [x] Servicios de Protection / Unprotection separados.
* [x] Fachada `IronShieldService` que compone ambos.
* [x] Pipeline Protect completo (datos → `.iron` cifrado).
* [x] Pipeline Unprotect completo (`.iron` → datos descifrados).
* [ ] Servicio de verificación de integridad.
* [ ] Gestión de errores y excepciones unificada.

## Phase 3 - CLI ✅

* [x] Comando `protect` (soporta archivos y directorios).
* [x] Comando `unprotect` (restaura archivos y directorios a ZIP).
* [x] Sistema.CommandLine 2.0.9 + Spectre.Console.
* [x] Output moderno con spinners, colores, errores user-friendly.
* [x] Password oculta en modo interactivo.
* [x] 8 tests de CLI (unit + integración end-to-end).
* [x] Smoke test script para testers (`tests/CLI/test-scenarios.sh`).
* [ ] Comando `inspect` (metadatos sin descifrar).
* [ ] Comando `version` semántico.
* [ ] Logging / auditoría.

## Phase 4 - Secure Sharing

* [ ] Compartición segura de archivos.
* [ ] Enlaces temporales.
* [ ] Flujo cliente-servidor.

## Phase 5 - GUI

* [ ] Aplicación Avalonia UI.
* [ ] Exploración de archivos.
* [ ] Arrastrar y soltar.
* [ ] Integración con las funciones de la CLI.
