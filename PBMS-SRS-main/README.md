# Parking Building Management System (PBMS)

Software Requirements Specification (SRS) repository for the Parking Building Management System project.

---

## Overview

Parking Building Management System (PBMS) is a parking facility management solution designed to support:

* Vehicle check-in and check-out
* Parking slot allocation
* Vehicle booking
* Monthly card management
* Payment processing
* Revenue monitoring
* Operational management

This repository contains the project's Software Requirements Specification (SRS) and supporting analysis documents.

---

## Main Documents

| Document                      | Description                                     |
| ------------------------------| ----------------------------------------------- |
| `PBMS_Feature_Actor_Based.md` | Features according to SRS document              |
| `PBMS_SRS_Document.md`        | Complete generated SRS document                 |
| `sections/`                   | Source sections used to build the SRS           |
| `build-srs.bat`               | Script for generating the complete SRS document |

---

## Repository Structure

```text
.
├── README.md
├── PBMS_Feature_Actor_Based.md
├── PBMS_SRS_Document.md
├── build-srs.bat
└── sections/
    ├── 00_Title_And_TOC.md
    ├── 01_Introduction.md
    ├── 02_Overall_Description.md
    ├── 03_Stakeholders_Actors.md
    ├── 04_Business_Context.md
    ├── 05_Functional_Requirements.md
    ├── 06_Business_Rules.md
    ├── 07_Finalized_Policy_Decisions.md
    └── 08_Concept_Entity_Physical_Model.md
```

---

## SRS Build Process

The complete SRS document is generated from the files located in the `sections/` directory.

### Update Workflow

1. Edit the appropriate file inside `sections/`
2. Run:

```bash
build-srs.bat
```

3. Verify the generated file:

```text
PBMS_SRS_Document.md
```

4. Commit and push changes:

```bash
git add .
git commit -m "Update SRS"
git push
```

---

## Important Note

`PBMS_SRS_Document.md` is generated from the source files located in `sections/`.

Do not edit the generated document directly.

All modifications should be made inside the `sections/` directory and then rebuilt using `build-srs.bat`.

---
<!-- Author: Nguyen Hoang Gia Thai -->
