# Pedagogy Notes

Append-only log of teaching observations and deliberate teaching decisions made while building this reference solution. Future agent sessions read this to understand *why* things are the way they are (or deliberately aren't).

---

## 2026-04-22 — Culture-sensitive parsing deliberately skipped

`DateTime.Parse` and `decimal.Parse` use the current machine culture by default. A file written on a machine with one locale can fail to parse on a machine with a different one (e.g. US uses `.` as the decimal separator; fr-CA uses `,`). The robust solution is `CultureInfo.InvariantCulture` + `DateTime.TryParseExact` with a fixed format, plus `decimal.TryParse` with `NumberStyles.Any` and invariant culture.

**Decision:** do not teach this on this project.

**Reasons:**
- The app runs on a single machine (the student's laptop at home, then the competition machine). Cross-locale round-tripping isn't actually exercised.
- Globalization is a large standalone sub-topic — cultures, `NumberStyles`, format specifiers, round-trip safety, exact-vs-standard parsing. Introducing it mid-project derails the core lessons (manual parsing, class boundaries, file I/O).
- The student is slow; cognitive budget is finite. Every sub-topic imported costs time that belongs to the main arc.

**Mitigation:** the final `README.md` must include a short "Known limitations" note stating that the app assumes a consistent machine locale.

**Where to revisit:** if the student stays with C# past this project, treat globalization as its own short lesson using a simpler example (one date round-tripped through a file) before re-opening `Animal.cs`.

**Guidance for future agent sessions:** do not retrofit invariant-culture parsing into `Animal.cs` unless the user explicitly asks for it. If the topic comes up, point at this note.

---

## General principle that emerged from this decision

When a shortcut would introduce a large new sub-topic the student isn't ready for, **document the shortcut in `PEDAGOGY.md` and the `README.md`, and move on**. Correctness tangents are the enemy of a slow learner making real progress. The workbook's job is to teach the core arc cleanly, not to produce production-grade code.
