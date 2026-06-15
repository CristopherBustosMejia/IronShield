# Iron File Format Specification

## Status

Draft

## Format Version

```text
1
```

## File Extension

```text
.iron
```

---

# Objectives

The Iron File Format (IFF) is the official container format used by IronShield.

Its goals are:

* Protect sensitive files using strong cryptography.
* Allow metadata inspection without requiring a password.
* Support integrity verification.
* Enable secure file sharing.
* Remain extensible without breaking existing files.
* Support future features such as digital signatures and audit trails.

---

# Design Principles

## Extensibility

New capabilities must be introduced through new block types.

Existing block definitions must never change their meaning.

---

## Forward Compatibility

Readers must ignore unknown block types.

This allows newer files to remain partially readable by older IronShield versions.

---

## Separation of Concerns

Public information must remain separate from encrypted information.

Metadata intended for inspection should not require a password.

Sensitive information must remain protected.

---

## Stable Format

The Iron File Format is designed to evolve by adding new blocks.

Existing block identifiers must remain reserved permanently once released.

---

# File Structure

An Iron file consists of a file header followed by a collection of blocks.

```text
FILE_HEADER

BLOCK_1
BLOCK_2
BLOCK_3
...
BLOCK_N
```

---

# File Header

The file header is always located at the beginning of the file.

## Structure

```text
MAGIC
VERSION
BLOCK_COUNT
```

| Field       | Type         | Size    |
| ----------- | ------------ | ------- |
| Magic       | ASCII String | 4 bytes |
| Version     | Byte         | 1 byte  |
| Block Count | Int32        | 4 bytes |

Total header size:

```text
9 bytes
```

---

## Magic

Constant value:

```text
IRON
```

Used to identify a valid Iron file.

---

## Version

Represents the Iron File Format version used by the file.

Current version:

```text
1
```

---

## Block Count

Number of blocks contained in the file.

---

# Binary Encoding

All integer values in the Iron File Format use:

```text
Little Endian
```

Unless explicitly stated otherwise.

---

# Block Structure

Each block follows the same binary structure.

```text
BLOCK_TYPE
IS_ENCRYPTED
BLOCK_LENGTH
BLOCK_DATA
```

| Field        | Type     | Size    |
| ------------ | -------- | ------- |
| Block Type   | Byte     | 1 byte  |
| Is Encrypted | Byte     | 1 byte  |
| Block Length | Int32    | 4 bytes |
| Block Data   | Variable | N bytes |

---

## Block Type

Identifies the block content.

Examples:

```text
1 = PublicMetadata
2 = IntegrityData
3 = EncryptionInfo
4 = EncryptedPayload
```

---

## Is Encrypted

Indicates whether the block payload is encrypted.

Values:

```text
0 = false
1 = true
```

---

## Block Length

Size of the block payload in bytes.

Does not include the block header.

---

## Block Data

Serialized block content.

The serialization process is:

```text
Object
↓
Serialize
↓
Byte[]
↓
Encrypt (optional)
↓
Block Data
```

# User Experience Goals

The following command should be possible without a password:

```bash
ironshield info secrets.iron
```

Example output:

```text
Original File : secrets.env
Size          : 2.4 KB
Created       : 2026-06-05
Created By    : crisred

Encryption    : AES-256-GCM
KDF           : Argon2id
```

---

# Compatibility Rules

Readers must support all released Iron File Format versions that they explicitly declare compatibility with.

Future versions must preserve the meaning of existing block identifiers.

Unknown block types must be ignored.

---

## Future Versions

Future versions may introduce new block types.

Existing block types must preserve their original meaning.

Readers must ignore unknown block types.

---

## Compatibility Guarantee

The following block identifiers are permanently reserved once defined:

| Id | Name             |
| -- | ---------------- |
| 1  | PublicMetadata   |
| 2  | IntegrityData    |
| 3  | EncryptionInfo   |
| 4  | FileContent      |

These identifiers must never be reused for different purposes.

---

# Reserved Future Block Types

| Id | Purpose          |
| -- | ---------------- |
| 10 | DigitalSignature |
| 11 | PublicKey        |
| 12 | AuditTrail       |

These blocks are reserved for future versions and are not defined in Version 1.
